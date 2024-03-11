using Azure.Costs.Common;
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

namespace Azure.Costs.Ui.Wpf.Vm
{
    public class DBTabVm : ObservableObject
    {
        #region Properties
        public ObservableCollection<RestSqlDb> RestSqlDbList { get; private set; } = new ObservableCollection<RestSqlDb>();

        public List<SelectableString> AllTags { get; set; } = new List<SelectableString>();

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

        private string? tagFilterSummary;
        public string? TagFilterSummary
        {
            get => tagFilterSummary;
            set => SetProperty(ref tagFilterSummary, value);
        }
        private string restErrorMessage;
        public string RestErrorMessage
        {
            get => restErrorMessage;
            set
            {

                SetProperty(ref restErrorMessage, value);
                if (string.IsNullOrEmpty(value))
                {
                    IsRestErrorMessageVisible = false;
                }
                else
                {
                    IsRestErrorMessageVisible = true;
                }
            }
        }
        private bool isRestErrorMessageVisible;
        public bool IsRestErrorMessageVisible
        {
            get => isRestErrorMessageVisible;
            set
            {
                SetProperty(ref isRestErrorMessageVisible, value);
            }
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


        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DBTabVm()
        {
            //_logger.Info("DBTabVm ctor");
        }

        public async Task RefreshDatabases(List<Subscription> selectedSubscriptions)
        {
            if (IsGetSqlServersBusy) return;

            IsGetSqlServersBusy = true;
            Debug.WriteLine("IsGetSqlServersBusy = true...");
            DbFooterErrorText = "";
            TagFilterSummary = "";
            RestSqlDbList.Clear();
            RestErrorMessage = "";
            decimal totalSqlDbCosts = 0;

            try
            {
                AllTags.Clear();
                
                //SyncSelectedSubs();// todo


                await Parallel.ForEachAsync(selectedSubscriptions
                    , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                    , async (sub, y) =>
                    {
                        await APIAccess.GetSqlServers(sub);

                        // add servers to ui quickly, dont wait for costs
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            sub.SqlServers.ForEach(s =>
                            {
                                s.Dbs.ForEach(db => RestSqlDbList.Add(db));
                            });
                        });

                        if (sub.SqlServers.Count > 0 && sub.ResourceCosts.Count == 0 && sub.ReadCosts) await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.SqlDatabase);

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
                                    MapCostToDb(db, sub.ResourceCosts);

                                    totalSqlDbCosts += db.TotalCostBilling; // TotalCostBilling has already been divided by db count if elastic pool

                                    foreach (var tag in db.TagsList)
                                    {
                                        var existing = AllTags.FirstOrDefault(x => x.StringValue == tag);
                                        if (existing == null)
                                        {
                                            AllTags.Add(new SelectableString() { StringValue = tag, IsSelected = true });
                                        }
                                    }

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

        private static void MapCostToDb(RestSqlDb db, List<ResourceCost> costs)
        {
            bool found = false;
            if (costs.Count == 0)
            {
                _logger.Error("elastic db!");
            }
            List<ResourceCost> elasticCosts = new List<ResourceCost>();
            foreach (ResourceCost cost in costs)
            {
                if (cost.ResourceId.EndsWith(db.name.ToLower()) && cost.ResourceId.Contains(db.resourceGroup.ToLower()))
                {
                    db.Costs.Add(cost);
                    db.TotalCostBilling += cost.Cost;
                    found = true;
                }
                if (db.ElasticPool != null && cost.ResourceId.Contains(db.ElasticPool.name.ToLower()))
                {
                    db.Costs.Add(cost);
                    db.TotalCostBilling += cost.Cost / db.ElasticPool.dbList.Count;
                    found = true;
                }
                if (cost.Meter.Contains("Elastic")
                    || cost.MeterSubCategory.Contains("Elastic")
                    //|| cost.Product.Contains("Elastic") 
                    || cost.ServiceName.Contains("Elastic"))
                {
                    elasticCosts.Add(cost);
                }
            }
            //if(elasticCosts.Count > 0)
            //{
            //    //_logger.Error($"elastic costs");
            //}
            if (!found)
            {
                _logger.Info($"why no cost for DB {db.name}? Costs.count: {costs.Count}");
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
        public void SetTagFilterSummary()
        {
            var x = AllTags.Where(x => x.IsSelected).Count();
            var y = AllTags.Count;
            if (x == y)
            {
                TagFilterSummary = "";
            }
            else
            {
                TagFilterSummary = $"{x}/{y}";
            }
        }
    }
}
