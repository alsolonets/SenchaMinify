## About

This library allows you minify ExtJS and Sencha Touch files using ASP.NET MVC engine on-the-fly.

## Usage

####Global.asax
```c#
protected void Application_Start()
{
    // ...
    BundleConfig.RegisterBundles(BundleTable.Bundles);
}
```


####BundleConfig.cs
```c#
using SenchaMinify;

// ...
public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        bundles.Add(
            new SenchaBundle("~/bundles/my-sencha-app")
            .IncludeDirectory("~/Scripts/my-sencha-app", "*.js", true)
        );
    }
}

```


####Index.cshtml
```razor
<script src="@Url.Content("~/bundles/my-sencha-app")" type="text/javascript"></script>
```

## Installation
NuGet:
```
Install-Package SenchaMinify
```
