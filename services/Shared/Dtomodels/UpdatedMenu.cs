using System.Collections.Generic;

namespace Koasta.Shared.Models
{
    public class UpdatedMenu
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string MenuDescription { get; set; }

        public string MenuImage { get; set; }

        public List<int> Products { get; set; }
    }
}
