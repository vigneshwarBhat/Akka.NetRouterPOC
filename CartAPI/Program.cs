using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Infra;
using Akka.Cluster.Infra.Actor;
using Akka.Cluster.Routing;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Akka.Routing;
using TradePlacementAPI;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables();

builder.Logging.ClearProviders().AddConsole();
builder.WebHost
.UseKestrel()
.ConfigureServices((context, services) =>
{
    services.AddControllers();
    services.AddAkka("cart", (builder, provider) =>
    {
        
     
        builder 
            .AddHoconFile("app.conf", HoconAddMode.Prepend)         
            .WithRemoting(hostname: "localhost", port: 9228)
            // Add common DevOps settings
            .WithOps(
                remoteOptions: new RemoteOptions
                {
                    HostName = "0.0.0.0",
                    Port = 9228
                },
                clusterOptions: new ClusterOptions
                {
                    SeedNodes = new[] { "akka.tcp://cart@localhost:9228" },
                    Roles = new[] { "cartcreator" },
                },
                config: context.Configuration,
                readinessPort: 11002,
                pbmPort: 9111)
            // Instantiate actors
            .WithActors((system, registry) =>
            {              
                var cartItemRouter = system.ActorOf(Props.Create(() => new CartItemProcessor())
                                 .WithRouter(new ClusterRouterPool(new RoundRobinPool(100), new ClusterRouterPoolSettings(100, 10, false, "cartitemrouteehost"))), "cartItemPoolRouter");
                 var cartRouter = system.ActorOf(Props.Create(() => new CartProcessor(cartItemRouter))
                                  .WithRouter(new ClusterRouterPool(new RoundRobinPool(100), new ClusterRouterPoolSettings(100, 10, false, "cartrouteehost"))), "cartPoolRouter");
                var consoleActor = system.ActorOf(Props.Create(() => new BridgeActor(cartRouter)), "bridge");
                registry.Register<BridgeActor>(consoleActor);
            });
    });
});



var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
   
// }
// else
// {
//     app.UseExceptionHandler("/error");
// }

app.UseSwagger();
app.UseSwaggerUI();
app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

