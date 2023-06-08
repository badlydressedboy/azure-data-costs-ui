using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEstateOverview.Models.SQL
{
    public abstract class BaseModel : ObservableObject
    {
        public string Name { get; set; }
        public string ConnString { get; set; }
        public List<string> ExceptionMessages { get; set; } = new List<string>();

        private string _ConnectivityError;
        public string ConnectivityError
        {
            get => _ConnectivityError;
            set
            {
                SetProperty(ref _ConnectivityError, value);
                if (string.IsNullOrEmpty(_ConnectivityError))
                {
                    HasConnectivityError = false;
                }
                else
                {
                    HasConnectivityError = true;
                }
            }
        }

        private bool _hasConnectivityError;
        public bool HasConnectivityError
        {
            get => _hasConnectivityError;
            set => SetProperty(ref _hasConnectivityError, value);
        }
        protected virtual async Task<DataResult> ProcessResult(DataResult result)
        {
            if (string.IsNullOrEmpty(result.ExceptionMessage))
            {
                ExceptionMessages.Add(result.ExceptionMessage);
            }
            return result;
        }
    }
}
