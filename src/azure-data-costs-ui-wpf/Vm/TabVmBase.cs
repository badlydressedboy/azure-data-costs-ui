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
        }
    }
}
