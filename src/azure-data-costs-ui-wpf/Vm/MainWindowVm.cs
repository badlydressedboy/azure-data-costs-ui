﻿using Azure.Costs.Common;
using Azure.Costs.Common.Models.Rest;
using Azure.Costs.Common.Models.SQL;
using Azure.Costs.Ui.Wpf.Vm;
using CommunityToolkit.Mvvm.ComponentModel;
using DataEstateOverview;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Azure.Costs.Ui.Wpf
{
    public class MainWindowVm : ObservableObject
    {

        #region vars

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DBTabVm DBTabVm { get; set; } = new DBTabVm();
        public List<Subscription> SelectedSubscriptions { get; set; } = new List<Subscription>();
        public ObservableCollection<Subscription> DetectedSubscriptions { get; set; } = new ObservableCollection<Subscription>();        
        public ObservableCollection<DataFactory> DataFactoryList { get; private set; } = new ObservableCollection<DataFactory>();
        public ObservableCollection<StorageAccount> StorageList { get; private set; } = new ObservableCollection<StorageAccount>();
        public ObservableCollection<VNet> VNetList { get; private set; } = new ObservableCollection<VNet>();
        public ObservableCollection<Purview> PurviewList { get; private set; } = new ObservableCollection<Purview>();
        public ObservableCollection<VM> VMList { get; private set; } = new ObservableCollection<VM>();
                

        public static string PortalUrl;

        public bool _readAllObjectsCheck { get; set; }
        public bool ReadAllObjectsCheck
        {
            get { return _readAllObjectsCheck; }
            set
            {
                if (_readAllObjectsCheck == value) return;

                _readAllObjectsCheck = value;
                foreach (var sub in DetectedSubscriptions)
                {
                    sub.ReadObjects = value;
                }

                OnPropertyChanged("ReadAllObjectsCheck");
            }
        }
        public bool _readAllCostsCheck { get; set; }
        public bool ReadAllCostsCheck
        {
            get { return _readAllCostsCheck; }
            set
            {
                if (_readAllCostsCheck == value) return;

                _readAllCostsCheck = value;
                foreach (var sub in DetectedSubscriptions)
                {
                    sub.ReadCosts = value;
                }

                OnPropertyChanged("ReadAllCostsCheck");
            }
        }

        private string? testLoginErrorMessage;
        public string? TestLoginErrorMessage
        {
            get => testLoginErrorMessage;
            set => SetProperty(ref testLoginErrorMessage, value);
        }
        

        
        
        private string totalADFCostsText;
        public string TotalADFCostsText
        {
            get => totalADFCostsText;
            set => SetProperty(ref totalADFCostsText, value);        
        }
        private string totalStorageCostsText;
        public string TotalStorageCostsText
        {
            get => totalStorageCostsText;
            set => SetProperty(ref totalStorageCostsText, value);
        }
        
        private string totalVNetCostsText;
        public string TotalVNetCostsText
        {
            get => totalVNetCostsText;
            set => SetProperty(ref totalVNetCostsText, value);
        }
        private string totalVMCostsText;
        public string TotalVMCostsText
        {
            get => totalVMCostsText;
            set => SetProperty(ref totalVMCostsText, value);
        }
        private string totalPurviewCostsText;
        public string TotalPurviewCostsText
        {
            get => totalPurviewCostsText;
            set => SetProperty(ref totalPurviewCostsText, value);
        }

        private string dataFactoryErrorMessage;
        public string DataFactoryErrorMessage
        {
            get => dataFactoryErrorMessage;
            set
            {

                SetProperty(ref dataFactoryErrorMessage, value);
                if (string.IsNullOrEmpty(value))
                {
                    IsDataFactoryErrorMessageVisible = false;
                }
                else
                {
                    IsDataFactoryErrorMessageVisible = true;
                }
            }
        }

        private bool isDataFactoryErrorMessageVisible;

        public bool IsDataFactoryErrorMessageVisible
        {
            get => isDataFactoryErrorMessageVisible;
            set
            {
                SetProperty(ref isDataFactoryErrorMessageVisible, value);
            }
        }
        private bool isRestQueryBusy;
        public bool IsRestQueryBusy
        {
            get => isRestQueryBusy;
            set
            {
                SetProperty(ref isRestQueryBusy, value);
            }
        }
        
        private bool isGetSubscriptionsBusy;
        public bool IsGetSubscriptionsBusy
        {
            get => isGetSubscriptionsBusy;
            set
            {
                SetProperty(ref isGetSubscriptionsBusy, value);
            }
        }

        private bool isStorageQueryBusy;
        public bool IsStorageQueryBusy
        {
            get => isStorageQueryBusy;
            set
            {
                SetProperty(ref isStorageQueryBusy, value);
            }
        }
        private bool isADFQueryBusy;
        public bool IsADFQueryBusy
        {
            get => isADFQueryBusy;
            set
            {
                SetProperty(ref isADFQueryBusy, value);
            }
        }
        private bool isVNetQueryBusy;
        public bool IsVNetQueryBusy
        {
            get => isVNetQueryBusy;
            set
            {
                SetProperty(ref isVNetQueryBusy, value);
            }
        }
        private bool isVMQueryBusy;
        public bool IsVMQueryBusy
        {
            get => isVMQueryBusy;
            set
            {
                SetProperty(ref isVMQueryBusy, value);
            }
        }
        private bool isPurviewQueryBusy;
        public bool IsPurviewQueryBusy
        {
            get => isPurviewQueryBusy;
            set
            {
                SetProperty(ref isPurviewQueryBusy, value);
            }
        }
        private bool isTestLoginBusy;
        public bool IsTestLoginBusy
        {
            get => isTestLoginBusy;
            set
            {
                SetProperty(ref isTestLoginBusy, value);
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
        private string restErrorMessage;
        public string RestErrorMessage
        {
            get => restErrorMessage;
            set  {

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

        private AzServer selectedAzServer;

        public AzServer SelectedAzServer
        {
            get => selectedAzServer;
            set => SetProperty(ref selectedAzServer, value);
        }

        private AzDB selectedAzDB;

        public AzDB SelectedAzDB
        {
            get => selectedAzDB;
            set {
                    SetProperty(ref selectedAzDB, value);
                    if(selectedAzDB.ParentAzServer != null) SelectedAzServer = selectedAzDB.ParentAzServer;
                }
        }

        private bool isQueryingDatabase;

        public bool IsQueryingDatabase
        {
            get => isQueryingDatabase;
            set
            {
                SetProperty(ref isQueryingDatabase, value);                
            }
        }

        private bool showRunningSessionsOnly;

        public bool ShowRunningSessionsOnly
        {
            get => showRunningSessionsOnly;
            set => SetProperty(ref showRunningSessionsOnly, value);            
        }

        public void UpdateHttpAccessCountMessage()
        {
            if (APIAccess.HttpClient == null) return;

            HttpAccessCountMessage = $"Total Rest Calls: {APIAccess.HttpClient.HttpCallCount}";
        }
        private string? httpAccessCountMessage;
        public string? HttpAccessCountMessage
        {
            get => httpAccessCountMessage;
            set => SetProperty(ref httpAccessCountMessage, value);
        }

        private string? selectSubscriptionsCountMessage;
        public string? SelectSubscriptionsCountMessage
        {
            get => selectSubscriptionsCountMessage;
            set => SetProperty(ref selectSubscriptionsCountMessage, value);
        }
        #endregion

        public MainWindowVm(){

            //Subscriptions.Add(new Subscription("a5be5e3e-da5c-45f5-abe9-9591a51fccfa"));//, this
            //Subscriptions.Add(new Subscription("151b40b6-6164-4053-9884-58a8d3151fe6"));//, this
            IsRestErrorMessageVisible = false;            
        }
        public async Task TestLogin()
        {
            if (IsTestLoginBusy) return;
            IsTestLoginBusy = true;
            IsRestQueryBusy = true;
            TestLoginErrorMessage = "";

            TestLoginErrorMessage = await APIAccess.TestLogin();
            PortalUrl = $@"https://portal.azure.com/#@{APIAccess.DefaultDomain}/resource/subscriptions/";

            IsTestLoginBusy = false;
            IsRestQueryBusy = false;
            UpdateHttpAccessCountMessage();
        }
        public async Task GetSubscriptions()
        {
            if (IsGetSubscriptionsBusy) return;
            IsGetSubscriptionsBusy = true;

            try
            {
                SelectedSubscriptions.Clear();
                DetectedSubscriptions.Clear();

                var subsList = await APIAccess.GetSubscriptions();
                //if (subsList.co) { }
                if (subsList == null )//|| subsList.Count == 0
                {
                    _logger.Info("No subscriptions! Are you logged into Azure?");
                    RestErrorMessage = "No subscriptions! Are you logged into Azure?";
                    return;
                }

                foreach (Subscription sub in subsList)
                {
                    DetectedSubscriptions.Add(sub);

                    var existing = App.Config.Subscriptions.FirstOrDefault(x => x.Name == sub.displayName);
                    if (existing != null)
                    {
                        sub.ReadObjects = existing.ReadObjects;
                        sub.ReadCosts = existing.ReadCosts;
                    }
                    else
                    {
                        App.Config.Subscriptions.Add(new DbMeta.Ui.Wpf.Config.ConfigSubscription() { Name = sub.displayName });
                    }

                    if (sub.ReadObjects) SelectedSubscriptions.Add(sub);
                }
                App.SaveConfig();
                UpdateHttpAccessCountMessage();
                UpdateAllSubsChecks();
            }
            catch (Exception ex) {
                _logger.Error(ex); 
            }
            IsGetSubscriptionsBusy = false;
        }

        public void UpdateAllSubsChecks()
        {
            // set the all checkbox properties if all or nothing
            bool allObjects = true;
            bool allCosts = true;
            foreach (Subscription sub in DetectedSubscriptions)
            {
                if (!sub.ReadObjects) allObjects = false;
                if (!sub.ReadCosts) allCosts = false;
            }
            ReadAllObjectsCheck = allObjects;
            ReadAllCostsCheck = allCosts;
        }

        public void SyncSelectedSubs()
        {
            SelectedSubscriptions = DetectedSubscriptions.Where(x => x.ReadObjects).ToList();
            SelectSubscriptionsCountMessage = SelectedSubscriptions.Count + " Selected Subscriptions";
        }

        // db/sql server data plus costs
        

        public async Task RefreshStorage()
        {
            if (IsStorageQueryBusy) return;
            IsStorageQueryBusy = true;

            try
            {
                SyncSelectedSubs();
                StorageList.Clear();

                decimal totalStorageCosts = 0;

                await Parallel.ForEachAsync(SelectedSubscriptions
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
                                }
                            });
                        });

                TotalStorageCostsText = totalStorageCosts.ToString("N2");
            }
            catch(Exception ex)
            {
                _logger.Error(ex);   
            }
            IsStorageQueryBusy = false;
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshDataFactories()
        {
            if (IsADFQueryBusy) return;
            IsADFQueryBusy = true;

            try
            {
                SyncSelectedSubs();
                DataFactoryList.Clear();

            decimal totalADFCosts = 0;

            await Parallel.ForEachAsync(SelectedSubscriptions
                    , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                    , async (sub, y) =>
                    {
                        await APIAccess.GetDataFactories(sub);
                        if (sub.DataFactories.Count > 0 && sub.ResourceCosts.Count == 0 && sub.ReadCosts) await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.DataFactory);
                    });

                foreach (var sub in SelectedSubscriptions)
                {
                    if (!sub.ReadObjects) continue; // ignore this subscription
                    foreach (var df in sub.DataFactories)
                    {
                        MapCostToDF(df, sub.ResourceCosts);
                        DataFactoryList.Add(df);

                        foreach (var c in df.Costs)
                        {
                            totalADFCosts += c.Cost;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex); 
            }
            IsADFQueryBusy = false;
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshVNets()
        {
            if (IsVNetQueryBusy) return;
            IsVNetQueryBusy = true;

            try
            {
                SyncSelectedSubs();

                VNetList.Clear();

                decimal totalVNetCosts = 0;

                await Parallel.ForEachAsync(SelectedSubscriptions
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            if (!sub.ReadObjects) return; // ignore this subscription

                            await APIAccess.GetVirtualNetworks(sub);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                sub.VNets.ForEach(vnet => { VNetList.Add(vnet); });
                            });

                            if (sub.VNets.Count > 0 && sub.ResourceCosts.Count == 0 && sub.ReadCosts) await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.VNet);

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

                TotalVNetCostsText = totalVNetCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsVNetQueryBusy = false;
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshVMs()
        {
            if (IsVMQueryBusy) return;

            //{
                IsVMQueryBusy = true;
            //}
            //);
           
            try
            {
                SyncSelectedSubs();

                VMList.Clear();

                decimal totalVMCosts = 0;

                await Parallel.ForEachAsync(SelectedSubscriptions
                        , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                        , async (sub, y) =>
                        {
                            if (!sub.ReadObjects) return; // ignore this subscription

                            await APIAccess.GetVirtualMachines(sub);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                sub.VMs.ForEach(vm => { VMList.Add(vm); });
                            });

                            if (sub.VMs.Count > 0 && sub.ResourceCosts.Count == 0 && sub.ReadCosts)
                            {
                                await APIAccess.GetSubscriptionCosts(sub, APIAccess.CostRequestType.VM);
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

               
                TotalVMCostsText = totalVMCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsVMQueryBusy = false;
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshPurview()
        {
            if (IsPurviewQueryBusy) return;
            IsPurviewQueryBusy = true;

            try
            {
                SyncSelectedSubs();

                PurviewList.Clear();

                decimal totalPurviewCosts = 0;


                var subsCopy = SelectedSubscriptions.ToList();
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
                        PurviewList.Add(purv);

                        foreach (var c in purv.Costs)
                        {
                            totalPurviewCosts += c.Cost;
                        }
                    }
                }
                TotalPurviewCostsText = totalPurviewCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsPurviewQueryBusy = false;
            UpdateHttpAccessCountMessage();
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

                    var lastSlashPos = cost.ResourceId.LastIndexOf("/")+1;
                    string costVmName = cost.ResourceId.Substring(lastSlashPos, (cost.ResourceId.Length - lastSlashPos) );

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
                }catch(Exception ex)
                {
                    _logger.Error($"VM costs error: {ex}");
                }
            }
            if (!found)
            {
                _logger.Info($"why no cost for VM {vm.name}?");
            }
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
            UpdateHttpAccessCountMessage();
            TotalPotentialVmSavingAmount = VMList.Sum(x => x.PotentialSavingAmount);
        }
        public async Task RefreshSqlDb()
        {
            if(SelectedAzDB == null) return;
            if (IsQueryingDatabase) return;
            IsQueryingDatabase = true;

            try
            {
                Task[] tasks = new Task[2];

                tasks[0] = Task.Run(async () =>
                {
                    await SelectedAzDB.Refresh();
                });
                tasks[1] = Task.Run(async () =>
                {
                    await SelectedAzServer.RefreshMetaData();
                });
           
                await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            IsQueryingDatabase = false;
            UpdateHttpAccessCountMessage();
        }

        public void SaveSubscriptionOptions()
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        public void LoadSubscriptionOptions()
        {
            try
            {

            }catch(Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }

        

    }

    public class SelectableString : ObservableObject
    {
        
        public string StringValue { get; set; }
        
        private bool isSelected;
        public bool IsSelected {
            get => isSelected;

            set
            {
                SetProperty(ref isSelected, value);
            }
        }
    }

}