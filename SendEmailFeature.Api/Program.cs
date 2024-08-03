using System.Diagnostics;
using SendEmailFeature.Lib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<EmailSender>();
var listener = new TextWriterTraceListener(@"C:\Users\Nickp\git\SendEmailFeature\logs.txt");
var sourceSwitch = new SourceSwitch("sourceSwitch", "Logging Sample")
{
    Level = SourceLevels.Information
};

builder.Logging.AddTraceSource(sourceSwitch, listener);
var app = builder.Build();

var emailSender =
    app.Services.GetService<EmailSender>()
    ?? throw new NullReferenceException("Cannot not find email service");
app.MapGet("/{email}", async (string email) => {
    await emailSender.TrySendAsync(email);
    listener.Flush();
});

app.Run();
