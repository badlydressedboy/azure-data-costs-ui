using Azure.Costs.Common.Models.Rest;
using Azure.Costs.Ui.Wpf.Vm;
using DataEstateOverview;
using DbMeta.Ui.Wpf.Config;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Azure.Costs.Ui.Wpf
{
    /// <summary>
    /// Interaction logic for TagsFilter.xaml
    /// </summary>
    public partial class RecommendationsWindow : MetroWindow
    {
        private DBTabVm _vm;

        
        public RecommendationsWindow(DBTabVm vm, RestSqlDb filterDb = null)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
            
            // build List
            List<DbRecommendation> recsList = new List<DbRecommendation>();

            // convert all vm database recommendation into DbRecommendation

            if (filterDb != null)
            {
                AddDbRecs(recsList, filterDb);
            }
            else
            {
                foreach (var db in _vm.RestSqlDbList)
                {
                    AddDbRecs(recsList, db);
                }
            }
            DbRecsDataGrid.ItemsSource = recsList.OrderByDescending(x=>x.Score);
            
        }

        private static void AddDbRecs(List<DbRecommendation> recsList, RestSqlDb db)
        {
            foreach (var advisor in db.AdvisorsList)
            {
                foreach (var rec in advisor.properties.recommendedActions)
                {
                    DbRecommendation newRec = new DbRecommendation();
                    newRec.Db = $"{db.serverName}.{db.name}";
                    newRec.Method = rec.properties.implementationDetails.method;
                    newRec.Script = rec.properties.implementationDetails.script;
                    newRec.RecommendationReason = rec.properties.recommendationReason;
                    newRec.ValidSince = rec.properties.validSince;
                    newRec.Score = rec.properties.score;
                    newRec.SqlDb = db;

                    recsList.Add(newRec);
                }
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ViewPortalDbButton_Click(object sender, RoutedEventArgs e)
        {
            OpenPortalFromButton(sender);
        }

        private void OpenPortalFromButton(object sender)
        {
            DbRecommendation rec = (DbRecommendation)((Button)sender).DataContext;
            PortalResource pr = (PortalResource)rec.SqlDb;

            // + "/recommendations"
            // dotnet core
            string url = pr.PortalResourceUrl.Replace("overview", "recommendations");
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        private void RowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ?
                Visibility.Visible : Visibility.Collapsed;
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

        private void DbRecsDataGrid_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e)
        {
            Debug.WriteLine("DbRecsDataGrid_RowDetailsVisibilityChanged");

            Grid gr = (Grid)e.DetailsElement;
            gr.Focus();
        }

        private void ViewPortalDbButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("ViewPortalDbButton_PreviewMouseLeftButtonDown");
           
            e.Handled = true;

            OpenPortalFromButton(sender);
        }
    }

    public class DbRecommendation
    {
        public string Db {  get; set; }
        public string Method { get; set; }
        public string Script { get; set; }

        public string RecommendationReason { get; set; }
        public DateTime ValidSince { get; set; }
        public int Score { get; set; }

        public RestSqlDb SqlDb { get; set; }

        public DbRecommendation() { }
    }
}
