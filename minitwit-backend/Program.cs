using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;
using Minitwit.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using System;
using Prometheus;


var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
	var configuration = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
		.AddJsonFile(
			$"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
			optional: true)
		.Build();

builder.Host.UseSerilog((context, configuration)  =>
    configuration.Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Debug()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticConfiguration:Uri"]!))
    {
        AutoRegisterTemplate = true,
        //IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
        IndexFormat = $"{context.Configuration["ApplicationName"]}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"

    })
    .Enrich.WithProperty("Environment", context.HostingEnvironment.ApplicationName)
    .ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseExceptionHandler("/Error");
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseRouting();

// Use Prometheus middleware.
// Capture metrics about all received HTTP requests.
app.UseHttpMetrics();
app.UseMetricServer();

app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    // Enable the /metrics page to export Prometheus metrics.
    //http://localhost:9090/
    //Open http://localhost:9090/metrics to see the metrics.
    endpoints.MapMetrics();
});

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataContext>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    while (!dbContext.Database.CanConnect())
    {
        logger.LogInformation("Waiting for database...");
        Thread.Sleep(1000);
    }
    dbContext.Database.Migrate();
}


//app.UseHttpsRedirection();

app.UseCors("cors");

app.UseAuthorization();
app.MapControllers();

app.Run();
