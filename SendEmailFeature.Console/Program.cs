using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendEmailFeature.Lib;

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

var app = host.Build();

var emailSender = app.Services.GetService<EmailSender>() ?? throw new NullReferenceException("Cannot not find email service");

var email = string.Empty;

while (string.IsNullOrEmpty(email)) {
    Console.WriteLine("Enter Email:");
    email = Console.ReadLine();
}

await emailSender.TrySendAsync(email);
