using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Akka.Cluster.Infra;


class Program
{
    static async Task Main(string[] args)
    {

        var host = new HostBuilder()
            .ConfigureHostConfiguration(builder =>
            {
                builder.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging();
                services.AddAkka("cart", (builder, provider) =>
                {
                    builder
                        .AddHoconFile("app.conf", HoconAddMode.Prepend) 
                        .AddHocon(hocon: "akka.remote.dot-netty.tcp.maximum-frame-size = 256000b", addMode: HoconAddMode.Prepend)
                        // Add common DevOps settings
                        .WithOps(
                            remoteOptions: new RemoteOptions
                            {
                                HostName = "0.0.0.0",
                                Port = 9331
                            },
                            clusterOptions: new ClusterOptions
                            {
                                SeedNodes = new[] { "akka.tcp://cart@localhost:9228" },
                                Roles = new[] { "cartrouteehost" }
                            },
                            config: hostContext.Configuration,
                            readinessPort: 11004,
                            pbmPort: 9113);
                });
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
                configLogging.AddConsole();

            })
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
       
    }
}