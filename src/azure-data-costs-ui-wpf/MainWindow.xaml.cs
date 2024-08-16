using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Azure.Costs.Common.Models.SQL;

using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Configuration;
using Microsoft.Extensions.Configuration;

using DbMeta.Ui.Wpf.Config;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using Path = System.IO.Path;
using Microsoft.Identity.Client;
using System.Windows.Controls.Primitives;
using System.Security.Policy;
using Azure.Costs.Common.Models.Rest;
using Azure.Costs.Common;
using DataEstateOverview;
using NLog;

namespace Azure.Costs.Ui.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        List<string> connStrings = new List<string>();
        MainWindowVm vm = new MainWindowVm();
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        public MainWindow()
        {
            InitializeComponent();

            connStrings.Add("");

            DataContext = vm;
            CostDaysText.Text = APIAccess.CostDays.ToString();

        }

        private async void SQLDBRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSelectedAzSqlDB();
        }

        private async void SummaryDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LoadSelectedAzSqlDB();
        }

        private async void SummaryDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            //await RefreshData();
        }
        private async Task SetAndLoadAzDB(RestSqlDb selectedRestDb = null)
        {
            if (selectedRestDb == null)
            {
                selectedRestDb = (RestSqlDb)RestDbDataGrid.SelectedItem;
            }

            if (selectedRestDb == null)
            {
                Debug.WriteLine($"RestDbDataGrid.SelectedItem == null");
                return;
            }

            vm.SelectedAzDB = selectedRestDb.AzDB;
            vm.SelectedAzServer = selectedRestDb.AzDB.ParentAzServer;
            //TabSql.IsSelected = true;

            if (!selectedRestDb.AzDB.HasRefreshed)
            {
                await LoadSelectedAzSqlDB();
            }
        }
        private async Task LoadSelectedAzSqlDB()
        {

            try
            {

                await vm.RefreshSqlDb();

            }
            catch (Exception ex) { }

        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RestDbDataGrid_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private async void RestDbDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            //if (dg?.CurrentColumn?.DisplayIndex == 0)
            if (e.OriginalSource is TextBlock)
            {
                var tb = (TextBlock)e.OriginalSource;
                if (tb.DataContext is RestSqlDb)
                {
                    var db = (RestSqlDb)tb.DataContext;
                    SetAndLoadAzDB(db);

                }
            }
        }

        private async void RestDbDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var db = (RestSqlDb)e.Row.DataContext;

            SetAndLoadAzDB(db);
            if (!db.GotMetricsHistory)
            {
                GetDbMetrics(db);
            }
        }
        private void RestDbDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRestDb = (RestSqlDb)RestDbDataGrid.SelectedItem;

            if (selectedRestDb == null)
            {
                Debug.WriteLine($"RestDbDataGrid.SelectedItem == null");
                return;
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await TestLogin();
            await vm.GetSubscriptions();
        }
        private async Task TestLogin()
        {
            // testing api access            
            await vm.TestLogin();
            if (!string.IsNullOrEmpty(APIAccess.TenantName)) {
                LoggedInOkMessageText.Text = "Logged in to tenant: " + APIAccess.TenantName;
            }
        }
        private async Task RefreshDBs()
        {
            try
            {
                await vm.RefreshDatabases();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void DBRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshDBs();
        }

        private void RestConfigButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RestGridMenuItemSqlDetails_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedAzSqlDB();
        }

        private void SQLDBBackButton_Click(object sender, RoutedEventArgs e)
        {
            TabRest.IsSelected = true;
        }

        private void RestDbDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item == null) return;

            vm.DBTabVm.SetFilterSummaries();

            var db = e.Item as RestSqlDb;
            bool tagFilterMatched = vm.DBTabVm.IsTagFilterMatched(db.TagsList);

            if (!tagFilterMatched)
            {
                Debug.WriteLine("tag filter not matched");
            }

            if (tagFilterMatched
                && vm.DBTabVm.SoFilter.IsValueSelected(db.properties.currentServiceObjectiveName)
                && vm.DBTabVm.ServerFilter.IsValueSelected(db.serverName)
                && vm.DBTabVm.ResourceGroupFilter.IsValueSelected(db.resourceGroup)
                && vm.DBTabVm.LocationFilter.IsValueSelected(db.location)
                && vm.DBTabVm.SubscriptionFilter.IsValueSelected(db.Subscription.displayName))
            {
                e.Accepted = true;
                return;
            }
            e.Accepted = false;
        }


        private void RestDbFilterText_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void RestDbFilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(RestDbDataGrid.ItemsSource).Refresh();
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView cvs = CollectionViewSource.GetDefaultView(RestDbDataGrid.ItemsSource);
            if (cvs?.CanGroup == true)
            {
                if (cvs.GroupDescriptions.Count > 0)
                {
                    cvs.GroupDescriptions.Clear();
                }
                else
                {
                    cvs.GroupDescriptions.Clear();
                    cvs.GroupDescriptions.Add(new PropertyGroupDescription("serverName"));
                }
            }
        }

        private async void RestGridMenuItemRefresh_Click(object sender, RoutedEventArgs e)
        {
            var selectedRestDb = (RestSqlDb)RestDbDataGrid.SelectedItem;
            if (selectedRestDb == null) return;

            await APIAccess.RefreshRestDb(selectedRestDb);
        }

        private async void DataFactoriesRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshDataFactories();
        }

        private void DataFactoryDataGrid_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void DataFactoryDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void DataFactoryDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item == null) return;

            vm.DFTabVm.SetFilterSummaries();

            var db = e.Item as DataFactory;
            bool tagFilterMatched = vm.DFTabVm.IsTagFilterMatched(db.TagsList);

            if (tagFilterMatched
                && vm.DFTabVm.ResourceGroupFilter.IsValueSelected(db.resourceGroup) // how are rgroups names slightly different?
                && vm.DFTabVm.LocationFilter.IsValueSelected(db.location)
                && vm.DFTabVm.SubscriptionFilter.IsValueSelected(db.Subscription.displayName))
            {
                e.Accepted = true;
                return;
            }
            e.Accepted = false;
        }

        private void DataFactoriesCollapseCostsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseDataGrid(DataFactoryDataGrid);
        }

        private void CostDaysText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                int days = int.Parse(CostDaysText.Text + e.Text);
                if (days < 1 || days > 90)
                {
                    e.Handled = true;
                    return;
                }
                APIAccess.CostDays = days;
            }
        }

        private async void TestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await TestLogin(); 
        }

        void IgnoreOnChecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void IgnoreCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is CheckBox)
            {
                var clickedSub = (Subscription)((CheckBox)e.OriginalSource).DataContext;
                var subName = clickedSub.displayName;
                var configSub = App.Config.Subscriptions.FirstOrDefault(x => x.Name == subName);
                if (configSub == null)
                {
                    configSub = new ConfigSubscription() { Name = subName };
                    App.Config.Subscriptions.Add(configSub);
                }
                configSub.ReadObjects = clickedSub.ReadObjects;
                configSub.ReadCosts = clickedSub.ReadCosts;

                App.SaveConfig();

                vm.SyncSelectedSubs();
                //vm.UpdateAllSubsChecks(); // creates weird single box unchecking - todo fix if very bored
            }
            //Debug.WriteLine("gdg");
        }

        private void StorageDataGrid_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void StorageDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void StorageDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item == null) return;

            vm.StorageTabVm.SetFilterSummaries();

            var db = e.Item as StorageAccount;
            bool tagFilterMatched = vm.StorageTabVm.IsTagFilterMatched(db.TagsList);

            if (tagFilterMatched
                && vm.StorageTabVm.ResourceGroupFilter.IsValueSelected(db.resourceGroup)
                && vm.StorageTabVm.LocationFilter.IsValueSelected(db.location)
                && vm.StorageTabVm.SubscriptionFilter.IsValueSelected(db.Subscription.displayName)
                && vm.StorageTabVm.SkuFilter.IsValueSelected(db.sku.name)
                && vm.StorageTabVm.TierFilter.IsValueSelected(db.sku.tier)
                )
            {
                e.Accepted = true;
                return;
            }
            e.Accepted = false;
        }

        private void VNetCollapseCostsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseDataGrid(VNetDataGrid);
        }

        private void VNetDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void VNetDataGrid_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void VNetDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item == null) return;

            vm.VNetTabVm.SetFilterSummaries();

            var db = e.Item as VNet;
            bool tagFilterMatched = vm.VNetTabVm.IsTagFilterMatched(db.TagsList);

            if (tagFilterMatched
                && vm.VNetTabVm.ResourceGroupFilter.IsValueSelected(db.resourceGroup)
                && vm.VNetTabVm.LocationFilter.IsValueSelected(db.location)
                && vm.VNetTabVm.SubscriptionFilter.IsValueSelected(db.Subscription.displayName))
            {
                e.Accepted = true;
                return;
            }
            e.Accepted = false;
        }

        private void VMCollapseCostsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseDataGrid(VMDataGrid);
        }

        private void VMDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void VMDataGrid_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void VMDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item == null) return;

            vm.VmTabVm.SetFilterSummaries();

            var db = e.Item as VM;
            bool tagFilterMatched = vm.VmTabVm.IsTagFilterMatched(db.TagsList);

            if (tagFilterMatched
                && vm.VmTabVm.ResourceGroupFilter.IsValueSelected(db.resourceGroup)
                && vm.VmTabVm.LocationFilter.IsValueSelected(db.location)
                && vm.VmTabVm.SubscriptionFilter.IsValueSelected(db.Subscription.displayName))
            {
                e.Accepted = true;
                return;
            }
            e.Accepted = false;
        }

        private void PurviewCollapseCostsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseDataGrid(PurviewDataGrid);
        }

        private void PurviewDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item == null) return;

            vm.PurviewTabVm.SetFilterSummaries();

            var db = e.Item as Purview;
            bool tagFilterMatched = vm.PurviewTabVm.IsTagFilterMatched(db.TagsList);

            if (tagFilterMatched
                && vm.PurviewTabVm.ResourceGroupFilter.IsValueSelected(db.resourceGroup)
                && vm.PurviewTabVm.LocationFilter.IsValueSelected(db.location)
                && vm.PurviewTabVm.SubscriptionFilter.IsValueSelected(db.Subscription.displayName))
            {
                e.Accepted = true;
                return;
            }
            e.Accepted = false;
        }

        private void RefreshDbStatsButton_Click(object sender, RoutedEventArgs e)
        {
            GetDbMetrics();
        }

        private async void GetDbMetrics(RestSqlDb db = null)
        {
            if (db == null)
            {
                if (RestDbDataGrid.CurrentItem == null) return;
                db = (RestSqlDb)RestDbDataGrid.CurrentItem;
            }

            await APIAccess.GetDbMetrics(db); // no minutes param passed so sqlDb.MetricsHistoryDays is used            
        }

        private async void StorageRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshStorage();
        }

        private async void VNetsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshVNets();
        }

        private async void VMsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshVms();
        }

        private async void PurviewRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshPurview();
        }
        private async void CosmosRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshCosmos();
        }

        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // each tabs main grid's isvisibilitychanged event used instead
        }

        private async void DBAnalyseSpendButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.DBTabVm.AnalyseDbSpend();
        }

        private async void VMDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var vm = (VM)e.Row.DataContext;

            if (!vm.GotMetricsHistory)
            {
                GetVmMetrics(vm);
            }

        }

        private void RefreshVmStatsButton_Click(object sender, RoutedEventArgs e)
        {
            if (VMDataGrid.CurrentItem == null) return;
            var vm = (VM)VMDataGrid.CurrentItem;
            GetVmMetrics(vm);
        }
        private async void GetVmMetrics(VM vm)
        {
            if (vm == null) return;
           
            await APIAccess.GetVmMetrics(vm);    
        }

        private void Expander_Process(object sender, RoutedEventArgs e)
        {
            if (sender is Expander expander)
            {
                var row = DataGridRow.GetRowContainingElement(expander);

                row.DetailsVisibility = expander.IsExpanded ? Visibility.Visible
                                                            : Visibility.Collapsed;
            }
        }


        private void StorageCollapseCostsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseDataGrid(StorageDataGrid);
        }

        private void RowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ?
                Visibility.Visible : Visibility.Collapsed;
        }

        private void ExpandCollapseDataGrid(DataGrid dg)
        {
            if (dg.RowDetailsVisibilityMode == DataGridRowDetailsVisibilityMode.Collapsed)
            {
                dg.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible;
            }
            else
            {
                dg.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            }
        }
        private void SelectRowDetails(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is DataGridDetailsPresenter) // Like this
            {
                var row = sender as DataGridRow;
                if (row == null)
                {
                    return;
                }
                row.Focusable = true;
                row.Focus();

                var elementWithFocus = Keyboard.FocusedElement as UIElement;
                if (elementWithFocus != null)
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        private async void VMAnalyseSpendButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.VmTabVm.AnalyseVmSpend();
        }


        private void DBTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BusyIndicatorSql_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Debug.WriteLine("BusyIndicatorSql_SourceUpdated");
        }

        private void BusyIndicatorSql_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            Debug.WriteLine("BusyIndicatorSql_TargetUpdated");
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            Debug.WriteLine("Popup_Opened - what field?");
        }

        private void FilterPopupOkButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Popup_ok_clicked - update filter");
        }

        private void RestDbDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // this doesnt need to do anything as double click expands row anyway - keeping here for convenience of how to get row
            //var row = ItemsControl.ContainerFromElement((DataGrid)sender,
            //                            e.OriginalSource as DependencyObject) as DataGridRow;

        }

        private void ViewPortalDbButton_Click(object sender, RoutedEventArgs e)
        {
            PortalResource pr = (PortalResource)((Button)sender).DataContext;

            // dotnet core
            Process.Start(new ProcessStartInfo { FileName = pr.PortalResourceUrl, UseShellExecute = true });

            // dotnet framework
            //Process.Start(db.PortalResourceUrl);
        }

        private void NonGridExpandingTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // select text and return othwise the datagrid expan event will fire and annoy you to hell.
            var tb = (TextBox)sender;
            tb.SelectAll();
            tb.Focus();
            e.Handled = true;
            return;
        }

        private void ColumnFilterButton_Click(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            if (!(but.DataContext is List<SelectableString>))
            {
                _logger.Error("Filter button has not had data context set correctly to list");
                return;
            }
            DataGrid dg = FindParent<DataGrid>(but);
            if (dg == null)
            {
                Debug.WriteLine("how no dg parent");
                return;
            }
            var tagsWin = new FilterWindow((List<SelectableString>)but.DataContext);

            tagsWin.Owner = this;

            tagsWin.ShowDialog();

            CollectionViewSource.GetDefaultView(dg.ItemsSource).Refresh();
            Focus(); // filter button looks weirdly bold/focus unless you remove focus
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private async void ResourcesRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshResources();
        }

        private void DBRecomendationsButton_Click(object sender, RoutedEventArgs e)
        {
            var recWindow = new RecommendationsWindow(vm.DBTabVm);
            recWindow.Owner = this;
            recWindow.ShowDialog();
        }

        private void RecoCellClick(object sender, RoutedEventArgs e)
        {
            var db = (RestSqlDb)((DataGridCell)sender).DataContext;
            var recWindow = new RecommendationsWindow(vm.DBTabVm, db);
            recWindow.Owner = this;
            recWindow.ShowDialog();
        }

        private void RefreshDbStatsButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GetDbMetrics();
        }

        private void ViewPortalDbButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PortalResource pr = (PortalResource)((Button)sender).DataContext;

            // dotnet core
            Process.Start(new ProcessStartInfo { FileName = pr.PortalResourceUrl, UseShellExecute = true });

        }

        private void AccessmethodCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0) return;

            var cb = (ComboBoxItem)e.AddedItems[0];
            if (cb.Content?.ToString() == "Azure CLI")
            {
                APIAccess.AccessMethod = "AzureCLI";
            }
            else if (cb.Content?.ToString() == "Visual Studio")
            {
                APIAccess.AccessMethod = "VisualStudio";
            }
        }


        private void CosmosCollapseCostsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandCollapseDataGrid(CosmosDataGrid);
        }

        private void CosmosDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {

        }

        private async void CosmosLayoutGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) return;
            
            if (vm.CosmosTabVm.CosmosList.Count == 0)
            {
                await vm.RefreshCosmos();
            }
        }

        private async void ADFLayoutGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) return;

            if (vm.DFTabVm.DataFactoryList.Count == 0)
            {
                await vm.RefreshDataFactories();
            }
        }

        private async void AzureSqlLayoutGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) return;

            if (vm.SelectedSubscriptions.Any(x => !x.HasEverGotSqlServers))
            {
                await vm.RefreshDatabases();
            }
        }

        private async void VMLayoutGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) return;

            if (vm.VmTabVm.VMList.Count == 0)
            {
                await vm.RefreshVms();
            }
        }

        private async void VNetLayoutGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) return;

            if (vm.VNetTabVm.VNetList.Count == 0)
            {
                await vm.RefreshVNets();
            }
        }

        private async void PurviewLayoutGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) return;

            if (vm.PurviewTabVm.PurviewList.Count == 0)
            {
                await vm.RefreshPurview();
            }
        }

        private async void StorageLayoutGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) return;

            if (vm.StorageTabVm.StorageList.Count == 0)
            {
                await vm.RefreshStorage();
            }
        }

        private void StatusSelectionText_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainTabControl.SelectedIndex = 0;
        }

        private void PurviewTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void VMTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }



    public class ignoresubscriptionnames
    {
        public List<string> FromPhone { get; set; }
        public string StartMessagePart { get; set; }
        public string EndMessagePart { get; set; }
    }
}
