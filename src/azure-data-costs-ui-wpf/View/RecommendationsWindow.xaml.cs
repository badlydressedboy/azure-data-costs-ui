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

        
        public RecommendationsWindow(DBTabVm vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
            
            // build List
            List<DbRecommendation> recsList = new List<DbRecommendation>();

            // convert all vm database recommendation into DbRecommendation
            foreach(var db in _vm.RestSqlDbList)
            {
                foreach (var advisor in db.AdvisorsList) {
                    //var rec = advisor.

                    foreach(var rec in advisor.properties.recommendedActions)
                    {
                        DbRecommendation newRec = new DbRecommendation();
                        newRec.Db = $"{db.serverName}.{db.name}";
                        newRec.Method = rec.properties.implementationDetails.method;
                        newRec.Script = rec.properties.implementationDetails.script;
                        newRec.RecommendationReason = rec.properties.recommendationReason;
                        newRec.ValidSince = rec.properties.validSince;
                        newRec.SqlDb = db;

                        recsList.Add(newRec);
                    }
                }
            }

            Debug.WriteLine("xx");
            DbRecsDataGrid.ItemsSource = recsList;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ViewPortalDbButton_Click(object sender, RoutedEventArgs e)
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
    }

    public class DbRecommendation
    {
        public string Db {  get; set; }
        public string Method { get; set; }
        public string Script { get; set; }

        public string RecommendationReason { get; set; }
        public DateTime ValidSince { get; set; }

        public RestSqlDb SqlDb { get; set; }

        public DbRecommendation() { }
    }
}
