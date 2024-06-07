using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalR_Demo_Application.App_Data;
using SignalR_Demo_Application.Hubs;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins("http://localhost:3000") // Replace with your client app's URL
            .AllowCredentials();
    });
});
builder.Services.AddScoped<SQLiteHelper>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
string currentDirectory = Environment.CurrentDirectory; // Set your actual file path
var lifetime = app.Services.GetService<IHostApplicationLifetime>(); // Get the service instance

if (lifetime == null)
{
    Console.WriteLine("Error: IHostApplicationLifetime service is not available.");
}
else
{
    // Register a callback to execute when the application is stopping
    lifetime.ApplicationStopping.Register(() =>
    {
        // Log the path to be deleted
        Console.WriteLine("Deleting SQLite file at path: " + currentDirectory + "\\App_Data\\MyDatabase.sqlite");

        try
        {
            // Delete the SQLite file
            File.Delete(currentDirectory + "\\App_Data\\MyDatabase.sqlite");
            Console.WriteLine("SQLite file deleted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting SQLite file: " + ex.Message);
        }
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/ChatHub");

app.Run();
