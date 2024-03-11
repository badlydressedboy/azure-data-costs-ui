using CommunityToolkit.Mvvm.ComponentModel;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Ui.Wpf.Vm
{
    public class TabFilters : ObservableObject
    {
        public Dictionary<string, Filter> Filters { get; set; } = new Dictionary<string, Filter>(); 
    }

    public class Filter : ObservableObject
    {
        public List<SelectableString> Items { get; set; } = new List<SelectableString>();

        private string? summaryText;
        public string? SummaryText
        {
            get => summaryText;
            set => SetProperty(ref summaryText, value);
        }

        public Filter()
        {
            SummaryText = "";
        }

        public void UpdateFilterSummary()
        {            
            var x = Items.Where(x => x.IsSelected).Count();
            var y = Items.Count;
            if (x != y)
            {
                SummaryText = $"{x}/{y}";
            }
            else
            {
                SummaryText = "";
            }            
        }

        public void AddSelectableItem(string potentialString)
        {
            var existing = Items.FirstOrDefault(x => x.StringValue == potentialString);
            if (existing == null)
            {
                Items.Add(new SelectableString() { StringValue = potentialString, IsSelected = true });
            }
        }

    }
}
