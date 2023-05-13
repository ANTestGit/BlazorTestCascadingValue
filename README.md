# BlazorTestCascadingValue
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