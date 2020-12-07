using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Linq;

namespace Koasta.Service.Admin.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "active-when")]
    [HtmlTargetElement("a", Attributes = "active-when,active-when-param")]
    public class ATagHelper : TagHelper
    {
        public string ActiveWhen { get; set; }
        public string ActiveWhenParam { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ActiveWhen == null)
                return;

            var targetPage = ActiveWhen;

            var currentPage = ViewContextData.RouteData.Values["page"].ToString();
            var hasParams = ViewContextData.RouteData.Values.Keys.Count > 1;

            if (!currentPage.Equals(targetPage))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ActiveWhenParam) && hasParams)
            {
                return;
            }

            if (hasParams && !ActiveWhenParam.Split(',').All(param => ViewContextData.RouteData.Values.ContainsKey(param)))
            {
                return;
            }

            if (output.Attributes.ContainsName("class"))
            {
                output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} active");
            }
            else
            {
                output.Attributes.SetAttribute("class", "active");
            }
        }
    }
}
