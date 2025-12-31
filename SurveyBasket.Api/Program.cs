using Hangfire;
using HangfireBasicAuthenticationFilter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using SurveyBasket.Api;
using SurveyBasket.Api.Services.NotificationService;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDependencies(builder.Configuration);

//  If I Want to Use  DistributedMemoryCache
//builder.Services.AddDistributedMemoryCache();

// Add serilog  Of Logging
builder.Host.UseSerilog((context, configuration) =>
         configuration.ReadFrom.Configuration(context.Configuration)
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    }
    );
    app.UseHangfireDashboard();

}
// To Get HTTP Request In Logging
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

// url of background jobs  (https://localhost:7270/jobs)
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization =

    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = app.Configuration.GetValue<string>("HangfireSettings:Username"),
            Pass = app.Configuration.GetValue<string>("HangfireSettings:Password")
        }
        ],
    DashboardTitle = "SurveyBasket Background Jobs",
    // if i want who use Dashboard can not to Delete Or manage Jobs
    //IsReadOnlyFunc = (DashboardContext context) => true
});
// Schedule Recurring Jobs
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope = scopeFactory.CreateScope();
var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
// Schedule a daily job to send notifications about new polls
RecurringJob.AddOrUpdate(
    "SendNewPollsNotification",
    () => notificationService.SendNewPollsNotification(null),
    Cron.Daily);

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

app.UseRateLimiter();

// route of health check (https://localhost:7270/health)
app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.Run();
