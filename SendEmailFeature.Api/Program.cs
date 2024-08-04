using System.Diagnostics;
using System.Runtime.CompilerServices;
using SendEmailFeature.Lib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<EmailSender>();
var root = GetProjectDirectory();

var listener = new TextWriterTraceListener(@$"{root}\logs.txt");
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
    var result = await emailSender.TrySendAsync(email);
    listener.Flush();
    return result;
});

app.Run();

static string? GetProjectDirectory([CallerFilePath] string sourceFilePath = "") 
    => Path.GetDirectoryName(sourceFilePath);