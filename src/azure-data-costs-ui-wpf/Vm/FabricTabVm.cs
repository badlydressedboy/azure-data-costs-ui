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
    public class FabricTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<Fabric> FabricList { get; private set; } = new ObservableCollection<Fabric>();

        private bool isFabricQueryBusy;
        public bool IsFabricQueryBusy
        {
            get => isFabricQueryBusy;
            set
            {
                SetProperty(ref isFabricQueryBusy, value);
            }
        }


        #endregion

        #region Filters

        public Filter FabricNameFilter { get; set; } = new Filter();
      
        #endregion

        
        public FabricTabVm()
        {
            _filterList.Add(FabricNameFilter);
        }

        public async Task RefreshFabric(List<Subscription> selectedSubscriptions)
        {
            if (IsFabricQueryBusy) return;
            IsFabricQueryBusy = true;

            try
            {
                //SyncSelectedSubs();
                ClearFilterItems();
                FabricList.Clear();

                decimal totalFabricCosts = 0;

                var subsCopy = selectedSubscriptions.ToList();
                await Parallel.ForEachAsync(subsCopy
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            await APIAccess.GetFabricCapacities(sub);
                            if (sub.FabricCapacities.Count > 0)
                            {
                                await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.Fabric, forceRead: true);
                            }
                        });

                foreach (var sub in subsCopy)
                {
                    if (!sub.ReadObjects) continue; // ignore this subscription
                    foreach (var purv in sub.FabricCapacities)
                    {
                        MapCostToFabric(purv, sub.ResourceCosts);

                        foreach (var c in purv.Costs) totalFabricCosts += c.Cost;

                        foreach (var tag in purv.TagsList) TagsFilter.AddSelectableItem(tag);

                        ResourceGroupFilter.AddSelectableItem(purv.resourceGroup);
                        SubscriptionFilter.AddSelectableItem(purv.Subscription.displayName);
                        LocationFilter.AddSelectableItem(purv.location);
                    }

                    // only add to grid after all filters have been added
                    sub.FabricCapacities.ForEach(purv => FabricList.Add(purv));
                }
                TotalCostsText = totalFabricCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsFabricQueryBusy = false;

           // UpdateHttpAccessCountMessage();
        }

        private static void MapCostToFabric(Fabric Fabric, List<ResourceCost> costs)
        {
            bool found = false;
            Fabric.Costs.Clear();
            Fabric.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                // either its a Fabric acc object or it is something in the managed resource group (which doesnt have 'Fabric/accounts' in its ResourceId)
                if (
                    //(!cost.ResourceId.Contains(Fabric.properties))
                    // (!cost.ResourceId.Contains(@"Fabric/accounts/"))
                     (!cost.ServiceName.Contains("Fabric")))
                {
                    //_logger.Error(cost.ResourceId);
                    continue;
                }

                //string costResourceName = cost.ResourceId.Substring(cost.ResourceId.IndexOf("databaseaccounts/") + 17);

                if (cost.ResourceId.Contains(Fabric.resourceGroup.ToLower()))
                {
                    Fabric.TotalCostBilling += cost.Cost;

                    Fabric.Costs.Add(cost);
                    found = true;
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for Fabric {Fabric.name}?");
            }
        }

    }

}
