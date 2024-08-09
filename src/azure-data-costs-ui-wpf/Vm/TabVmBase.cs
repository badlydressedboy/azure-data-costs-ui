using Azure.Costs.Common.Models.Rest;
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
        public Filter LocationFilter { get; set; } = new Filter();


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
                lock (filter.Items)
                {
                    filter.Items.Clear();
                }
            }
            lock (TagsFilter)
            {
                lock (TagsFilter)
                {
                    TagsFilter.Items.Add(new SelectableString() { StringValue = "", IsSelected = true }); // need option for NO tags
                }
            }

        }

        protected static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public TabVmBase()
        {
            _filterList.Add(TagsFilter);            
            _filterList.Add(ResourceGroupFilter);
            _filterList.Add(SubscriptionFilter);
            _filterList.Add(LocationFilter);
        }

        public bool IsTagFilterMatched(List<string> objectTags)
        {
            bool matched = true;
            lock (TagsFilter)
            {
                if (TagsFilter.Items.Count > 0) matched = false;

                if (objectTags.Count > 0)
                {
                    foreach (var tag in objectTags)
                    {
                        foreach (var allTag in TagsFilter.Items.Where(x => x.IsSelected))
                        {
                            if (tag == allTag.StringValue)
                            {
                                matched = true;
                            }
                        }
                    }
                }
                else
                {
                    var existing = TagsFilter.Items.FirstOrDefault(x => x.IsSelected && x.StringValue == "");
                    if (existing != null)
                    {
                        matched = true;
                    }
                }
            }
            return matched; 
        }
    }
}
