using System.Collections.Generic;

namespace Koasta.Service.Admin.Models
{
    public class BarChartData
    {
        public List<string> Labels { get; set; }
        public List<List<double>> Series { get; set; }
    }
}
