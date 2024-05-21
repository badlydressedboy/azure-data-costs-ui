using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Common.Models.SQL
{
    public class AzDB : BaseModel
    {
        public List<EventLogEntry> EventLog { get; set; } = new List<EventLogEntry>();

        private bool isQueryingDatabase;

        public bool IsQueryingDatabase
        {
            get => isQueryingDatabase;
            set
            {
                SetProperty(ref isQueryingDatabase, value);
            }
        }
        private bool userHasSelectPermission;

        public bool UserHasSelectPermission
        {
            get => userHasSelectPermission;
            set
            {
                SetProperty(ref userHasSelectPermission, value);
            }
        }
        public AzServer ParentAzServer { get; set; }
        public string ServerName { get; set; }

        //public string DatabaseName { get; set; }
        private string databaseName;
        public string DatabaseName
        {
            get => databaseName;
            set => SetProperty(ref databaseName, value);
        }

        public string Edition { get; set; }
        public string ServiceObjective { get; set; }

        public string MonthlyCost { get; set; }
        public string ElasticPoolName { get; set; }
        public float StorageMB { get; set; }
        public float AvgCpuPc { get; set; }
        public float AvgDataIoPc { get; set; }
        public float AvgLogWritePc { get; set; }
        public float MaxWorkerPc { get; set; }
        public float MaxSessionPc { get; set; }
        public bool HasRefreshed { get; set; }

        public List<DBPrincipal> DBPrincipals { get; set; } = new List<DBPrincipal>();
        public List<FireWallRule> DBFireWallRules { get; set; } = new List<FireWallRule>();
        public List<DBSyncState> SyncStates { get; set; } = new List<DBSyncState>();
        public List<Session> Sessions { get; set; } = new List<Session>();

        public List<string> DBUserPermissions { get; set; } = new List<string>();
        public async Task Refresh()
        {
            if (IsQueryingDatabase) return;
            IsQueryingDatabase = true;
            Debug.WriteLine($"Querying {DatabaseName}...");
            try
            {
                ExceptionMessages.Clear();
                ConnectivityError = "";
                ParentAzServer.ConnectivityError = "";

                Task[] tasks = new Task[4];

                tasks[0] = Task.Run(async () =>
                {
                    var result = await ProcessResult(await DataAccess.GetDbPrincipals(ConnString));
                    ConnectivityError = result.ExceptionMessage;
                    if (result.Result != null)
                    {
                        DBPrincipals.Clear();
                        DBPrincipals.AddRange((List<DBPrincipal>)result.Result);
                    }
                });
                tasks[1] = Task.Run(async () =>
                {
                    var result = await ProcessResult(await DataAccess.GetDbFireWallRules(ConnString));
                    ConnectivityError = result.ExceptionMessage;
                    if (result.Result != null)
                    {
                        DBFireWallRules.Clear();
                        DBFireWallRules.AddRange((List<FireWallRule>)result.Result);
                    }
                });
                tasks[2] = Task.Run(async () =>
                {
                    var result = await ProcessResult(await DataAccess.GetDbReplication(ConnString));
                    ConnectivityError = result.ExceptionMessage;
                    if (result.Result != null)
                    {
                        SyncStates.Clear();
                        SyncStates.AddRange((List<DBSyncState>)result.Result);

                        if(SyncStates.FirstOrDefault(x => x.PartnerDatabase != null) == null)
                        {
                            SyncStates.Clear();
                        }
                    }
                });
                tasks[3] = Task.Run(async () =>
                {
                    var result = await ProcessResult(await DataAccess.GetDbSessions(ConnString));
                    ConnectivityError = result.ExceptionMessage;
                    if (result.Result != null)
                    {
                        Sessions.Clear();
                        Sessions.AddRange((List<Session>)result.Result);
                    }
                });


                await Task.WhenAll(tasks);
            }catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            IsQueryingDatabase = false;
            HasRefreshed = true;
            Debug.WriteLine($"Finished Querying {DatabaseName}");
        }

        public void SetParent(AzServer parentServer)
        {
            ParentAzServer = parentServer;
            ServerName = parentServer.Name;
            ConnString = parentServer.ConnString.Replace("master", DatabaseName);

            
        }

        public async void GetPermissions()
        {

            var result = await ProcessResult(await DataAccess.GetDbPermission(ConnString));
            ConnectivityError = result.ExceptionMessage;
            if (result.Result != null)
            {
                var permsList = (List<string>)result.Result;
                DBUserPermissions.Clear();
                DBUserPermissions.AddRange(permsList);

                UserHasSelectPermission = permsList.Contains("SELECT");
            }
        }
    }
}
