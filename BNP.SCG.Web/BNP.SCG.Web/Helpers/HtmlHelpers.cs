using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BNP.SCG.Web
{
    public static class HtmlHelpers
    {
        public static string IsSelected(this IHtmlHelper html, string controller = null, string action = null, string cssClass = null, string route = null)
        {
            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];
            string currentID = (string)html.ViewContext.RouteData.Values["id"];

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            if (String.IsNullOrEmpty(route))
                route = currentID;

            return controller == currentController && action == currentAction && (!string.IsNullOrEmpty(route) ? route == currentID : true) ?
                cssClass : String.Empty;
        }

        public static string PageClass(this IHtmlHelper htmlHelper)
        {
            string currentAction = (string)htmlHelper.ViewContext.RouteData.Values["action"];
            return currentAction;
        }
    }
}
