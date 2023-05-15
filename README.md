# Be afraid of CascadingValue

## Introduction

Test for cascading value refresh

This simple code produced cascading value refresh twice!!!! NET 6.0 but it is the for NET7.0
```CS
<button @onclick="OnRefresh">Refresh</button><br/>

<CascadingValue Value="@_forecasts" IsFixed="false">
    <TestComponent></TestComponent>
</CascadingValue>
@code {
    private WeatherForecast[]? _forecasts;

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("---Main component loading shared data---");
        _forecasts = await ForecastService.GetForecastAsync(DateTime.Now);
    }

    private async Task OnRefresh()
    {
        Console.WriteLine("---Pressed button Refresh---");
        await Task.Delay(200);
    }
}
```
TestComponet (code behind only)
``` CS
@code {
    [CascadingParameter]
    private WeatherForecast[]? Forecasts { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("OnInitializedAsync");
        await base.OnInitializedAsync();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var objects = parameters.ToDictionary();
        string parDescription = String.Join(", ",objects);
        Console.WriteLine($"set:{parDescription}");
        await base.SetParametersAsync(parameters);
    }

    protected override async Task OnParametersSetAsync()
    {
        Console.WriteLine("OnParametersSetAsync enter");
        await base.OnParametersSetAsync();
        // simulate some work
        await Task.Delay(250);
        Console.WriteLine("OnParametersSetAsync exit");
    }
}
```
---Pressed button Refresh---  
set:[Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]  
OnParametersSetAsync enter  
set:[Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]  
OnParametersSetAsync enter  
OnParametersSetAsync exit  
OnParametersSetAsync exit  

## Try to resolve
Firstly I thinking this is a [bug](https://github.com/dotnet/aspnetcore/issues/48223)
but in reality this behavior is by design.
(SetParametersAsync)[https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.componentbase.setparametersasync?view=aspnetcore-7.0]
will be called independent of parameters changing.


# Notes
From ticket discussion there are some additional information.

We need to pay attention to [the component lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-7.0)

So I added new logs about components rendering.
I created new branch NonUpdatableCasdingValue where you can the difference with 'non updated' CascadingParameters

```
***App init***
***Main Layout init***
---Main component loading shared data---
set:[Test1, ], [Forecasts, ]
OnInitializedAsync
OnParametersSetAsync enter
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync child:enter
OnAfterRenderAsync Owner:First render
OnAfterRenderAsync child:First render
OnParametersSetAsync exit
ShouldRender child
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
ShouldRender Owner
set:[Test1, Test], [Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]
OnParametersSetAsync enter
ShouldRender child
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
OnAfterRenderAsync Owner: next render
OnParametersSetAsync exit
ShouldRender child
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
---Pressed button Refresh---
ShouldRender Owner
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync Owner: next render
ShouldRender Owner
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync Owner: next render
```
Here is new logs from master branch

```
***App init***
***Main Layout init***
---Main component loading shared data---
set:[Forecasts, ]
OnInitializedAsync
OnParametersSetAsync enter
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync child:enter
OnAfterRenderAsync child:First render
OnAfterRenderAsync Owner:First render
OnParametersSetAsync exit
ShouldRender child
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
ShouldRender Owner
set:[Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]
OnParametersSetAsync enter
ShouldRender child
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
OnAfterRenderAsync Owner: next render
OnParametersSetAsync exit
ShouldRender child
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
---Pressed button Refresh---
ShouldRender Owner
set:[Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]
OnParametersSetAsync enter
ShouldRender child
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync child:enter
OnAfterRenderAsync Owner: next render
OnAfterRenderAsync child: next render
ShouldRender Owner
set:[Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]
OnParametersSetAsync enter
ShouldRender child
OnAfterRenderAsync Owner: enter
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
OnAfterRenderAsync Owner: next render
OnParametersSetAsync exit
ShouldRender child
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
OnParametersSetAsync exit
ShouldRender child
OnAfterRenderAsync child:enter
OnAfterRenderAsync child: next render
```