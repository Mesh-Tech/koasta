using System.Collections.Generic;

namespace Koasta.Service.Admin.Models
{
    public class BarChartDesciptiveElem
    {
        public string Meta { get; set; }
        public double Value { get; set; }
    }

    public class BarChartDescriptiveData
    {
        public List<string> Labels { get; set; }
        public List<List<BarChartDesciptiveElem>> Series { get; set; }
    }
}
