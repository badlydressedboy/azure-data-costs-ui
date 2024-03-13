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
using Azure.Costs.Ui.Wpf;
using NLog.Filters;

namespace Azure.Costs.Ui.Wpf.Vm
{
    public class StorageTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<StorageAccount> StorageList { get; private set; } = new ObservableCollection<StorageAccount>();

        private bool isStorageQueryBusy;
        public bool IsStorageQueryBusy
        {
            get => isStorageQueryBusy;
            set
            {
                SetProperty(ref isStorageQueryBusy, value);
            }
        }
        

        #endregion

        #region Filters

       
        #endregion

       
        public StorageTabVm()
        {     
        }

        public async Task RefreshStorage(List<Subscription> selectedSubscriptions)
        {
            if (IsStorageQueryBusy) return;
            IsStorageQueryBusy = true;

            try
            {
                //SyncSelectedSubs();
                StorageList.Clear();
                ClearFilterItems();
                decimal totalStorageCosts = 0;

                await Parallel.ForEachAsync(selectedSubscriptions
                        , new ParallelOptions() { MaxDegreeOfParallelism = 20 }
                        , async (sub, y) =>
                        {
                            await APIAccess.GetStorageAccounts(sub);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                sub.StorageAccounts.ForEach(sa =>
                                {
                                    StorageList.Add(sa);
                                });
                            });

                            if (sub.StorageAccounts.Count > 0 && sub.ResourceCosts.Count == 0 && sub.ReadCosts)
                            {
                                await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.Storage);
                            }

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var sa in sub.StorageAccounts)
                                {
                                    MapCostToStorage(sa, sub.ResourceCosts);

                                    foreach (var c in sa.Costs)
                                    {
                                        totalStorageCosts += c.Cost;
                                    }

                                    foreach (var tag in sa.TagsList) TagsFilter.AddSelectableItem(tag);

                                    ResourceGroupFilter.AddSelectableItem(sa.resourceGroup);
                                    SubscriptionFilter.AddSelectableItem(sa.Subscription.displayName);
                                }
                            });
                        });

                TotalCostsText = totalStorageCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsStorageQueryBusy = false;
            //UpdateHttpAccessCountMessage();
        }

        private static void MapCostToStorage(StorageAccount sa, List<ResourceCost> costs)
        {
            bool found = false;
            sa.Costs.Clear();
            sa.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                if (cost.ResourceId.EndsWith(sa.name.ToLower()) && cost.ResourceId.Contains(sa.resourceGroup.ToLower()))
                {
                    sa.TotalCostBilling += cost.Cost;

                    sa.Costs.Add(cost);
                    found = true;
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for storage {sa.name}? Sub costs error: {sa.Subscription.CostsErrorMessage}");
            }
        }

    }

}
