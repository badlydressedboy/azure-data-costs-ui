﻿using Azure.Costs.Common;
using Azure.Costs.Common.Models.Rest;
using CommunityToolkit.Mvvm.ComponentModel;
using DataEstateOverview;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Costs.Ui.Wpf;
using NLog.Filters;

namespace Azure.Costs.Ui.Wpf.Vm
{
    public class DBTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<RestSqlDb> RestSqlDbList { get; private set; } = new ObservableCollection<RestSqlDb>();
        
        private bool isGetSqlServersBusy;
        public bool IsGetSqlServersBusy
        {
            get => isGetSqlServersBusy;
            set
            {
                SetProperty(ref isGetSqlServersBusy, value);
            }
        }
        private string dbFooterErrorText;
        public string DbFooterErrorText
        {
            get => dbFooterErrorText;
            set => SetProperty(ref dbFooterErrorText, value);
        }

        private string dbRecsButtonBaseText = "DB RECOMMENDATIONS";
        private string dbRecsButtonText;
        public string DbRecsButtonText
        {
            get => dbRecsButtonText;
            set => SetProperty(ref dbRecsButtonText, value);
        }

        

        private string totalSqlDbCostsText;
        public string TotalSqlDbCostsText
        {
            get => totalSqlDbCostsText;
            set => SetProperty(ref totalSqlDbCostsText, value);
        }
        private bool isDbSpendAnalysisBusy;
        public bool IsDbSpendAnalysisBusy
        {
            get => isDbSpendAnalysisBusy;
            set
            {
                SetProperty(ref isDbSpendAnalysisBusy, value);
            }
        }
        private decimal _totalPotentialDbSavingAmount;
        public decimal TotalPotentialDbSavingAmount
        {
            get { return _totalPotentialDbSavingAmount; }
            set
            {
                _totalPotentialDbSavingAmount = value;
                OnPropertyChanged("TotalPotentialDbSavingAmount");
            }
        }
        private bool hasDbSpendAnalysisBeenPerformed;
        public bool HasDbSpendAnalysisBeenPerformed
        {
            get => hasDbSpendAnalysisBeenPerformed;
            set
            {
                SetProperty(ref hasDbSpendAnalysisBeenPerformed, value);
            }
        }


        #endregion

        #region Filters
                
        public Filter SoFilter { get; set; } = new Filter();
        public Filter ServerFilter { get; set; } = new Filter();
        

        
        #endregion

        private static int _recsCount = 0;  

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DBTabVm()
        {
            //_logger.Info("DBTabVm ctor");        
            _filterList.Add(SoFilter);
            _filterList.Add(ServerFilter);

            DbRecsButtonText = dbRecsButtonBaseText;
        }

        private void UpdateRecsCount(int additionalRecsCount)
        {
            _recsCount += additionalRecsCount; 
            DbRecsButtonText = $"{dbRecsButtonBaseText} ({_recsCount})";
        }

        public async Task RefreshDatabases(List<Subscription> selectedSubscriptions)
        {
            if (IsGetSqlServersBusy) return;

            IsGetSqlServersBusy = true;
            Debug.WriteLine("IsGetSqlServersBusy = true...");
            DbFooterErrorText = "";
            RestSqlDbList.Clear();
            RestErrorMessage = "";
            _recsCount = 0;
            UpdateRecsCount(0);

            decimal totalSqlDbCosts = 0;

            try
            {
                // repopulate filters from empty
                ClearFilterItems();                

                //SyncSelectedSubs();// todo

                await Parallel.ForEachAsync(selectedSubscriptions
                    , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                    , async (sub, y) =>
                    {
                        await APIAccess.GetSqlServers(sub);

                        // build filters before adding item to grid
                       

                        // add servers to ui quickly, dont wait for costs
                        App.Current.Dispatcher.Invoke(() =>
                        { 
                            sub.SqlServers.ForEach(s =>
                            {
                                s.Dbs.ForEach(db =>
                                {
                                    foreach (var tag in db.TagsList) TagsFilter.AddSelectableItem(tag);

                                    ResourceGroupFilter.AddSelectableItem(db.resourceGroup);
                                    SoFilter.AddSelectableItem(db.properties.currentServiceObjectiveName);
                                    ServerFilter.AddSelectableItem(db.serverName);
                                    SubscriptionFilter.AddSelectableItem(db.Subscription.displayName);
                                    LocationFilter.AddSelectableItem(db.location);
                                });
                            });
                            sub.SqlServers.ForEach(s =>
                            {
                                s.Dbs.ForEach(db => {                                  

                                    RestSqlDbList.Add(db); // filters set before adding to list
                                                           // 
                                    UpdateRecsCount(db.AdvisorRecommendationCount);
                                });
                            });
                        });

                        if (sub.SqlServers.Count > 0)
                        {
                            await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.SqlDatabase, forceRead:true);
                        }

                        // on ui thread
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            if (sub.ResourceCosts.Count == 0 && sub.ReadCosts && sub.SqlServers.Count > 0) // only an error if we actually asked for costs
                            {
                                _logger.Info($"No expected DB costs found for sub: {sub.displayName}");
                                // sub.CostsErrorMessage = "No expected DB costs found."; // this may already have costs error message so dont overwrite it
                                //continue;
                            }
                            foreach (var s in sub.SqlServers)
                            {
                                foreach (var db in s.Dbs)
                                {
                                    MapCostToDb(db);

                                    totalSqlDbCosts += db.TotalCostBilling; // TotalCostBilling has already been divided by db count if elastic pool                                    
                                }
                            }

                            if (!string.IsNullOrEmpty(sub.CostsErrorMessage))
                            {
                                if (!string.IsNullOrEmpty(RestErrorMessage)) RestErrorMessage += "\n";
                                RestErrorMessage += sub.CostsErrorMessage;
                            }
                        });
                    });

                TotalSqlDbCostsText = totalSqlDbCosts.ToString("N2");

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsGetSqlServersBusy = false;
            Debug.WriteLine("IsGetSqlServersBusy = false");
            
            
            //UpdateHttpAccessCountMessage(); // todo
        }

        // Refresh all or subset of selected subscriptions
        // Support TotalSqlDbCostsText updating so need to sum all db costs after get costs 
        public async Task GetMissingDbCosts()
        {
            var subs = new List<Subscription>();
            foreach (var db in RestSqlDbList)
            {
                if (db.Subscription.SqlServers.Count > 0 && db.Subscription.ResourceCosts.Count == 0) // read costs regardless of main screen cost selection
                {
                    if (!subs.Contains(db.Subscription)) subs.Add(db.Subscription);
                }
            }

            await Parallel.ForEachAsync(subs
                , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                , async (sub, y) =>
            {
                await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.SqlDatabase, true);
                Debug.WriteLine($"Got sub costs for {sub.displayName}, count: {sub.ResourceCosts.Count}");
            });

            // on ui thread
            App.Current.Dispatcher.Invoke(() =>
            {
                decimal totalSqlDbCosts = 0;
                foreach (var db in RestSqlDbList)
                {
                    
                    MapCostToDb(db);
                    if(db.TotalCostBilling == 0)
                    {
                        Debug.WriteLine("How?");
                    }
                    totalSqlDbCosts += db.TotalCostBilling; // TotalCostBilling has already been divided by db count if elastic pool                                    
                }

                TotalSqlDbCostsText = totalSqlDbCosts.ToString("N2");
                //if (!string.IsNullOrEmpty(dbFooterErrorText.sub.CostsErrorMessage))
                //{
                //    if (!string.IsNullOrEmpty(RestErrorMessage)) RestErrorMessage += "\n";
                //    RestErrorMessage += sub.CostsErrorMessage;
                //}
            });
        }

        private static void MapCostToDb(RestSqlDb db)
        {
            bool found = false;
            if (db.Subscription.ResourceCosts.Count == 0)
            {
                _logger.Error("elastic db!");
            }
            db.ElasticPool?.Costs.Clear();

            foreach (ResourceCost cost in db.Subscription.ResourceCosts)
            {
                
                if (cost.ResourceId.Contains(db.name.ToLower()) && cost.ResourceId.Contains(db.resourceGroup.ToLower()))
                {
                    db.Costs.Add(cost);
                    db.TotalCostBilling += cost.Cost;
                    found = true;
                    
                    if (cost.ResourceType.Contains("longtermretentionservers"))
                    {
                        Debug.WriteLine("Add me!");
                    }
                }
                if (db.ElasticPool != null && cost.ResourceId.Contains(db.ElasticPool.name.ToLower()))
                {
                    //db.Costs.Add(cost);
                    db.TotalCostBilling += cost.Cost / db.ElasticPool.dbList.Count;
                    found = true;

                    // each db will have identical copy of pools costs
                    // these need to be divided by db count
                    var dbCost = new ElasticPoolCost(cost);
                    db.ElasticPool.Costs.Add(dbCost);  
                }
                //if (cost.Meter.Contains("Elastic")
                //    || cost.MeterSubCategory.Contains("Elastic")
                //    //|| cost.Product.Contains("Elastic") 
                //    || cost.ServiceName.Contains("Elastic"))
                //{
                //    elasticCosts.Add(cost);
                //}
            }

            if (db.ElasticPool != null)
            {
                foreach (var cost in db.ElasticPool?.Costs)
                {
                    cost.PerDbCost = cost.Cost / db.ElasticPool.dbList.Count;
                }
            }
            //_logger.Error($"elastic costs");
            
            if (!found)
            {
                if (db.Subscription.ReadCosts)
                {
                    _logger.Info($"why no cost for DB {db.name}? Costs.count: {db.Subscription.ResourceCosts.Count}");
                }
            }
        }

        public async Task AnalyseDbSpend()
        {
            if (IsDbSpendAnalysisBusy) return;
            IsDbSpendAnalysisBusy = true;

            RestErrorMessage = "";
            //decimal totalPotentialSaving = 0;
            TotalPotentialDbSavingAmount = 0;
            try
            {
                await GetMissingDbCosts(); // if any subs not already selected for costs their costs will be got here

                await Parallel.ForEachAsync(RestSqlDbList.OrderByDescending(x => x.TotalCostBilling)
                    , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                    , async (db, y) =>
                    {
                        db.SpendAnalysisStatus = "Analysing...";
                        db.OverSpendFromMaxPcString = "?";

                        await APIAccess.GetDbMetrics(db);

                        db.SpendAnalysisStatus = "Complete";
                        //await APIAccess.GetSqlServers(sub);
                    });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsDbSpendAnalysisBusy = false;
            HasDbSpendAnalysisBeenPerformed = true;
            //UpdateHttpAccessCountMessage(); // todo
            TotalPotentialDbSavingAmount = RestSqlDbList.Sum(x => x.PotentialSavingAmount);
        }
        


    }

}
