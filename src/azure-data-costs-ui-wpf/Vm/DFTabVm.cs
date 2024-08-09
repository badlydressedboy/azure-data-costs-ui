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
    public class DFTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<DataFactory> DataFactoryList { get; private set; } = new ObservableCollection<DataFactory>();

        private bool isADFQueryBusy;
        public bool IsADFQueryBusy
        {
            get => isADFQueryBusy;
            set
            {
                SetProperty(ref isADFQueryBusy, value);
            }
        }


        #endregion

        #region Specific Filters

        //public Filter DFNameFilter { get; set; } = new Filter();

        #endregion

      
        public DFTabVm()
        {
            //_logger.Info("DBTabVm ctor");

            //_filterList.Add(DFNameFilter);
       
        }

        public async Task RefreshDataFactories(List<Subscription> selectedSubscriptions)
        {
            if (IsADFQueryBusy) return;
            IsADFQueryBusy = true;

            try
            {
                // SyncSelectedSubs();
                DataFactoryList.Clear();

                ClearFilterItems();
                
                decimal totalADFCosts = 0;

                await Parallel.ForEachAsync(selectedSubscriptions
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            await APIAccess.GetDataFactories(sub);
                            if (sub.DataFactories.Count > 0)
                            {
                                await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.DataFactory, forceRead:true);
                            }
                        });

                foreach (var sub in selectedSubscriptions)
                {
                    if (!sub.ReadObjects) continue; // ignore this subscription

                    // build filters before adding item to grid
                    foreach (var df in sub.DataFactories)
                    { 
                        MapCostToDF(df, sub.ResourceCosts);                        
                        foreach (var c in df.Costs) totalADFCosts += c.Cost;       
                        
                        foreach (var tag in df.TagsList) TagsFilter.AddSelectableItem(tag);

                        ResourceGroupFilter.AddSelectableItem(df.resourceGroup);
                        LocationFilter.AddSelectableItem(df.location);
                        SubscriptionFilter.AddSelectableItem(df.Subscription.displayName);
                    }
                    
                    sub.DataFactories.ForEach(df => DataFactoryList.Add(df));
                    
                }
                TotalCostsText = totalADFCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsADFQueryBusy = false;
            //UpdateHttpAccessCountMessage();
        }

        private static void MapCostToDF(DataFactory df, List<ResourceCost> costs)
        {
            bool found = false;
            df.Costs.Clear();
            df.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                if (cost.ResourceId.Contains(df.name.ToLower()) && cost.ResourceId.Contains(df.resourceGroup.ToLower()))
                {
                    df.TotalCostBilling += cost.Cost;

                    df.Costs.Add(cost);
                    found = true;
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for df {df.name}?");
            }
        }

        

    }

}
