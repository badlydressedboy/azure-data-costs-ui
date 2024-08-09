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
    public class VmTabVm : TabVmBase
    {
        #region Properties
        public ObservableCollection<VM> VMList { get; private set; } = new ObservableCollection<VM>();

        private bool isVMQueryBusy;
        public bool IsVMQueryBusy
        {
            get => isVMQueryBusy;
            set
            {
                SetProperty(ref isVMQueryBusy, value);
            }
        }
        private bool isVmSpendAnalysisBusy;
        public bool IsVmSpendAnalysisBusy
        {
            get => isVmSpendAnalysisBusy;
            set
            {
                SetProperty(ref isVmSpendAnalysisBusy, value);
            }
        }
        private bool hasVmSpendAnalysisBeenPerformed;
        public bool HasVmSpendAnalysisBeenPerformed
        {
            get => hasVmSpendAnalysisBeenPerformed;
            set
            {
                SetProperty(ref hasVmSpendAnalysisBeenPerformed, value);
            }
        }

        private decimal _totalPotentialVmSavingAmount;
        public decimal TotalPotentialVmSavingAmount
        {
            get { return _totalPotentialVmSavingAmount; }
            set
            {
                _totalPotentialVmSavingAmount = value;
                OnPropertyChanged("TotalPotentialVmSavingAmount");
            }
        }

        #endregion

        #region Filters



        #endregion



        public VmTabVm()
        {

        }

        public async Task RefreshVMs(List<Subscription> selectedSubscriptions)
        {
            if (IsVMQueryBusy) return;

            //{
            IsVMQueryBusy = true;
            //}
            //);

            try
            {
                //SyncSelectedSubs();

                VMList.Clear();
                ClearFilterItems();
                decimal totalVMCosts = 0;

                await Parallel.ForEachAsync(selectedSubscriptions
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            if (!sub.ReadObjects) return; // ignore this subscription

                            await APIAccess.GetVirtualMachines(sub);

                            // build filters before adding item to grid
                            sub.VMs.ForEach(vm => {
                                foreach (var tag in vm.TagsList) TagsFilter.AddSelectableItem(tag);

                                ResourceGroupFilter.AddSelectableItem(vm.resourceGroup);
                                SubscriptionFilter.AddSelectableItem(vm.Subscription.displayName);
                                LocationFilter.AddSelectableItem(vm.location);
                            });

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                sub.VMs.ForEach(vm => {                                 
                                    VMList.Add(vm); 
                                });
                            });

                            if (sub.VMs.Count > 0)
                            {
                                await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.VM, forceRead:true);
                            }

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                foreach (var vm in sub.VMs)
                                {
                                    MapCostToVM(vm, sub.ResourceCosts);

                                    foreach (var c in vm.Costs)
                                    {
                                        totalVMCosts += c.Cost;
                                    }                                  
                                }
                            });
                        });


                TotalCostsText = totalVMCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsVMQueryBusy = false;
            //UpdateHttpAccessCountMessage();
        }

        private static void MapCostToVM(VM vm, List<ResourceCost> costs)
        {
            bool found = false;
            vm.Costs.Clear();
            vm.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                try
                {
                    if ((!cost.ResourceId.Contains(@"virtualmachines/")) && (!cost.ResourceId.Contains(@"disks/"))) continue;

                    var lastSlashPos = cost.ResourceId.LastIndexOf("/") + 1;
                    string costVmName = cost.ResourceId.Substring(lastSlashPos, (cost.ResourceId.Length - lastSlashPos));

                    if ((costVmName.ToLower() == vm.name.ToLower()) && cost.ResourceId.Contains(vm.resourceGroup.ToLower()))
                    {
                        if (cost.ServiceName == "Virtual Machines" || cost.ServiceName == "Storage" || cost.ServiceName == "Bandwidth" || cost.ServiceName == "Virtual Network")
                        {
                            vm.TotalCostBilling += cost.Cost;
                            vm.Costs.Add(cost);
                            found = true;
                        }
                    }
                    else
                    {
                        // disk name cost does not match vm name< which means it MAY be a disk
                        if (cost.ResourceId.ToLower().Contains(vm.resourceGroup.ToLower()) && cost.ResourceType.ToLower().Contains("disks"))// too wide - need actual vm
                        {
                            foreach (var disk in vm.properties.storageProfile.dataDisks)
                            {
                                if (cost.ResourceId.ToLower().Contains(disk.name.ToLower()))
                                {
                                    vm.TotalCostBilling += cost.Cost;
                                    vm.Costs.Add(cost);
                                    found = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"VM costs error: {ex}");
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for VM {vm.name}?");
            }
        }

        public async Task AnalyseVmSpend()
        {
            if (IsVmSpendAnalysisBusy) return;
            IsVmSpendAnalysisBusy = true;

            RestErrorMessage = "";
            //decimal totalPotentialSaving = 0;
            TotalPotentialVmSavingAmount = 0;
            try
            {
                await Parallel.ForEachAsync(VMList.OrderByDescending(x => x.TotalCostBilling)
                    , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                    , async (vm, y) =>
                    {
                        vm.SpendAnalysisStatus = "Analysing...";
                        vm.OverSpendFromMaxPcString = "?";

                        await APIAccess.GetVmMetrics(vm);


                        vm.SpendAnalysisStatus = "Complete";
                    });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsVmSpendAnalysisBusy = false;
            HasVmSpendAnalysisBeenPerformed = true;
            //UpdateHttpAccessCountMessage();
            TotalPotentialVmSavingAmount = VMList.Sum(x => x.PotentialSavingAmount);
        }
    }

}
