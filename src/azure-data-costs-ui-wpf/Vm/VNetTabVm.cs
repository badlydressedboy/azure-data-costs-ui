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
    public class VNetTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<VNet> VNetList { get; private set; } = new ObservableCollection<VNet>();

        private bool isVNetQueryBusy;
        public bool IsVNetQueryBusy
        {
            get => isVNetQueryBusy;
            set
            {
                SetProperty(ref isVNetQueryBusy, value);
            }
        }


        #endregion

        #region Filters

      
        
        #endregion

       
        public VNetTabVm()
        {
           
        }

        public async Task RefreshVNets(List<Subscription> selectedSubscriptions)
        {
            if (IsVNetQueryBusy) return;
            IsVNetQueryBusy = true;

            try
            {
                //SyncSelectedSubs();

                VNetList.Clear();

                decimal totalVNetCosts = 0;

                await Parallel.ForEachAsync(selectedSubscriptions
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            if (!sub.ReadObjects) return; // ignore this subscription

                            await APIAccess.GetVirtualNetworks(sub);

                            foreach (var vnet in sub.VNets)
                            {                              
                                foreach (var tag in vnet.TagsList) TagsFilter.AddSelectableItem(tag);

                                ResourceGroupFilter.AddSelectableItem(vnet.resourceGroup);
                                SubscriptionFilter.AddSelectableItem(vnet.Subscription.displayName);
                            }

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                sub.VNets.ForEach(vnet => { VNetList.Add(vnet); });
                            });

                            if (sub.VNets.Count > 0)
                            {
                                await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.VNet, forceRead: true);
                            }

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var vnet in sub.VNets)
                                {
                                    MapCostToVNet(vnet, sub.ResourceCosts);

                                    foreach (var c in vnet.Costs)
                                    {
                                        totalVNetCosts += c.Cost;
                                    }                               
                                }
                            });
                        });

                TotalCostsText = totalVNetCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsVNetQueryBusy = false;
            //UpdateHttpAccessCountMessage();
        }


        private static void MapCostToVNet(VNet vnet, List<ResourceCost> costs)
        {
            bool found = false;
            vnet.Costs.Clear();
            vnet.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                if (cost.ResourceId.Contains(vnet.resourceGroup.ToLower()))
                {
                    // todo - get component name from end of resourceId
                    if (!cost.ResourceId.Contains(@"network/")) continue;

                    string resourceName = "";
                    if (cost.ResourceId.Contains(@"privateendpoint"))
                    {
                        resourceName = cost.ResourceId.Substring(cost.ResourceId.IndexOf("privateendpoints/") + 17);
                    }
                    //string 
                    cost.ResourceName = resourceName;

                    //if (vnet.name == costVnetName)
                    //{
                    vnet.TotalCostBilling += cost.Cost;

                    vnet.Costs.Add(cost);
                    found = true;
                    //}
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for vnet {vnet.name}?");
            }
        }

    }

}
