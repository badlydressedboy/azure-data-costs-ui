using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Ui.Wpf.Vm
{
    public abstract class TabVmBase : ObservableObject
    {
        protected List<Filter> _filterList = new List<Filter>();

        public Filter TagsFilter { get; set; } = new Filter();
        public Filter SubscriptionFilter { get; set; } = new Filter();        
        public Filter ResourceGroupFilter { get; set; } = new Filter();


        private string restErrorMessage;
        public string RestErrorMessage
        {
            get => restErrorMessage;
            set
            {

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
        private string totalCostsText;
        public string TotalCostsText
        {
            get => totalCostsText;
            set => SetProperty(ref totalCostsText, value);
        }
        public void SetFilterSummaries()
        {
            foreach (var filter in _filterList)
            {
                filter.UpdateFilterSummary();
            }
        }
        public void ClearFilterItems()
        {
            foreach (var filter in _filterList)
            {
                filter.Items.Clear();
            }
            TagsFilter.Items.Add(new SelectableString() { StringValue = "", IsSelected = true }); // need option for NO tags

        }

        protected static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public TabVmBase()
        {
            _filterList.Add(TagsFilter);            
            _filterList.Add(ResourceGroupFilter);
            _filterList.Add(SubscriptionFilter);
        }
    }
}
