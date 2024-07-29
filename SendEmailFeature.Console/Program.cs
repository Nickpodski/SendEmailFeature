using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendEmailFeature.Lib;

var host = Host.CreateApplicationBuilder();
host.Configuration.AddJsonFile("appsettings.Development.json", true, true);

host.Services.AddTransient<EmailSender>();

var app = host.Build();

var emailSender = app.Services.GetService<EmailSender>();

emailSender?.TrySendAsync("nickpodski@gmail.com");

