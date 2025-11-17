using Serilog;
using SurveyBasket.Api;


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
    app.UseSwagger();
    app.UseSwaggerUI();
}
// To Get HTTP Request In Logging
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

app.Run();
