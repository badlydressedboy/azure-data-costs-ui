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
            if(e.OriginalSource is TextBlock)
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
            // testing api access            
            await vm.TestLogin();
            await vm.GetSubscriptions();
        }

        private async Task RefreshDBs()
        {            
            try
            {
                await vm.DBTabVm.RefreshDatabases(vm.SelectedSubscriptions); 
            }catch(Exception ex)
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
            if(e.Item == null) return;

            vm.DBTabVm.SetFilterSummaries();

            var db = e.Item as RestSqlDb;
           
            bool textFilterMatched = true;
            bool tagFilterMatched = true;
            bool soFilterMatched = true;

            // 1.filter on name text
            //if (RestDbFilterText.Text.Length > 0)
            //{
            //    textFilterMatched = false;
            //    if (!db.name.Contains(RestDbFilterText.Text))
            //    {
            //        textFilterMatched = true;
            //    }
            //}

            // 2.filter on tag
            var tags = vm.DBTabVm.TagsFilter.Items;
            if (tags.Count > 0) tagFilterMatched = false;

            if (db.TagsList.Count > 0)
            {
                foreach (var tag in db.TagsList)
                {
                    foreach (var allTag in tags.Where(x => x.IsSelected))
                    {
                        if (tag == allTag.StringValue)
                        {
                            tagFilterMatched = true;
                        }
                    }
                }
            }
            else {
                var existing = tags.FirstOrDefault(x => x.IsSelected && x.StringValue == "");
                if (existing != null)
                {
                    tagFilterMatched = true;
                }
            }

            // 3.filter on so
            var sos = vm.DBTabVm.SoFilter.Items;
            if (sos.Count > 0) soFilterMatched = false;
            foreach (var selectedSo in sos.Where(x => x.IsSelected))
            {
                if (db.properties.currentServiceObjectiveName.ToUpper() == selectedSo.StringValue.ToUpper())
                {
                    soFilterMatched = true;
                    break;
                }
            }


            //textFilterMatched && 
            if (tagFilterMatched && soFilterMatched) { 
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
                if(days<1 || days > 90)
                {
                    e.Handled = true;
                    return;
                }
                APIAccess.CostDays = days;
            }       
        }

        private async void TestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.TestLogin();
        }

        void IgnoreOnChecked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }    

        private void IgnoreCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource is CheckBox)
            {
                var clickedSub = (Subscription)((CheckBox)e.OriginalSource).DataContext;   
                var subName = clickedSub.displayName;
                var configSub = App.Config.Subscriptions.FirstOrDefault(x => x.Name == subName);
                if(configSub == null)
                {
                    configSub = new ConfigSubscription() { Name=subName};
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

        }

        private void PurviewCollapseCostsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PurviewDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {

        }

        private void RefreshDbStatsButton_Click(object sender, RoutedEventArgs e)
        {
            GetDbMetrics();
        }

        private async void GetDbMetrics(RestSqlDb db  = null)
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
            await vm.RefreshVMs();
        }

        private async void PurviewRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.RefreshPurview();
        }

        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (MainTabControl.SelectedIndex)
            {
                case(0):
                    // db summary
                    break;
                    case(1):
                    // db details
                    // have any subscriptions never had sql servers queried?
                    if (vm.SelectedSubscriptions.Any(x => !x.HasEverGotSqlServers))
                    {
                        // shouldnt need this BUT the db summary busyindicator wont fire on first activation
                        Cursor = Cursors.Wait;
                        await vm.DBTabVm.RefreshDatabases(vm.SelectedSubscriptions);
                        Cursor = Cursors.Arrow;
                    }   
                    break;

                    case(2):
                    // adf
                    if(vm.DataFactoryList.Count == 0)
                    {
                        await vm.RefreshDataFactories();
                    }
                    break;
;
                case (3):
                    // storage
                    if (vm.StorageList.Count == 0)
                    {
                        await vm.RefreshStorage();
                    }
                    break;
                case (4):
                    // vnets
                    if (vm.VNetList.Count == 0)
                    {
                        await vm.RefreshVNets();
                    }
                    break;
                case (5):
                    // vms
                    if (vm.VMList.Count == 0)
                    {
                        
                        await vm.RefreshVMs();
                    }
                    break;
                case (6):
                    // purview
                    if (vm.PurviewList.Count == 0)
                    {
                        await vm.RefreshPurview();
                    }
                    break;
            }
        }

        private async void DBAnalyseSpendButton_Click(object sender, RoutedEventArgs e)
        {
            await vm.DBTabVm.AnalyseDbSpend();
        }

        private async void VMDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {

            GetVmMetrics();
            
        }

        private void RefreshVmStatsButton_Click(object sender, RoutedEventArgs e)
        {
            GetVmMetrics();
        }
        private async void GetVmMetrics()
        {
            if (VMDataGrid.CurrentItem == null) return;
            var vm = (VM)VMDataGrid.CurrentItem;

            if (vm != null && (!vm.GotMetricsHistory))
            {
                await APIAccess.GetVmMetrics(vm);
            } // no minutes param passed so sqlDb.MetricsHistoryDays is used            
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
            await vm.AnalyseVmSpend();
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

        private void DbFilterButton_Click(object sender, RoutedEventArgs e)
        {
            Button but = (Button)sender;
            if(!(but.DataContext is List<SelectableString>))
            {
                _logger.Error("Filter button has not had data context set correctly to list");
                return;
            }
            var tagsWin = new FilterWindow((List<SelectableString>)but.DataContext);
           
            tagsWin.Owner = this;

            tagsWin.ShowDialog();                       

            // do actual filter: vm.AllDBTags

            CollectionViewSource.GetDefaultView(RestDbDataGrid.ItemsSource).Refresh();
        }
    }
    public class ignoresubscriptionnames
    {
        public List<string> FromPhone { get; set; }
        public string StartMessagePart { get; set; }
        public string EndMessagePart { get; set; }
    }
}
