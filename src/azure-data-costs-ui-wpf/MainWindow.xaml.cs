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
using DataEstateOverview.Models.SQL;
using DataEstateOverview.Models.Rest;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using DbMeta.Ui.Wpf.Models.Rest;
using DbMeta.Ui.Wpf.Config;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using Path = System.IO.Path;
using Microsoft.Identity.Client;

namespace DataEstateOverview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        List<string> connStrings = new List<string>();
        DataContextVM vm = new DataContextVM();
        
        public MainWindow()
        {
            InitializeComponent();

            connStrings.Add("");

            DataContext = vm;
            CostDaysText.Text = APIAccess.CostDays.ToString();  
        }

        private async void VNetCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshData();
        }

        private async Task RefreshData()
        {
            if (ServerFirewallGrid == null) return;

            Cursor = Cursors.Wait;

            ServerFirewallGrid.ItemsSource = null;
            ServerEventLogGrid.ItemsSource = null;
            DbLoginsGrid.ItemsSource = null;
            DbSyncStateGrid.ItemsSource = null;

            try
            {
                await SelectAzDB();                
            } catch (Exception ex) {
                Debug.WriteLine(ex);
            }
            Cursor = Cursors.Arrow;
        }

        private async void SQLDBRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshData();
        }

        private async void SummaryDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectAzDB();
        }

        private async void SummaryDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            //await RefreshData();
        }

        private async Task SelectAzDB()
        {
            var selectedRestDb = (RestSqlDb)RestDbDataGrid.SelectedItem;
            TabSql.IsSelected = true;

            vm.SelectedAzDB = selectedRestDb.AzDB;

            ServerFirewallGrid.ItemsSource = null;
            ServerEventLogGrid.ItemsSource = null;
            DbEventLogGrid.ItemsSource = null;

            Cursor = Cursors.Wait;
            try
            {
                DbLoginsGrid.ItemsSource = null;
                DbSyncStateGrid.ItemsSource = null;
                DbRequestsGrid.ItemsSource = null;

                await vm.RefreshSqlDb();             

                ServerEventLogGrid.ItemsSource = vm.SelectedAzServer.EventLog.Where(x => x.Database == "master");
                ServerFirewallGrid.ItemsSource = vm.SelectedAzServer.FireWallRules;
                DbLoginsGrid.ItemsSource = vm.SelectedAzDB.DBPrincipals;
                DbRequestsGrid.ItemsSource = vm.SelectedAzDB.Sessions;
                DbEventLogGrid.ItemsSource = vm.SelectedAzServer.EventLog.Where(x=>x.Database == vm.SelectedAzDB.DatabaseName);

                if (vm.SelectedAzDB.SyncStates.FirstOrDefault(x => x.PartnerDatabase != null) != null)
                {
                    DbSyncStateGrid.ItemsSource = vm.SelectedAzDB.SyncStates;
                }
            }
            catch (Exception ex) { }
            Cursor = Cursors.Arrow;
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
            if (dg?.CurrentColumn?.DisplayIndex == 0)
            {
                await SelectAzDB();
            }
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // testing api access            
            await vm.TestLogin();
            await RefreshRest();
        }

        private async Task RefreshRest()
        {
            
            Cursor = Cursors.Wait;

            try
            {
                await vm.GetSubscriptions();
                await vm.RefreshRest(); 
            }catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            Cursor = Cursors.Arrow;
        }

        private async void RestRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshRest();
        }

        private void RestConfigButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RestGridMenuItemSqlDetails_Click(object sender, RoutedEventArgs e)
        {
            SelectAzDB();            
        }

        private void SQLDBBackButton_Click(object sender, RoutedEventArgs e)
        {
            TabRest.IsSelected = true;
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {

        }

        private void RestDbDataGridViewSource_Filter(object sender, FilterEventArgs e)
        {
            var db = e.Item as RestSqlDb;
            if (db != null)            
            {
                if (RestDbFilterText.Text.Length > 0)
                {
                    if (!db.name.Contains(RestDbFilterText.Text))
                    {
                        e.Accepted = false;
                        return;
                    }                    
                } 
                e.Accepted = true;
            }
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
            await vm.RefreshRest();
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
            }
            Debug.WriteLine("gdg");
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

        private void DataGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void DataGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private async void RestDbDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            if (RestDbDataGrid.CurrentItem == null) return;
            
            var db = (RestSqlDb)RestDbDataGrid.CurrentItem;
            if (!db.GotMetricsHistory)
            {
                await APIAccess.GetDbMetrics(db, 10080);// 10080: week
            } 
            
        }

        //private void IgnoreCheckBox_UnChecked(object sender, RoutedEventArgs e)
        //{

        //}


    }
    public class ignoresubscriptionnames
    {
        public List<string> FromPhone { get; set; }
        public string StartMessagePart { get; set; }
        public string EndMessagePart { get; set; }
    }
}
