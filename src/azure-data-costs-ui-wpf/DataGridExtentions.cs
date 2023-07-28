using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace DataEstateOverview
{
    public static class DataGridExtensions
    {
        public static readonly DependencyProperty SortDescProperty = DependencyProperty.RegisterAttached(
            "SortDesc", typeof(bool), typeof(DataGridExtensions), new PropertyMetadata(false, OnSortDescChanged));

        private static void OnSortDescChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if (grid != null)
            {
                grid.Sorting += (source, args) => {
                    if (args.Column.SortDirection == null)
                    {
                        // here we check an attached property value of target column
                        var sortDesc = (bool)args.Column.GetValue(DataGridExtensions.SortDescProperty);
                        if (sortDesc)
                        {
                            args.Column.SortDirection = ListSortDirection.Ascending;
                        }
                    }
                };
            }
        }

        public static void SetSortDesc(DependencyObject element, bool value)
        {
            element.SetValue(SortDescProperty, value);
        }

        public static bool GetSortDesc(DependencyObject element)
        {
            return (bool)element.GetValue(SortDescProperty);
        }
    }
}
