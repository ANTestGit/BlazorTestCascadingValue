# Be afraid of CascadingValue

## Introduction

Test for cascading value refresh

This simple code produced cascading value refresh twice!!!! NET 6.0 but it is the for NET7.0 too
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
    
> **Note**. This is logs without rendering cycle

## Try to resolve
Firstly I thinking this is a [bug](https://github.com/dotnet/aspnetcore/issues/48223)
but in reality this behavior is by design.
(SetParametersAsync)[https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.componentbase.setparametersasync?view=aspnetcore-7.0]
will be called independent of parameters changing.

## What is happens?

For better understanding, try to swith into 3rd branch where CascadingValue has `IsFixed=true`
Then you will see the next output:

    ---Pressed button Refresh---
    ShouldRender Owner
    OnAfterRenderAsync Owner: enter
    OnAfterRenderAsync Owner: next render
    ShouldRender Owner
    OnAfterRenderAsync Owner: enter
    OnAfterRenderAsync Owner: next render
    
Now look into [component lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-7.0) diagramm.
First, we call async function OnRefresh() and as task is not complete rendring is calling.
Second, we have next non complete task `await Task.Delay(200);` - rendering called again.

If we enable CascadingValue changes with `IsFixed=false` then in addition to rendering SetParametersAsync will be called as parameter changing is not checked. After it we could expect call of `OnParametersSetAsync`

> **Note**. Id child component will have additional standart parameters, then behavior will be different.

## Conclusion
Never mix **CascadingParameter** with any additional work into `OnParametersSetAsync` function.
This work could be started in undesired time.

## Solutions
There are some possible solutions if you want to use CascadingValue:
- use `IsFixed=true`
- check if CascadingParameter changed, itself into SetParametersAsync (before calling base method!)
- use service instead of cascading value. See branch LoaderService

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