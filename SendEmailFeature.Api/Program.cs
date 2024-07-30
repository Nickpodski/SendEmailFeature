using SendEmailFeature.Lib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<EmailSender>();
var app = builder.Build();

var emailSender = app.Services.GetService<EmailSender>() ?? throw new NullReferenceException("Cannot not find email service");
app.MapGet("/{email}", async (string email) => await emailSender.TrySendAsync(email));

app.Run();
