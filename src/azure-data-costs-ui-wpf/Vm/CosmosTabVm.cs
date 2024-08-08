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
    public class CosmosTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<Cosmos> CosmosList { get; private set; } = new ObservableCollection<Cosmos>();

        private bool isCosmosQueryBusy;
        public bool IsCosmosQueryBusy
        {
            get => isCosmosQueryBusy;
            set
            {
                SetProperty(ref isCosmosQueryBusy, value);
            }
        }


        #endregion

        #region Filters

        public Filter CosmosNameFilter { get; set; } = new Filter();
      
        #endregion

        
        public CosmosTabVm()
        {
            _filterList.Add(CosmosNameFilter);
        }

        public async Task RefreshCosmos(List<Subscription> selectedSubscriptions)
        {
            if (IsCosmosQueryBusy) return;
            IsCosmosQueryBusy = true;

            try
            {
                //SyncSelectedSubs();
                ClearFilterItems();
                CosmosList.Clear();

                decimal totalCosmosCosts = 0;

                var subsCopy = selectedSubscriptions.ToList();
                await Parallel.ForEachAsync(subsCopy
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            await APIAccess.GetCosmosDBs(sub);
                            //if (sub.Cosmoss.Count > 0 && sub.ResourceCosts.Count == 0 && sub.ReadCosts) await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.Cosmos);
                        });

                foreach (var sub in subsCopy)
                {
                    if (!sub.ReadObjects) continue; // ignore this subscription
                    foreach (var purv in sub.Cosmos)
                    {
                        //MapCostToCosmos(purv, sub.ResourceCosts);

                        foreach (var c in purv.Costs) totalCosmosCosts += c.Cost;

                        foreach (var tag in purv.TagsList) TagsFilter.AddSelectableItem(tag);

                        ResourceGroupFilter.AddSelectableItem(purv.resourceGroup);
                        SubscriptionFilter.AddSelectableItem(purv.Subscription.displayName);
                        LocationFilter.AddSelectableItem(purv.location);
                    }

                    // only add to grid after all filters have been added
                    sub.Cosmos.ForEach(purv => CosmosList.Add(purv));
                }
                TotalCostsText = totalCosmosCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsCosmosQueryBusy = false;

           // UpdateHttpAccessCountMessage();
        }

        private static void MapCostToCosmos(Purview purv, List<ResourceCost> costs)
        {
            bool found = false;
            purv.Costs.Clear();
            purv.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                // either its a Cosmos acc object or it is something in the managed resource group (which doesnt have 'Cosmos/accounts' in its ResourceId)
                if ((!cost.ResourceId.Contains(purv.properties.managedResourceGroupName))
                    && (!cost.ResourceId.Contains(@"Cosmos/accounts/"))
                    && (!cost.ServiceName.Contains("Cosmos")))
                {
                    //_logger.Error(cost.ResourceId);
                    continue;
                }

                string costPurvName = cost.ResourceId.Substring(cost.ResourceId.IndexOf("Cosmos/accounts/") + 17);

                if (costPurvName == purv.name || cost.ResourceId.Contains(purv.properties.managedResourceGroupName.ToLower()))
                {
                    purv.TotalCostBilling += cost.Cost;

                    purv.Costs.Add(cost);
                    found = true;
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for Cosmos {purv.name}?");
            }
        }

    }

}
