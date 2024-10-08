﻿using Azure.Costs.Common;
using Azure.Costs.Common.Models.Rest;
using Azure.Costs.Common.Models.SQL;
using Azure.Costs.Ui.Wpf.Vm;
using BusyIndicator;
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

        public IndicatorType BusyIndicatorType { get; set; } = IndicatorType.Grid;

        public DBTabVm DBTabVm { get; set; } = new DBTabVm();
        public StorageTabVm StorageTabVm { get; set; } = new StorageTabVm();
        public VNetTabVm VNetTabVm { get; set; } = new VNetTabVm();
        public PurviewTabVm PurviewTabVm { get; set; } = new PurviewTabVm();

        public CosmosTabVm CosmosTabVm { get; set; } = new CosmosTabVm();

        public FabricTabVm FabricTabVm { get; set; } = new FabricTabVm();
        public DFTabVm DFTabVm { get; set; } = new DFTabVm();
        public VmTabVm VmTabVm { get; set; } = new VmTabVm();
        public ResourcesTabVm ResourcesTabVm { get; set; } = new ResourcesTabVm();

        public List<Subscription> SelectedSubscriptions { get; set; } = new List<Subscription>();
        public ObservableCollection<Subscription> DetectedSubscriptions { get; set; } = new ObservableCollection<Subscription>();                        
        
            
        public static string PortalUrl;

        public static string TenantName { get; set; }

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

        public string? SelectedDateRangeMessage
        {
            get {
                return SelectedDateRangeDayCount + " Days Selected";
            }
            
        }

        private DateTime selectedDateRangeStartDate;
        public DateTime SelectedDateRangeStartDate
        {
            get => selectedDateRangeStartDate;
            set
            {
                SetProperty(ref selectedDateRangeStartDate, value);
                OnPropertyChanged("SelectedDateRangeDayCount");
                OnPropertyChanged("SelectedDateRangeMessage");
                SyncSelectedDates();
            }
        }
        private DateTime selectedDateRangeEndDate;
        public DateTime SelectedDateRangeEndDate
        {
            get => selectedDateRangeEndDate;
            set
            {
                SetProperty(ref selectedDateRangeEndDate, value);
                OnPropertyChanged("SelectedDateRangeDayCount");
                OnPropertyChanged("SelectedDateRangeMessage");
                SyncSelectedDates();
            }
        }
 
        public int SelectedDateRangeDayCount
        {
            get { 
                return (SelectedDateRangeEndDate- SelectedDateRangeStartDate).Days;
            }
        
//            set => SetProperty(ref selectedDateRangeEndDate, value);
        }
        #endregion

        public MainWindowVm(){

            //Subscriptions.Add(new Subscription("a5be5e3e-da5c-45f5-abe9-9591a51fccfa"));//, this
            //Subscriptions.Add(new Subscription("151b40b6-6164-4053-9884-58a8d3151fe6"));//, this
            IsRestErrorMessageVisible = false;

            SelectedDateRangeStartDate = DateTime.Now.AddDays(-30);
            SelectedDateRangeEndDate = DateTime.Now;
        }
        public void SyncSelectedDates()
        {
            APIAccess.SelectedStartDate = SelectedDateRangeStartDate;
            APIAccess.SelectedEndDate = SelectedDateRangeEndDate;
        }
        public async Task TestLogin()
        {
            if (IsTestLoginBusy) return;
            IsTestLoginBusy = true;
            IsRestQueryBusy = true;
            TestLoginErrorMessage = "";

            TestLoginErrorMessage = await APIAccess.TestLogin();
            PortalUrl = $@"https://portal.azure.com/#@{APIAccess.DefaultDomain}/resource/subscriptions/";
            TenantName = APIAccess.TenantName;

            IsTestLoginBusy = false;
            IsRestQueryBusy = false;

            await GetSubscriptions();

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
        
        public void UpdateDbsScannedByPurview()
        {
            if (PurviewTabVm.PurviewList.Count == 0) return;

            // defo have a purview so can safely say dbs are not scanned by something that realy does exist
            foreach (var db in DBTabVm.RestSqlDbList)
            {
                db.IsScannedByPurview = "No";
            }
            foreach (var db in CosmosTabVm.CosmosList)
            {
                db.IsScannedByPurview = "No";
            }

            foreach (var purv in PurviewTabVm.PurviewList)
            {
                // first should be only one
                foreach(var source in purv.DataSourceGridRows)
                {
                    // process based on source DsKind - careful as some types have NULL fields where appropriate

                    if (source.DsKind.Contains("AzureSql"))
                    {
                        foreach (var db in DBTabVm.RestSqlDbList)
                        {
                            if (source.DsEndPoint.Contains(db.serverName) && db.name == source.ScanDatabaseName)
                            {
                                db.IsScannedByPurview = "Yes";
                            }
                        }
                    }

                    if (source.DsKind == "AzureCosmosDb")
                    {
                        foreach (var db in CosmosTabVm.CosmosList)
                        {
                            if (source.DsResourceName.Contains(db.name))//&& db.name == source.ScanDatabaseName)
                            {
                                db.IsScannedByPurview = "Yes";
                            }
                        }
                    }
                }
            }
            UpdateHttpAccessCountMessage();
        }
        public async Task RefreshDatabases()
        {
            await DBTabVm.RefreshDatabases(SelectedSubscriptions);
            UpdateDbsScannedByPurview();
            UpdateHttpAccessCountMessage();
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

        public async Task RefreshStorage()
        {
            await StorageTabVm.RefreshStorage(SelectedSubscriptions);
            UpdateHttpAccessCountMessage();
        }


        public async Task RefreshVNets()
        {
            await VNetTabVm.RefreshVNets(SelectedSubscriptions);
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshVms()
        {
            await VmTabVm.RefreshVMs(SelectedSubscriptions);
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshPurview()
        {
            await PurviewTabVm.RefreshPurview(SelectedSubscriptions);
            UpdateDbsScannedByPurview();
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshCosmos()
        {
            await CosmosTabVm.RefreshCosmos(SelectedSubscriptions);
            UpdateDbsScannedByPurview();
            UpdateHttpAccessCountMessage();
        }
        public async Task RefreshFabric()
        {
            await FabricTabVm.RefreshFabric(SelectedSubscriptions);
            UpdateHttpAccessCountMessage();
        }
        public async Task RefreshDataFactories()
        {
            await DFTabVm.RefreshDataFactories(SelectedSubscriptions);
            UpdateHttpAccessCountMessage();
        }

        public async Task RefreshResources()
        {
            await ResourcesTabVm.RefreshResources(SelectedSubscriptions);
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
