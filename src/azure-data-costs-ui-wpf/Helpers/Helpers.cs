using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Costs.Ui.Wpf
{
    public static class Helpers
    {
        public static string GetFilterSummary(List<SelectableString> stringList)
        {
            string returnString = "";
            var x = stringList.Where(x => x.IsSelected).Count();
            var y = stringList.Count;
            if (x != y)
            {
                returnString = $"{x}/{y}";
            }
            return returnString;
        }

        public static void AddSelectableString(List<SelectableString> stringList, string potentialString)
        {
            var existing = stringList.FirstOrDefault(x => x.StringValue == potentialString);
            if (existing == null)
            {
                stringList.Add(new SelectableString() { StringValue = potentialString, IsSelected = true });
            }
        }
    }
}
