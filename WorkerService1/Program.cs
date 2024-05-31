using Serilog;
using WorkerService1;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/myapp.csv", rollingInterval: RollingInterval.Day)
    .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();