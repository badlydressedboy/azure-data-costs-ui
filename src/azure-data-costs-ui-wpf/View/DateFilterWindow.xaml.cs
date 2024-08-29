using Azure.Costs.Common.Models.Rest;
using Azure.Costs.Ui.Wpf.Vm;
using DataEstateOverview;
using DbMeta.Ui.Wpf.Config;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
    public partial class DateFilterWindow : MetroWindow
    {
        private MainWindowVm _vm;

        
        public DateFilterWindow(MainWindowVm vm  )
        {
            InitializeComponent();

            _vm = vm;
            DataContext = _vm;
            
            
        }     


        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Close();    
        }
    }
}
