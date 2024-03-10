using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Ui.Wpf.Vm
{
    public class FilterWindowVm : ObservableObject
    {
        private bool _allChecksSelected;

        public List<SelectableString> TagList { get; set; }

        public bool AllChecksSelected
        {
            get { return _allChecksSelected; }

            set
            {
                if (_allChecksSelected == value) return;

                _allChecksSelected = value;                
                SetProperty(ref _allChecksSelected, value);
                
                UpdateSelectedBasedOnAllCheck();
            }
        }

        public FilterWindowVm(List<SelectableString> tagList)
        {
            TagList = tagList;
            TestAllChecksSelected();
        }

        public void TestAllChecksSelected()
        {
            // todo - fix this as it creates a loop and goes screwey

            //if (TagList.All(x => x.IsSelected))
            //{
            //    AllChecksSelected = true;
            //    return;
            //}
            //AllChecksSelected = false;
        }

        public void UpdateSelectedBasedOnAllCheck()
        {
            if (AllChecksSelected)
            {
                foreach (var tag in TagList)
                {
                    tag.IsSelected = true;
                }
            }
            else
            {
                foreach (var tag in TagList)
                {
                    tag.IsSelected = false;
                }
            }
        }
    }
}
