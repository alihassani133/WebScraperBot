using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadProductInfo
{
    public class ProductInfo
    {
        public int Number { get; set; }
        public string PartNumber { get; set; } = "";
        public string PartNumberHtml { get; set; } = "";
        public string DescriptionHtml { get; set; } = "";
        public string WebpageLink { get; set; } = "";
        public string PriceHtml { get; set; } = "";
        public string ImagePath { get; set; } = "";
        public string SpecificationsHtml { get; set; } = "";
        public string KitContentHtml { get; set; } = "";
    }


}
