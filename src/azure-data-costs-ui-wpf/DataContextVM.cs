using CommunityToolkit.Mvvm.ComponentModel;
using DataEstateOverview.Models.Rest;
using DataEstateOverview.Models.SQL;
using DbMeta.Ui.Wpf.Models.Rest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataEstateOverview
{
    public class DataContextVM : ObservableObject
    {        
        public List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ObservableCollection<Subscription> DetectedSubscriptions { get; set; } = new ObservableCollection<Subscription>();

        public ObservableCollection<RestSqlDb> RestSqlDbList { get; private set; } = new ObservableCollection<RestSqlDb>();
        public ObservableCollection<DataFactory> DataFactoryList { get; private set; } = new ObservableCollection<DataFactory>();
        public ObservableCollection<StorageAccount> StorageList { get; private set; } = new ObservableCollection<StorageAccount>();
        public ObservableCollection<VNet> VNetList { get; private set; } = new ObservableCollection<VNet>();
        public ObservableCollection<VM> VMList { get; private set; } = new ObservableCollection<VM>();

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
        private bool isTestLoginBusy;
        public bool IsTestLoginBusy
        {
            get => isTestLoginBusy;
            set
            {
                SetProperty(ref isTestLoginBusy, value);
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


        public DataContextVM(){

            Subscriptions.Add(new Subscription("a5be5e3e-da5c-45f5-abe9-9591a51fccfa"));
            Subscriptions.Add(new Subscription("151b40b6-6164-4053-9884-58a8d3151fe6"));
            IsRestErrorMessageVisible = false;            
        }
        public async Task TestLogin()
        {
            if (IsTestLoginBusy) return;
            IsTestLoginBusy = true;
            TestLoginErrorMessage = "";

            TestLoginErrorMessage = await APIAccess.TestLogin();

            IsTestLoginBusy = false;
        }
        public async Task GetSubscriptions()
        {
            Subscriptions.Clear();
            DetectedSubscriptions.Clear();

            var subsList = await APIAccess.GetSubscriptions();

            foreach (Subscription sub in subsList)
            {
                DetectedSubscriptions.Add(sub);
                
                var existing = App.Config.Subscriptions.FirstOrDefault(x => x.Name == sub.displayName);
                if(existing != null)
                {
                    sub.ReadObjects = existing.ReadObjects;
                    sub.ReadCosts = existing.ReadCosts;
                }
                else
                {
                    App.Config.Subscriptions.Add(new DbMeta.Ui.Wpf.Config.ConfigSubscription() { Name = sub.displayName });
                }

                if (sub.ReadObjects) Subscriptions.Add(sub);
            }
            App.SaveConfig();
        }
        public async Task RefreshRest()
        {
            if (IsRestQueryBusy) return;
            IsRestQueryBusy = true;
            RestSqlDbList.Clear();
            DataFactoryList.Clear();
            StorageList.Clear();
            VNetList.Clear();
            RestErrorMessage = "";
            DataFactoryErrorMessage = "";
            TotalADFCostsText = "";
            decimal totalADFCosts = 0;
            decimal totalStorageCosts = 0;
            decimal totalVNetCosts = 0;
            decimal totalVMCosts = 0;

            try
            {            
                await Parallel.ForEachAsync(Subscriptions
                    , new ParallelOptions() { MaxDegreeOfParallelism = 10 }
                    , async (sub, y) =>
                {
                    await APIAccess.RefreshSubscription(sub);                   
                });

                // on ui thread
                foreach (var sub in Subscriptions)
                {
                    if(!sub.ReadObjects) continue; // ignore this subscription

                    foreach (var s in sub.SqlServers)
                    {
                        foreach (var db in s.Dbs)
                        {
                            MapCostToDb(db, sub.ResourceCosts);
                            RestSqlDbList.Add(db);
                        }
                    }

                    foreach (var df in sub.DataFactories)
                    {
                        MapCostToDF(df, sub.ResourceCosts);
                        DataFactoryList.Add(df);

                        foreach(var c in df.Costs)
                        {
                            totalADFCosts += c.Cost;
                        }
                    }
                    
                    foreach (var sa in sub.StorageAccounts)
                    {
                        MapCostToStorage(sa, sub.ResourceCosts);
                        StorageList.Add(sa);

                        foreach (var c in sa.Costs)
                        {
                            totalStorageCosts += c.Cost;
                        }
                    }
                   
                    foreach (var vnet in sub.VNets)
                    {
                        MapCostToVNet(vnet, sub.ResourceCosts);
                        VNetList.Add(vnet);

                        foreach (var c in vnet.Costs)
                        {
                            totalVNetCosts += c.Cost;
                        }
                    }

                    foreach (var vm in sub.VMs)
                    {
                        MapCostToVM(vm, sub.ResourceCosts);
                        VMList.Add(vm);

                        foreach (var c in vm.Costs)
                        {
                            totalVMCosts += c.Cost;
                        }
                    }

                    if (!string.IsNullOrEmpty(sub.CostsErrorMessage))
                    {
                        if(!string.IsNullOrEmpty(RestErrorMessage)) RestErrorMessage += "\n";
                        RestErrorMessage += sub.CostsErrorMessage;
                    }
                }
                TotalADFCostsText = totalADFCosts.ToString("N2");
                TotalStorageCostsText = totalStorageCosts.ToString("N2");
                TotalVNetCostsText = totalVNetCosts.ToString("N2");
                TotalVMCostsText = totalVMCosts.ToString("N2");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            IsRestQueryBusy = false;
        }

        private static void MapCostToDb(RestSqlDb db, List<ResourceCost> costs)
        {
            bool found = false;
            foreach(ResourceCost cost in costs)
            {
                if (cost.ResourceId.Contains(db.name) && cost.ResourceId.Contains(db.resourceGroup))
                {
                    db.Costs.Add(cost);
                    db.TotalCostBilling += cost.Cost;
                    found = true;
                    if(db.name == "ot-prd-pay-sqldb-we-02")
                    {
                        Debug.WriteLine("test db"); ;
                    }
                }                
            }
            if (!found)
            {
                Debug.WriteLine($"why no cost for {db.name}?");
            }
        }
        private static void MapCostToDF(DataFactory df, List<ResourceCost> costs)
        {
            bool found = false;
            df.Costs.Clear();
            df.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                if (cost.ResourceId.Contains(df.name) && cost.ResourceId.Contains(df.resourceGroup))
                {
                    df.TotalCostBilling += cost.Cost;

                    df.Costs.Add(cost);
                    found = true;

                    if(df.name == "ot-dev-mi-adf-we-02")
                    {
                        Debug.WriteLine($"found our boy");
                    }
                }
            }
            if (!found)
            {
                Debug.WriteLine($"why no cost for df {df.name}?");
            }
        }
        private static void MapCostToStorage(StorageAccount sa, List<ResourceCost> costs)
        {
            bool found = false;
            sa.Costs.Clear();
            sa.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                if (cost.ResourceId.Contains(sa.name) && cost.ResourceId.Contains(sa.resourceGroup))
                {
                    sa.TotalCostBilling += cost.Cost;

                    sa.Costs.Add(cost);
                    found = true;
                }
            }
            if (!found)
            {
                Debug.WriteLine($"why no cost for df {sa.name}?");
            }
        }

        private static void MapCostToVNet(VNet vnet, List<ResourceCost> costs)
        {
            bool found = false;
            vnet.Costs.Clear();
            vnet.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                if (cost.ResourceId.Contains(vnet.resourceGroup))
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
                Debug.WriteLine($"why no cost for df {vnet.name}?");
            }
        }

        private static void MapCostToVM(VM vm, List<ResourceCost> costs)
        {
            bool found = false;
            vm.Costs.Clear();
            vm.TotalCostBilling = 0;

            foreach (ResourceCost cost in costs)
            {
                if(!cost.ResourceId.Contains(@"virtualmachines/")) continue;
                string costVmName = cost.ResourceId.Substring(cost.ResourceId.IndexOf("virtualmachines/") + 16);
            
                if (costVmName == vm.name)
                {
                    if (cost.ServiceName == "Virtual Machines" || cost.ServiceName == "Bandwidth"|| cost.ServiceName == "Virtual Network")
                    {
                        // "ResourceType
                        vm.TotalCostBilling += cost.Cost;

                        vm.Costs.Add(cost);
                        found = true;
                    }
                }
            }
            if (!found)
            {
                Debug.WriteLine($"why no cost for df {vm.name}?");
            }
        }


        public async Task RefreshSqlDb()
        {
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
                Debug.WriteLine(ex);
            }
            IsQueryingDatabase = false;
        }

        public void SaveSubscriptionOptions()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void LoadSubscriptionOptions()
        {
            

            try
            {

            }catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }

}
