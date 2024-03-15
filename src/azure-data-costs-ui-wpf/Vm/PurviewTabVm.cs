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
    public class PurviewTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<Purview> PurviewList { get; private set; } = new ObservableCollection<Purview>();

        private bool isPurviewQueryBusy;
        public bool IsPurviewQueryBusy
        {
            get => isPurviewQueryBusy;
            set
            {
                SetProperty(ref isPurviewQueryBusy, value);
            }
        }


        #endregion

        #region Filters

        public Filter PurviewNameFilter { get; set; } = new Filter();
      
        #endregion

        
        public PurviewTabVm()
        {
            _filterList.Add(PurviewNameFilter);
        }

        public async Task RefreshPurview(List<Subscription> selectedSubscriptions)
        {
            if (IsPurviewQueryBusy) return;
            IsPurviewQueryBusy = true;

            try
            {
                //SyncSelectedSubs();
                ClearFilterItems();
                PurviewList.Clear();

                decimal totalPurviewCosts = 0;

                var subsCopy = selectedSubscriptions.ToList();
                await Parallel.ForEachAsync(subsCopy
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            await APIAccess.GetPurviews(sub);
                            if (sub.Purviews.Count > 0 && sub.ResourceCosts.Count == 0 && sub.ReadCosts) await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.Purview);
                        });

                foreach (var sub in subsCopy)
                {
                    if (!sub.ReadObjects) continue; // ignore this subscription
                    foreach (var purv in sub.Purviews)
                    {
                        MapCostToPurview(purv, sub.ResourceCosts);
                        
                        foreach (var c in purv.Costs) totalPurviewCosts += c.Cost;
                        
                        foreach (var tag in purv.TagsList) TagsFilter.AddSelectableItem(tag);

                        ResourceGroupFilter.AddSelectableItem(purv.resourceGroup);
                        SubscriptionFilter.AddSelectableItem(purv.Subscription.displayName);
                        LocationFilter.AddSelectableItem(purv.location);
                    }

                    // only add to grid after all filters have been added
                    sub.Purviews.ForEach(purv=> PurviewList.Add(purv));
                }
                TotalCostsText = totalPurviewCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsPurviewQueryBusy = false;
           // UpdateHttpAccessCountMessage();
        }

        private static void MapCostToPurview(Purview purv, List<ResourceCost> costs)
        {
            bool found = false;
            purv.Costs.Clear();
            purv.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                // either its a purview acc object or it is something in the managed resource group (which doesnt have 'purview/accounts' in its ResourceId)
                if ((!cost.ResourceId.Contains(purv.properties.managedResourceGroupName))
                    && (!cost.ResourceId.Contains(@"purview/accounts/"))
                    && (!cost.ServiceName.Contains("purview")))
                {
                    //_logger.Error(cost.ResourceId);
                    continue;
                }

                string costPurvName = cost.ResourceId.Substring(cost.ResourceId.IndexOf("purview/accounts/") + 17);

                if (costPurvName == purv.name || cost.ResourceId.Contains(purv.properties.managedResourceGroupName.ToLower()))
                {
                    purv.TotalCostBilling += cost.Cost;

                    purv.Costs.Add(cost);
                    found = true;
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for Purview {purv.name}?");
            }
        }

    }

}
