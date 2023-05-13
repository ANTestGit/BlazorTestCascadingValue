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

---Pressed button Refresh---  
set:[Test1, Test], [Test2, Test2], [Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]  
OnParametersSetAsync enter  
OnParametersSetAsync exit  
set:[Test1, Test], [Test2, Test2], [Forecasts, BlazorTestCascadingValue.Data.WeatherForecast[]]  
OnParametersSetAsync enter  
OnParametersSetAsync exit