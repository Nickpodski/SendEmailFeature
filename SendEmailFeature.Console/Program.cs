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

await emailSender.TrySendAsync("nickpodski@gmail.com");
