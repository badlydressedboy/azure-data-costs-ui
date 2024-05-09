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
using Azure.Costs.Common.Models.ResourceGraph;

namespace Azure.Costs.Ui.Wpf.Vm
{
    public class ResourcesTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<Resource> ResourceList { get; private set; } = new ObservableCollection<Resource>();

        private bool isResourcesQueryBusy;
        public bool IsResourcesQueryBusy
        {
            get => isResourcesQueryBusy;
            set
            {
                SetProperty(ref isResourcesQueryBusy, value);
            }
        }


        #endregion

        #region Filters

        public Filter ResourceNameFilter { get; set; } = new Filter();
      
        #endregion

        
        public ResourcesTabVm()
        {
            _filterList.Add(ResourceNameFilter);
        }

        public async Task RefreshResources(List<Subscription> selectedSubscriptions)
        {
            if (IsResourcesQueryBusy) return;
            IsResourcesQueryBusy = true;

            try
            {
                //SyncSelectedSubs();
                ClearFilterItems();
                ResourceList.Clear();

            
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
                    //if (!sub.ReadObjects) continue; // ignore this subscription
                    //foreach (var purv in sub.Purviews)
                    //{
                    //    MapCostToPurview(purv, sub.ResourceCosts);
                        
                    //    foreach (var c in purv.Costs) totalPurviewCosts += c.Cost;
                        
                    //    foreach (var tag in purv.TagsList) TagsFilter.AddSelectableItem(tag);

                    //    ResourceGroupFilter.AddSelectableItem(purv.resourceGroup);
                    //    SubscriptionFilter.AddSelectableItem(purv.Subscription.displayName);
                    //    LocationFilter.AddSelectableItem(purv.location);
                    //}

                    //// only add to grid after all filters have been added
                    //sub.Purviews.ForEach(purv=> PurviewList.Add(purv));
                }
               // TotalCostsText = totalPurviewCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsResourcesQueryBusy = false;
           // UpdateHttpAccessCountMessage();
        }

    }

}
