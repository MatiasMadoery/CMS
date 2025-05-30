using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using System;

public class FakeUrlHelper : IUrlHelper
{
    public ActionContext ActionContext { get; }

    public FakeUrlHelper()
    {
        // Se puede dejar vacío o inicializar según convenga.
        ActionContext = new ActionContext();
    }

   

public string Action(UrlActionContext actionContext)
{
    if (actionContext.Values == null)
        return "http://localhost/";

    var routeValues = new RouteValueDictionary(actionContext.Values);
    if (routeValues.ContainsKey("machineId"))
    {
        return $"http://localhost/QrCodes/Details?machineId={routeValues["machineId"]}";
    }
    return "http://localhost/QrCodes/Details";
}

public string Content(string contentPath)
    {
        return contentPath;
    }

    public bool IsLocalUrl(string url)
    {
        return true;
    }

    public string Link(string routeName, object values)
    {
        return $"http://localhost/{routeName}";
    }

    public string RouteUrl(UrlRouteContext routeContext)
    {
        return $"http://localhost/{routeContext.RouteName}";
    }
}
