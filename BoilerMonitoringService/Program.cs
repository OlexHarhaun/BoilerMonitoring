using BoilerMonitoringService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddCustomLogging(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddCustomServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();