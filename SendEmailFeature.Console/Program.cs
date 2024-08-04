using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SendEmailFeature.Lib;

var root = GetProjectDirectory();

var listener = new TextWriterTraceListener(@$"{root}\logs.txt");
var sourceSwitch = new SourceSwitch("sourceSwitch", "Logging Sample")
{
    Level = SourceLevels.Information
};


var host = Host.CreateDefaultBuilder();
host.ConfigureAppConfiguration((hostingContext, configuration) =>
{
    configuration.Sources.Clear();
    configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
});

host.ConfigureServices((context, services) =>
{
    services.AddTransient<EmailSender>();
});

host.ConfigureLogging((loggerBuilder) => {
    loggerBuilder.AddTraceSource(sourceSwitch, listener);
});

var app = host.Build();

var emailSender = app.Services.GetService<EmailSender>() ?? throw new NullReferenceException("Cannot not find email service");

var email = string.Empty;

while (string.IsNullOrEmpty(email)) {
    Console.WriteLine("Enter Email:");
    email = Console.ReadLine();
}

await emailSender.TrySendAsync(email);
listener.Flush();

static string? GetProjectDirectory([CallerFilePath] string sourceFilePath = "") 
    => Path.GetDirectoryName(sourceFilePath);
