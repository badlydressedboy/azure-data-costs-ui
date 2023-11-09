using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Services.AppAuthentication;
using System.Net.Http;
using System.Windows.Markup;
using DataEstateOverview.Models;
using System.Net.Http.Json;
using DataEstateOverview.Models.Rest;
using System.Diagnostics.Metrics;
using System.Windows.Documents;
using DataEstateOverview.Models.UI;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using DbMeta.Ui.Wpf.Models.Rest;
using System.Globalization;
using System.Security.Policy;
using CsvHelper;
using Azure.Costs.Ui.Wpf.Models.Rest;
using Azure.Costs.Ui.Wpf;

namespace DataEstateOverview
{
    public static class APIAccess
    {
        public static MyHttpClient HttpClient;        
        private static string _accessToken;
        public static int CostDays { get; set; } = 30;

        public static HttpClient GetHttpClient(string baseAddress, int timeoutSecs)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
                    ,
                Timeout = TimeSpan.FromSeconds(timeoutSecs)
            };

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);

            return httpClient;
        }
        private static async Task<HttpResponseMessage> GetHttpClientAsync(string url)
        {
            if (HttpClient == null)
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                _accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/");
                HttpClient = new MyHttpClient("https://management.azure.com/", 30, _accessToken);
            }
            HttpResponseMessage r = await HttpClient.GetAsync(url);
            return r;
        }
        public static async Task<string?> TestLogin()
        {
            try
            {
                
                //var httpClient = GetHttpClient("https://management.azure.com/subscriptions/", 5);
                HttpResponseMessage response = await GetHttpClientAsync("https://management.azure.com/subscriptions?api-version=2020-01-01");// httpClient.GetAsync("https://management.azure.com/subscriptions?api-version=2020-01-01");
                if (!response.IsSuccessStatusCode)
                {
                    return response.ReasonPhrase;
                }                
            }
            catch (Exception ex)
            {
                return ex.Message;                
            }
            return null;
        }

        public static async Task<List<Subscription>> GetSubscriptions()
        {
            try
            {
                //AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                //_accessToken = azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/").Result;
                //Debug.WriteLine(_accessToken);

                //_httpClient = new HttpClient
                //{
                //    BaseAddress = new Uri("https://management.azure.com/subscriptions/")
                //    ,
                //    Timeout = TimeSpan.FromSeconds(30)
                //};

                //_httpClient.DefaultRequestHeaders.Remove("Authorization");
                //_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);

                //HttpResponseMessage response = await GetHttpClientAsync("https://management.azure.com/subscriptions?api-version=2020-01-01");

                HttpResponseMessage response = await GetHttpClientAsync("https://management.azure.com/subscriptions?api-version=2020-01-01");// httpClient.GetAsync("https://management.azure.com/subscriptions?api-version=2020-01-01");

                var json = await response.Content.ReadAsStringAsync();
                RootSubscription subscriptions = await response?.Content?.ReadFromJsonAsync<RootSubscription>();
                return subscriptions.value;
                
            }
            catch (Exception ex)
            {
                // subscription scope usage bug: https://learn.microsoft.com/en-us/answers/questions/795590/az-consumption-usage-list-errors-out-with-please-u.html
                Debug.WriteLine(ex);
            }
            return null;
        }
        public static async Task RefreshSubscriptionCosts(Subscription subscription)
        {
            /* rest api and also sdk
             * sdk currently (october 2022) has bug which stops consumption requests
             * api is only alternative
             * 
             */

            var sw = Stopwatch.StartNew();    

            try
            {
                /* How this authentication works:
                 * https://learn.microsoft.com/en-gb/dotnet/api/overview/azure/service-to-service-authentication?view=azure-dotnet
                 * 
                 * For local development, AzureServiceTokenProvider fetches tokens using Visual Studio, Azure command-line interface (CLI), or Azure AD Integrated Authentication. 
                 * Each option is tried sequentially and the library uses the first option that succeeds. 
                 * If no option works, an AzureServiceTokenProviderException exception is thrown with detailed information.
                 * 
                 * If you have cloned the repo to run this code you have visual studio so likely to work for you
                 * If deploying/running on non visual studio computers then suing Azure CLI:
                 *  - az login
                 *  - az account get-access-token --resource https://vault.azure.net
                 */

            
                try
                {
                    Task[] tasks = new Task[7];
                    tasks[0] = Task.Run(async () =>
                    {
                        await GetSqlServers(subscription);      
                    });
                    tasks[1] = Task.Run(async () =>
                    {
                        await GetSubscriptionCosts(subscription, CostRequestType.SqlDatabase);
                    });
                    tasks[2] = Task.Run(async () =>
                    {
                        //await GetDataFactories(subscription);
                    });
                    tasks[3] = Task.Run(async () =>
                    {
                        //await GetStorageAccounts(subscription);
                    });
                    tasks[4] = Task.Run(async () =>
                    {
                        //await GetVirtualNetworks(subscription);
                    });
                    tasks[5] = Task.Run(async () =>
                    {
                        //await GetVirtualMachines(subscription);
                    });
                    tasks[6] = Task.Run(async () =>
                    {
                        //await GetPurviews(subscription);
                    });



                    await Task.WhenAll(tasks);

                    // map costs to resources


                }
                catch (AggregateException ae)
                {
                    throw ae.Flatten();//https://msdn.microsoft.com/en-us/library/dd537614(v=vs.110).aspx
                }                
            }
            catch(Exception ex)
            {
                // subscription scope usage bug: https://learn.microsoft.com/en-us/answers/questions/795590/az-consumption-usage-list-errors-out-with-please-u.html
                Debug.WriteLine(ex);
            }
            Debug.WriteLine($"Finished subscription in {sw.Elapsed.TotalSeconds} seconds");
        }
        public static async Task GetSqlServers(Subscription subscription)
        {
            try
            {

                string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/resources?$filter=resourceType eq 'Microsoft.sql/servers'&$expand=resourceGroup,createdTime,changedTime&$top=1000&api-version=2021-04-01";
                StringContent queryString = new StringContent("api-version=2021-04-01");
                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                // get location and name properties from list of servers
                RootRestSqlServer servers = await response.Content.ReadFromJsonAsync<RootRestSqlServer>();
                if (servers.value == null) return;
                
                foreach (var restSql in servers.value)
                {
                    string rg = restSql.id.Substring(restSql.id.IndexOf("resourceGroup") + 15);
                    restSql.resourceGroup = rg.Substring(0, rg.IndexOf("/"));

                    string sub = restSql.id.Substring(restSql.id.IndexOf("subscription") + 14);
                    restSql.Subscription = subscription;// = sub.Substring(0, sub.IndexOf("/"));

                    restSql.AzServer = new Models.SQL.AzServer(restSql.name);
                }
                subscription.SqlServers = servers.value.ToList();
                subscription.HasEverGotSqlServers = true;

                await Parallel.ForEachAsync(subscription.SqlServers
                    , new ParallelOptions() { MaxDegreeOfParallelism = 50 }
                    , async (server, y) =>
                {
                    await GetSqlElasticPools(server);
                    await GetSqlServerDatabases(server);
                });
            }catch(Exception ex)
            {

            }

            //Debug.WriteLine("fin get sql servers");
        }

        private static async Task GetSqlElasticPools(RestSqlServer sqlServer)
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions/{sqlServer.Subscription.subscriptionId}/resourceGroups/{sqlServer.resourceGroup}/providers/Microsoft.Sql/servers/{sqlServer.name}/elasticpools?api-version=2021-02-01-preview";
                StringContent queryString = new StringContent("api-version=2021-04-01");
                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                // get location and name properties from list of servers

                


                RootElasticPool pools = await response.Content.ReadFromJsonAsync<RootElasticPool>();
                if (pools.value == null) return;

                //if(json.Length > 14 || sqlServer.name =="octopus-sql-server-staging")
                //{
                //    Debug.WriteLine("got elastic");
                //}
                sqlServer.ElasticPools.Clear(); 
                foreach (var restPool in pools.value)
                {
                    sqlServer.ElasticPools.Add(restPool);
                    //string rg = restSql.id.Substring(restSql.id.IndexOf("resourceGroup") + 15);
                    //restSql.resourceGroup = rg.Substring(0, rg.IndexOf("/"));

                    //string sub = restSql.id.Substring(restSql.id.IndexOf("subscription") + 14);
                    //restSql.Subscription = subscription;// = sub.Substring(0, sub.IndexOf("/"));

                    //restSql.AzServer = new Models.SQL.AzServer(restSql.name);
                }
                //subscription.SqlServers = servers.value.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            //Debug.WriteLine("fin get elastic pools");
        }

        private static async Task GetSqlServerDatabases(RestSqlServer sqlServer)
        {
            //if (sqlServer.name != "octopus-sql-server-staging") return;

            List<RestSqlDb> returnList = new List<RestSqlDb>();
            sqlServer.Dbs.Clear();

            try
            {
                string dbUrl = $"https://management.azure.com/subscriptions/{sqlServer.Subscription.subscriptionId}/resourceGroups/{sqlServer.resourceGroup}/providers/Microsoft.Sql/servers/{sqlServer.name}/databases?api-version=2021-02-01-preview";              
                var response = await GetHttpClientAsync(dbUrl);
                var json = await response.Content.ReadAsStringAsync();
                RootRestSqlDb databases = await response?.Content?.ReadFromJsonAsync<RootRestSqlDb>();                

                foreach (var db in databases.value.Where(x => x.name != "master"))
                {
                    db.serverName = sqlServer.name;
                    db.Subscription = sqlServer.Subscription;
                    db.resourceGroup = sqlServer.resourceGroup;

                    db.AzDB.DatabaseName = db.name;
                    db.AzDB.SetParent(sqlServer.AzServer);

                    if (db.properties.elasticPoolId != null)
                    {                        
                        foreach(var ep in sqlServer.ElasticPools)
                        {                            
                            if (db.properties.elasticPoolId.Contains(ep.name))
                            {                                
                                db.properties.elasticPoolName = ep.name;
                                db.ElasticPool = ep;
                            }
                        }
                    }

                    sqlServer.Dbs.Add(db);
                }

                // add dbs to elastic pools
                foreach (var pool in sqlServer.ElasticPools)
                {
                    foreach (var db in sqlServer.Dbs)
                    {
                        if(db.ElasticPool != null && db.ElasticPool.name == pool.name)
                        {
                            pool.dbList.Add(db);
                            db.properties.currentServiceObjectiveName += $" {pool.sku.name} {pool.sku.capacity}";
                        }
                    }
                }

                try
                {
                  
                    Task[] tasks = new Task[6];
                    tasks[0] = Task.Run(async () =>
                    {
                        await Task.WhenAll(sqlServer.Dbs.Select(i => GetDbMetrics(i, 2)));
                    });
                    tasks[1] = Task.Run(async () =>
                    {
                        await Task.WhenAll(sqlServer.Dbs.Select(i => GetDbLTRs(i)));
                    });
                    tasks[2] = Task.Run(async () =>
                    {
                        //await Task.WhenAll(sqlServer.Dbs.Select(i => GetDbServiceTierAdvisors(i)));
                    });
                    tasks[3] = Task.Run(async () =>
                    {
                        await Task.WhenAll(sqlServer.Dbs.Select(i => GetDbRecommendedActions(i)));
                    });
                    tasks[4] = Task.Run(async () =>
                    {
                        await Task.WhenAll(sqlServer.Dbs.Select(i => GetDbLatestVulnerabilityAssesment(i)));
                    });
                    tasks[5] = Task.Run(async () =>
                    {
                        await Task.WhenAll(sqlServer.Dbs.Select(i => GetDbUsages(i)));
                    });

                    await Task.WhenAll(tasks);

                }
                catch (AggregateException ae)
                {
                    throw ae.Flatten();//https://msdn.microsoft.com/en-us/library/dd537614(v=vs.110).aspx
                }


                //Debug.WriteLine($"finished getting {sqlServer.name} dbs ({sqlServer.Dbs.Count})");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }            
        }

        public static async Task RefreshRestDb(RestSqlDb sqlDb)
        {
            Task[] tasks = new Task[5];

            tasks[0] = Task.Run(async () =>
            {
                GetDbRecommendedActions(sqlDb);
            });
            tasks[1] = Task.Run(async () =>
            {
                GetDbLatestVulnerabilityAssesment(sqlDb);
            });
            tasks[2] = Task.Run(async () =>
            {
                GetDbUsages(sqlDb);
            });
            tasks[3] = Task.Run(async () =>
            {
                GetDbMetrics(sqlDb);
            });
            tasks[4] = Task.Run(async () =>
            {
                GetDbLTRs(sqlDb);
            });



            await Task.WhenAll(tasks);
            Debug.WriteLine("Complete refresh db for " + sqlDb.name);

            // await Task.WhenAll(_restSqlDbList.Select(i => GetCosts(i)));
            //await GetDbRecommendedActions(sqlDb);
            //await GetDbLatestVulnerabilityAssesment(sqlDb);
            //await GetDbUsages(sqlDb);
            //await GetDbMetrics(sqlDb);
            //await GetDbLTRs(sqlDb);
        }

        // short retention policies
        // https://learn.microsoft.com/en-us/rest/api/sql/2021-11-01/backup-short-term-retention-policies

        // maintenance windows
        // https://learn.microsoft.com/en-us/rest/api/sql/2021-11-01/maintenance-windows/get?tabs=HTTP

        private static async Task GetDbUsages(RestSqlDb sqlDb)
        {
            //Debug.WriteLine("usage 1");
            try
            {
                string url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name}/usages?api-version=2021-11-01";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var usages = await response?.Content?.ReadFromJsonAsync<DBUsageRoot>();
                //Debug.WriteLine("usage 2");
                if (usages?.value != null)
                {
                    foreach (var a in usages.value)
                    {
                        if(a.name == "database_size")
                        {
                            sqlDb.currentDbSizeBytes = a.properties.currentValue;
                            sqlDb.currentDbSizeLimitBytes = a.properties.limit;
                        }
                        if (a.name == "database_allocated_size")
                        {
                            sqlDb.currentAllocatedDbSizeBytes = a.properties.currentValue;
                            sqlDb.currentAllocatedDbSizeLimitBytes = a.properties.limit;
                        }
                    }

                    //Debug.WriteLine("usage 3");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static async Task GetDbRecommendedActions(RestSqlDb sqlDb)
        {
            try
            {
                if (sqlDb.IsSynapse) return; // dwh type db not supported

                string url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name}/advisors?$expand=recommendedActions&api-version=2021-11-01";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Recommendations error for {sqlDb.serverName}.{sqlDb.name}; {response.ReasonPhrase}");
                    sqlDb.RecommendationsError = json;
                    return;
                }
                var advisors = await response?.Content?.ReadFromJsonAsync<List<Advisor>>();
                                
                if (advisors != null)
                {
                    foreach (var a in advisors)
                    {
                        sqlDb.AdvisorRecommendationCount += a.properties.recommendedActions.Count;

                        foreach (var ra in a.properties.recommendedActions) {
                            sqlDb.advisorRecommendationDetails += ra.properties.implementationDetails.script + "\n\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static async Task GetDbLatestVulnerabilityAssesment(RestSqlDb sqlDb)
        {

            try
            {
                string url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name.ToLower()}/vulnerabilityAssessments/default/scans?api-version=2020-11-01-preview";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (json.ToLower().Contains("exist"))
                    {
                        // is no settings for scans it creates a 'doesnt exist' error we dont really care about
                        sqlDb.VulnerabilityScanError = ""; // could output something?
                    }
                    else
                    {
                        // unknown error
                        sqlDb.VulnerabilityScanError = json;
                        Debug.WriteLine($"Vuln Assessment Error for {sqlDb.serverName}.{sqlDb.name}: {response.StatusCode}, {response.ReasonPhrase}");
                    }                    
                    
                    return;
                }
                var scans = await response?.Content?.ReadFromJsonAsync<RootVulnerabilityScans>();

                if (scans?.value != null)
                {
                    var latestScan = scans.value.OrderByDescending(x => x.properties.endTime).First();
                    if (latestScan == null) return;
                    sqlDb.latestVulnerabilityScanProperties = latestScan.properties;
                   
                    //Debug.WriteLine("ltr null");
                    //sqlDb.lTRPolicyProperties = ltr?.properties;
                }
                else
                {
                    //Debug.WriteLine("ltr null");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private static async Task GetDbLTRs(RestSqlDb sqlDb)
        {
            
            try
            {
                string url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name}/backupLongTermRetentionPolicies/default?api-version=2020-11-01-preview";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var ltr = await response?.Content?.ReadFromJsonAsync<LTRPolicy>();

                if (ltr?.properties != null)
                {
                    sqlDb.lTRPolicyProperties = ltr?.properties;
                }
                else
                {
                    //Debug.WriteLine("ltr null");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
        public static async Task GetDbMetrics(RestSqlDb sqlDb, int? minutes = null)
        {

            try
            {
                /*cpu_percent
                physical_data_read_percent
                log_write_percent
                dtu_consumption_percent
                storage - data space used
                deadlock
                storage_percent
                workers_percent
                sessions_percent
                sessions_count
                dtu_limit
                dtu_used
                sqlserver_process_core_percent
                sqlserver_process_memory_percent
                tempdb_data_size
                tempdb_log_size
                tempdb_log_used_percent
                allocated_data_storage
                 * 
                 * 
                 * timespan: 2022-11-15T14:42:53Z/2022-11-15T15:42:53Z
                 */

                string timeGrainParam = "&interval=PT1M"; // PT1H, PT30M, PT1M, P1D

                if(minutes == null) // use sqlDb.MetricsHistoryDays
                {
                    minutes = sqlDb.MetricsHistoryDays * 1440;
                }
                int mins = (int)minutes;

                if(mins > 120) timeGrainParam = "&interval=PT5M";
                if (mins > 360) timeGrainParam = "&interval=PT15M";
                if (mins > 720) timeGrainParam = "&interval=PT30M";
                if (mins > 1440) timeGrainParam = "&interval=PT1H";
                if (mins > 4320) timeGrainParam = "&interval=PT3H";
                if (mins > 8640) timeGrainParam = "&interval=PT6H";

                sqlDb.MetricsHistoryMinutes = mins;

                string timeFrom = DateTime.UtcNow.AddMinutes(-1* mins).ToString("s") + "Z";
                string timeTo = DateTime.UtcNow.ToString("s") + "Z";
                
                if (sqlDb.IsElaticPoolMember)
                {
                    // get elastic pool costs if required
                    if (sqlDb.ElasticPool.PerformanceMetricSeries.Count > -1)
                    {
                        string poolUrl = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/elasticPools/{sqlDb.ElasticPool.name}/providers/Microsoft.Insights/metrics?aggregation=average,maximum{timeGrainParam}&timespan={timeFrom}/{timeTo}&metricnames=sessions_count,cpu_percent&api-version=2021-05-01";

                        var res = await GetHttpClientAsync(poolUrl);

                        if (!res.IsSuccessStatusCode)
                        {
                            Debug.WriteLine($"Failed pool metrics!: {res.ReasonPhrase}");
                            sqlDb.MetricsErrorMessage = $"Failed pool metrics!: {res.ReasonPhrase}";
                        }
                        var poolJson = await res.Content.ReadAsStringAsync();
                        var poolMetrics = await res?.Content?.ReadFromJsonAsync<RootMetric>();

                        if (poolMetrics?.value != null)
                        {
                            foreach (var metric in poolMetrics.value)
                            {
                                if (metric.timeseries.Count() == 0) continue;
                                string metricName = metric.name.value;
                                var pool = sqlDb.ElasticPool;

                                try
                                {

                                    switch (metricName)
                                    {
                                        case "dtu_consumption_percent":
                                            pool.PerformanceMetricSeries.Clear();

                                            //sqlDb.MaxDtuUsed = 0;
                                            foreach (var d in metric.timeseries[0].data.OrderByDescending(x => x.timeStamp))
                                            {
                                                pool.PerformanceMetricSeries.Add(d);
                                                if (d.maximum > pool.MaxDtuUsed) pool.MaxDtuUsed = d.maximum;
                                            }

                                            break;
                                        case "cpu_percent":
                                            pool.PerformanceMetricSeries.Clear();

                                            //sqlDb.MaxDtuUsed = 0;
                                            foreach (var d in metric.timeseries[0].data.OrderByDescending(x => x.timeStamp))
                                            {
                                                pool.PerformanceMetricSeries.Add(d);
                                                if (d.maximum > pool.MaxDtuUsed) pool.MaxDtuUsed = d.maximum;
                                            }

                                            break;
                                    }
                                }catch(Exception ex) { 
                                    Debug.WriteLine(ex.ToString()); 
                                }
                            }
                        }
                    }
                }
               
                string url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name}/providers/Microsoft.Insights/metrics?{timeGrainParam}&aggregation=average,maximum&timespan={timeFrom}/{timeTo}&metricnames=physical_data_read_percent,log_write_percent,dtu_consumption_percent,sessions_count,storage,storage_percent,workers_percent,sessions_percent,dtu_limit,dtu_used,sqlserver_process_core_percent,sqlserver_process_memory_percent,tempdb_data_size,tempdb_log_size,tempdb_log_used_percent,allocated_data_storage&api-version=2021-05-01";
                if (sqlDb.IsVCore) 
                {
                    // dont get elastic pool metrics - we are still at db level
                    url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name}/providers/Microsoft.Insights/metrics?aggregation=average,maximum{timeGrainParam}&timespan={timeFrom}/{timeTo}&metricnames=cpu_percent&api-version=2021-05-01";
                }
                if (sqlDb.IsSynapse)
                {
                    // DWU used
                    url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name}/providers/Microsoft.Insights/metrics?aggregation=average,maximum{timeGrainParam}&timespan={timeFrom}/{timeTo}&metricnames=dwu_consumption_percent&api-version=2021-05-01";
                }

                sqlDb.IsRestQueryBusy = true;
                sqlDb.MetricsErrorMessage = "";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Failed metrics!");
                    sqlDb.MetricsErrorMessage = $"Failed metrics!: {response.ReasonPhrase}";
                    sqlDb.IsRestQueryBusy = false;
                    return;
                }
                var json = await response.Content.ReadAsStringAsync();
                var metrics = await response?.Content?.ReadFromJsonAsync<RootMetric>();

                App.Current.Dispatcher.Invoke(new Action(() =>
                {

                    if (metrics?.value != null)
                    {
                        foreach (var metric in metrics.value)
                        {
                            if (metric.timeseries.Count() == 0) continue;

                            var latestAvg = metric.timeseries[0].data[0].average;
                            var latestMax = metric.timeseries[0].data[0].maximum;

                            if (latestMax > 0)
                            {
                                //Debug.WriteLine("Got max for " + metric.name.value);
                            }

                            string metricName = metric.name.value;
                            long outLong;
                            switch (metricName)
                            {
                                case "dtu_consumption_percent":
                                    sqlDb.dtu_consumption_percent = latestAvg;

                                    sqlDb.PerformanceMetricSeries.Clear();

                                    sqlDb.MaxDtuUsed = 0;
                                    foreach (var d in metric.timeseries[0].data.OrderByDescending(x => x.timeStamp))
                                    {
                                        sqlDb.PerformanceMetricSeries.Add(d);
                                        if (d.maximum > sqlDb.MaxDtuUsed) sqlDb.MaxDtuUsed = d.maximum;
                                    }

                                    break;
                                case "cpu_percent":
                                    sqlDb.dtu_consumption_percent = latestAvg;

                                    sqlDb.PerformanceMetricSeries.Clear();

                                    sqlDb.MaxDtuUsed = 0;
                                    foreach (var d in metric.timeseries[0].data.OrderByDescending(x => x.timeStamp))
                                    {
                                        sqlDb.PerformanceMetricSeries.Add(d);
                                        if (d.maximum > sqlDb.MaxDtuUsed) sqlDb.MaxDtuUsed = d.maximum;
                                    }

                                    break;
                                case "dwu_consumption_percent": // DWH
                                    sqlDb.dtu_consumption_percent = latestAvg;

                                    sqlDb.PerformanceMetricSeries.Clear();

                                    sqlDb.MaxDtuUsed = 0;
                                    foreach (var d in metric.timeseries[0].data.OrderByDescending(x => x.timeStamp))
                                    {
                                        sqlDb.PerformanceMetricSeries.Add(d);
                                        if (d.maximum > sqlDb.MaxDtuUsed) sqlDb.MaxDtuUsed = d.maximum;
                                    }

                                    break;


                                case "physical_data_read_percent":
                                    sqlDb.physical_data_read_percent = latestAvg;
                                    break;
                                case "log_write_percent":
                                    sqlDb.log_write_percent = latestAvg;
                                    break;
                                case "storage_percent":
                                    sqlDb.storage_percent = latestAvg;
                                    break;
                                case "workers_percent":
                                    sqlDb.workers_percent = latestAvg;
                                    break;
                                case "sessions_percent":
                                    sqlDb.sessions_percent = latestAvg;
                                    break;
                                case "sqlserver_process_core_percent":
                                    sqlDb.sqlserver_process_core_percent = latestAvg;
                                    break;
                                case "sqlserver_process_memory_percent":
                                    sqlDb.sqlserver_process_memory_percent = latestAvg;
                                    break;

                                case "sessions_count":
                                    if (Int64.TryParse(latestAvg.ToString(), out outLong))
                                    {
                                        sqlDb.sessions_count = outLong;
                                    }
                                    break;
                                case "storage":
                                    if (Int64.TryParse(latestAvg.ToString(), out outLong))
                                    {
                                        sqlDb.storage = outLong;
                                    }
                                    break;
                                case "dtu_limit":
                                    if (Int64.TryParse(latestAvg.ToString(), out outLong))
                                    {
                                        sqlDb.dtu_limit = outLong;
                                    }
                                    break;
                                case "dtu_used":
                                    if (Int64.TryParse(latestAvg.ToString(), out outLong))
                                    {
                                        sqlDb.dtu_used = outLong;
                                    }
                                    break;
                                case "tempdb_data_size":
                                    sqlDb.tempdb_data_size = latestMax;
                                    // always 0
                                    break;
                                case "tempdb_log_size":
                                    sqlDb.tempdb_log_size = latestMax;
                                    // always 0
                                    break;
                                case "tempdb_log_used_percent":
                                    sqlDb.tempdb_log_used_percent = latestMax;
                                    // always 0
                                    break;
                                case "allocated_data_storage":
                                    if (Int64.TryParse(latestAvg.ToString(), out outLong))
                                    {
                                        sqlDb.allocated_data_storage = outLong;
                                    }
                                    break;
                            }
                        }

                        // spend analysis
                        if (minutes > 2)
                        {
                            sqlDb.GotMetricsHistory = true;
                            sqlDb.OverSpendFromMaxPc = 100 - sqlDb.MaxDtuUsed;
                            sqlDb.CalcPotentialSaving();
                        }
                    }
                }));

                //Debug.WriteLine($"finished getting {sqlDb.name} metrics");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
            sqlDb.IsRestQueryBusy = false;
        }

        public static async Task GetVmMetrics(VM vm, int? minutes = null)
        {

            try
            {
             
                string timeGrainParam = "&interval=PT1M"; // PT1H, PT30M, PT1M, P1D

                if (minutes == null) // use sqlDb.MetricsHistoryDays
                {
                    minutes = vm.MetricsHistoryDays * 1440;
                }
                int mins = (int)minutes;

                if (mins > 120) timeGrainParam = "&interval=PT5M";
                if (mins > 360) timeGrainParam = "&interval=PT15M";
                if (mins > 720) timeGrainParam = "&interval=PT30M";
                if (mins > 1440) timeGrainParam = "&interval=PT1H";
                if (mins > 4320) timeGrainParam = "&interval=PT3H";
                if (mins > 8640) timeGrainParam = "&interval=PT6H";

                vm.MetricsHistoryMinutes = mins;

                string timeFrom = DateTime.UtcNow.AddMinutes(-1 * mins).ToString("s") + "Z";
                string timeTo = DateTime.UtcNow.ToString("s") + "Z";

                string url = $"https://management.azure.com/subscriptions/{vm.Subscription.subscriptionId}/resourceGroups/{vm.resourceGroup}/providers/Microsoft.Compute/virtualMachines/{vm.name}/providers/Microsoft.Insights/metrics?{timeGrainParam}&aggregation=average,maximum&timespan={timeFrom}/{timeTo}&metricnames=Percentage%20CPU&api-version=2021-05-01";
                // string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/providers/Microsoft.Compute/virtualMachines?api-version=2022-08-01";

                vm.IsRestQueryBusy = true;
                vm.MetricsErrorMessage = "";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Failed vm metrics!");
                    vm.MetricsErrorMessage = $"Failed metrics!: {response.ReasonPhrase}";
                    vm.IsRestQueryBusy = false;
                    return;
                }
                var json = await response.Content.ReadAsStringAsync();
                var metrics = await response?.Content?.ReadFromJsonAsync<RootMetric>();

                if (metrics?.value != null)
                {
                    foreach (var metric in metrics.value)
                    {
                        if (metric.timeseries.Count() == 0) continue;

                        var latestAvg = metric.timeseries[0].data[0].average;
                        var latestMax = metric.timeseries[0].data[0].maximum;

                        if (latestMax > 0)
                        {
                            //Debug.WriteLine("Got max for " + metric.name.value);
                        }

                        string metricName = metric.name.value;
                        long outLong;
                        switch (metricName)
                        {
                          
                            case "Percentage CPU":
                                //vm.dtu_consumption_percent = latestAvg;

                                vm.PerformanceMetricSeries.Clear();

                                vm.MaxCpuUsed = 0;
                                foreach (var d in metric.timeseries[0].data.OrderByDescending(x => x.timeStamp))
                                {
                                    vm.PerformanceMetricSeries.Add(d);
                                    if (d.maximum > vm.MaxCpuUsed) vm.MaxCpuUsed = d.maximum;
                                }

                                break;
                          
                          
                        }
                    }

                    // spend analysis
                    if (minutes > 2)
                    {
                        vm.GotMetricsHistory = true;
                        vm.OverSpendFromMaxPc = 100 - vm.MaxCpuUsed;
                        vm.CalcPotentialSaving();
                    }
                }

                //Debug.WriteLine($"finished getting {sqlDb.name} metrics");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            vm.IsRestQueryBusy = false;
        }
        private static async Task GetDbServiceTierAdvisors(RestSqlDb sqlDb)
        {
            
            try
            {
                string url = $"https://management.azure.com/subscriptions/{sqlDb.Subscription.subscriptionId}/resourceGroups/{sqlDb.resourceGroup}/providers/Microsoft.Sql/servers/{sqlDb.serverName}/databases/{sqlDb.name}/serviceTierAdvisors/?api-version=2022-09-01";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                RootServiceTierAdvisor advisors = await response?.Content?.ReadFromJsonAsync<RootServiceTierAdvisor>();

                if(advisors?.value?.Count() > 0)
                {
                    sqlDb.serviceTierAdvisor = advisors.value[0];
                    if (advisors.value.Count() > 1)
                    {
                        Debug.WriteLine($"multi advisors! {sqlDb.name} {advisors.value.Count()}");
                    }
                }
                
                //Debug.WriteLine($"finished getting {sqlDb.name} advisors");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
        public enum CostRequestType
        {
            Purview
                , SqlDatabase
                , DataFactory
                , Storage
                , VNet
                , VM
        }
        public static async Task GetSubscriptionCosts(Subscription subscription, CostRequestType costType)
        {
            /* How this authentication works:
             * https://learn.microsoft.com/en-gb/dotnet/api/overview/azure/service-to-service-authentication?view=azure-dotnet
             * 
             * For local development, AzureServiceTokenProvider fetches tokens using Visual Studio, Azure command-line interface (CLI), or Azure AD Integrated Authentication. 
             * Each option is tried sequentially and the library uses the first option that succeeds. 
             * If no option works, an AzureServiceTokenProviderException exception is thrown with detailed information.
             * 
             * If you have cloned the repo to run this code you have visual studio so likely to work for you
             * If deploying/running on non visual studio computers then suing Azure CLI:
             *  - az login
             *  - az account get-access-token --resource https://vault.azure.net
             */

            if (!subscription.ReadCosts) return;
            if (!subscription.NeedsNewCosts()) return;

            subscription.ResourceCosts.Clear();
            subscription.CostsErrorMessage = "";

            try
            {       
                string timeFrom = DateTime.UtcNow.AddDays(-CostDays).ToString("s") + "Z";
                string timeTo = DateTime.UtcNow.ToString("s") + "Z";         

                string payload = @"{'type':'ActualCost','dataSet':{'granularity':'None','aggregation':{'totalCost':{'name':'Cost','function':'Sum'},'totalCostUSD':{'name':'CostUSD','function':'Sum'},'sorting':[{'direction':'descending','name':'Cost'}],'grouping':[{'type':'Dimension','name':'ServiceName'},{'type':'Dimension','name':'MeterSubCategory'},{'type':'Dimension','name':'Product'},{'type':'Dimension','name':'Meter'},{'type':'Dimension','name':'ChargeType'},{'type':'Dimension','name':'PublisherType'}]},'timeframe':'Custom','timePeriod':{'from':'2022-11-01T00:00:00.000Z','to':'2022-11-30T23:59:59.000Z'}";
                payload = @"{""type"":""ActualCost"",""dataSet"":{""granularity"":""None"",""aggregation"":{""totalCost"":{""name"":""Cost"",""function"":""Sum""},""totalCostUSD"":{""name"":""CostUSD"",""function"":""Sum""}},""sorting"":[{""direction"":""descending"",""name"":""Cost""}],""grouping"":[{""type"":""Dimension"",""name"":""ResourceGroupName""},{""type"":""Dimension"",""name"":""SubscriptionId""}]},""timeframe"":""Custom"",""timePeriod"":{""from"":""2022-11-01T00:00:00.000Z"",""to"":""2022-11-30T23:59:59.000Z""}}";
                payload = @"{""type"":""ActualCost"",""dataSet"":{""granularity"":""None"",""aggregation"":{""totalCost"":{""name"":""Cost"",""function"":""Sum""},""totalCostUSD"":{""name"":""CostUSD"",""function"":""Sum""}},""sorting"":[{""direction"":""descending"",""name"":""Cost""}],""grouping"":[{""type"":""Dimension"",""name"":""ResourceId""},{""type"":""Dimension"",""name"":""ServiceName""},{""type"":""Dimension"",""name"":""MeterSubCategory""},{""type"":""Dimension"",""name"":""Product""},{""type"":""Dimension"",""name"":""Meter""},{""type"":""Dimension"",""name"":""ChargeType""},{""type"":""Dimension"",""name"":""PublisherType""}]},""timeframe"":""Custom"",""timePeriod"":{""from"":""2022-11-01T00:00:00.000Z"",""to"":""2022-11-30T23:59:59.000Z""}}";

                // TheLastMonth, MonthToDate
                // filter: https://learn.microsoft.com/en-us/rest/api/cost-management/query/usage?tabs=HTTP#queryfilter

                // *** for available service names look at the costs analysis page in the portal and group by service name ***

                // todo - work out how to break up subscription costs from 1 list into a list for each of these
                string typeClause = "";
                switch (costType)
                {
                    case CostRequestType.Purview:
                        typeClause = @"""Bandwidth"",""Purview"",""Storage""";
                        break;
                    case CostRequestType.SqlDatabase:
                        typeClause = @"""SQL Database"",""SQL Server"", ""Advanced Threat Protection"",""Storage"", ""Synapse SQL Pool"", ""Azure Synapse Analytics"", ""Power BI Embedded"", ""Bandwidth""";
                        break;
                    case CostRequestType.DataFactory:
                        typeClause = @"""Azure Data Factory v2"",""Storage"", ""Bandwidth""";
                        break; 
                    case CostRequestType.Storage:
                        typeClause = @""""",""Bandwidth"",""Storage""";
                        break;
                    case CostRequestType.VNet:
                        typeClause = @"""Virtual Network"",""Bandwidth"",""Storage""";
                        break;
                    case CostRequestType.VM:
                        typeClause = @"""Virtual machines"", ""Virtual Network"", ""Bandwidth"",""Storage""";
                        break;                    
                }
                //" + typeClause + @"

                payload = @"{""type"":""ActualCost""
                   
                            ,""timeframe"": ""Custom""
                            , ""timePeriod"":{""from"":""" + timeFrom + @""",""to"":""" + timeTo + @"""}
                            ,""dataSet"":{
                                ""granularity"":""None""
                                
 ,""filter"": { 
                                    ""dimensions"": {
                                        ""name"": ""serviceName""
                                        ,""operator"": ""In""
                                        ,""values"": [  


                                            ""Azure Data Factory v2""
                                            ,""SQL Database""
                                            ,""SQL Server""
                                            ,""Storage""
                                            ,""Virtual machines""
                                            ,""Bandwidth""
                                            ,""Virtual Network""
                                            ,""Advanced Threat Protection""
                                            ,""Purview""
                                            ,""Azure Purview""
                                            ,""Power BI Embedded""
                                            ,""Azure Synapse Analytics""
                                            ,""Synapse SQL Pool""
                                        ]
                                    }
                                }



                            ,""aggregation"":{
                                ""totalCost"":{
                                    ""name"":""Cost""
                                    ,""function"":""Sum""
                                },
                                ""totalCostUSD"":{
                                    ""name"":""CostUSD""
                                    ,""function"":""Sum""
                                }
                            }
                            ,""sorting"":[
                                {""direction"":""descending""
                                ,""name"":""Cost""}
                            ]
                            ,""grouping"":[
                                {""type"":""Dimension""
                                ,""name"":""ResourceId""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""ServiceName""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""MeterSubCategory""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""Product""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""Meter""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""ChargeType""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""PublisherType""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""Provider""
                                }
                               
                                ,{""type"":""Dimension""
                                    ,""name"":""MeterCategory""
                                }
                                ,{""type"":""Dimension""
                                    ,""name"":""ResourceType""
                                }
                            ]
                        }
                    }";

                /*
              
                                     
                 */
             


                string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/providers/Microsoft.CostManagement/query?api-version=2021-10-01";
            
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://management.azure.com/subscriptions/")
                };

                client.DefaultRequestHeaders.Remove("Authorization");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");                
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header               

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(payload,
                                                    Encoding.UTF8,
                                                    "application/json");//CONTENT-TYPE header

                HttpResponseMessage response = await client.SendAsync(request);// client.PostAsync(url, new StringContent(payload));

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Debug.WriteLine(response.Content);
                }

                if (!response.IsSuccessStatusCode)
                {
                    
                    subscription.CostsErrorMessage = $@"Subscription '{subscription.displayName}' costs query {response.ReasonPhrase}";
                    return;
                }
                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("vm-exds-1-we-01")){
                    Debug.WriteLine("got it");
                }
                ResourceCostQuery query = await response.Content.ReadFromJsonAsync<ResourceCostQuery>();

                foreach (var obj in query.properties?.rows)
                {
                    
                    try
                    {
                        var rc = new ResourceCost();
                        rc.SubscriptionId = subscription.subscriptionId;
                        rc.Cost = Decimal.Parse(obj[0].ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint); 
                        rc.CostUSD = Decimal.Parse(obj[1].ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);
                        rc.ResourceId = obj[2].ToString();
                        rc.ServiceName = obj[3].ToString();
                        rc.MeterSubCategory = obj[4].ToString();
                        rc.Product = obj[5].ToString();
                        rc.Meter = obj[6].ToString();
                        rc.ChargeType = obj[7].ToString();
                        rc.PublisherType = obj[8].ToString();
                        rc.ResourceType = obj[11].ToString();
                        rc.Currency = obj[12].ToString();
                        subscription.ResourceCosts.Add(rc);

                        //if (rc.ServiceName.Contains("Factory"))
                        //{
                        //    Debug.WriteLine("factory");
                        //}
                        //if (rc.ServiceName!= "SQL Database")
                        //{
                        //    Debug.WriteLine("factory");
                        //}
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }                 
                }                                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                subscription.CostsErrorMessage = ex.Message;
            }            
        }

        public static async Task GetDataFactories(Subscription subscription)
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/resources?$filter=resourceType eq 'Microsoft.DataFactory/factories' &$expand=resourceGroup,createdTime,changedTime&$top=1000&api-version=2021-04-01";
                StringContent queryString = new StringContent("api-version=2021-04-01");
                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                // get location and name properties from list of servers
                DataFactoryRoot factories = await response.Content.ReadFromJsonAsync<DataFactoryRoot>();
                if (factories?.value == null) return;
                // 
                foreach (var factory in factories.value)
                {
                    string rg = factory.id.Substring(factory.id.IndexOf("resourceGroup") + 15);
                    factory.resourceGroup = rg.Substring(0, rg.IndexOf("/"));

                    string sub = factory.id.Substring(factory.id.IndexOf("subscription") + 14);
                    factory.Subscription = subscription;
                }
                subscription.DataFactories = factories.value.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }            
        }
        public static async Task GetStorageAccounts(Subscription subscription)
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/providers/Microsoft.Storage/storageAccounts?api-version=2022-05-01";
                
                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("got storage acc");

                RootStorageAccount root = await response.Content.ReadFromJsonAsync<RootStorageAccount>();

                foreach(var account in root.value)
                {
                    Debug.WriteLine($"Got storage acc {account.name}");
                    account.Subscription = subscription;

                    string rg = account.id.Substring(account.id.IndexOf("resourceGroup") + 15);
                    account.resourceGroup = rg.Substring(0, rg.IndexOf("/"));

                }
                subscription.StorageAccounts = root.value.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static async Task GetVirtualNetworks(Subscription subscription)
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/providers/Microsoft.Network/virtualNetworks?api-version=2022-05-01";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("got vnet");

                RootVNet root = await response.Content.ReadFromJsonAsync<RootVNet>();
                if (root?.value == null) return;

                foreach (var vnet in root.value)
                {
                    Debug.WriteLine($"Got storage acc {vnet.name}");
                    vnet.Subscription = subscription;

                    string rg = vnet.id.Substring(vnet.id.IndexOf("resourceGroup") + 15);
                    vnet.resourceGroup = rg.Substring(0, rg.IndexOf("/"));

                }
                subscription.VNets = root.value.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static async Task GetVirtualMachines(Subscription subscription)
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/providers/Microsoft.Compute/virtualMachines?api-version=2022-08-01";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("got vnet");

                RootVM root = await response.Content.ReadFromJsonAsync<RootVM>();
                if (root?.value == null) return;

                foreach (var vm in root.value)
                {
                    Debug.WriteLine($"Got storage acc {vm.name}");
                    vm.Subscription = subscription;

                    string rg = vm.id.Substring(vm.id.IndexOf("resourceGroup") + 15);
                    vm.resourceGroup = rg.Substring(0, rg.IndexOf("/"));
                }
                subscription.VMs = root.value.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static async Task GetPurviews(Subscription subscription)
        {
            try
            {
                string url = $"https://management.azure.com/subscriptions/{subscription.subscriptionId}/providers/Microsoft.Purview/accounts?api-version=2021-07-01";

                HttpResponseMessage response = await GetHttpClientAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                
                RootPurview root = await response.Content.ReadFromJsonAsync<RootPurview>();
                if (root?.value == null) return;

                foreach (var purv in root.value)
                {
                    Debug.WriteLine($"Got purv acc {purv.name}");
                    purv.Subscription = subscription;

                    string rg = purv.id.Substring(purv.id.IndexOf("resourceGroup") + 15);
                    purv.resourceGroup = rg.Substring(0, rg.IndexOf("/"));
                }
                subscription.Purviews = root.value.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }


}
