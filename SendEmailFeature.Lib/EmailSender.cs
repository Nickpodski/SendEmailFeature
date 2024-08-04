using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SendEmailFeature.Lib;

public class EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
{
    private readonly IConfiguration _configuration = configuration;
    private Validation<Error, EmailConfiguration> _emailConfiguration 
        => _configuration.ToEmailConfiguration();
    private readonly ILogger<EmailSender> _logger = logger;

    private Validation<Error, MailAddress> GetSender() => _emailConfiguration.Select(config => 
        new MailAddress(config.SenderEmail, config.SenderName));

    private Validation<Error, MailMessage> GetMessage(string toAddress) => GetSender().Select(
        sender => new MailMessage(sender, new(toAddress))
        {
            Subject = "Testing Gmail SMTP in C#",
            Body = "Hello, this is a test email sent from a C# application using Gmail SMTP."
        }
    );

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var isValid = Try(() => 
        {

            var replacedEmail = Regex.Replace(
                email,
                @"(@)(.+)$",
                DomainMapper,
                RegexOptions.None,
                TimeSpan.FromMilliseconds(200)
            );

            static string DomainMapper(Match match)
            {
                var idn = new IdnMapping();
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }

            return Regex.IsMatch(
                replacedEmail,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250)
            );
        });

        return isValid.IfFail(false);
    }

    private Validation<Error, SmtpClient> GetSmtpClient() =>
        from emailConfiguration in _emailConfiguration
        select new SmtpClient
        {
            Host = emailConfiguration.Host,
            Port = emailConfiguration.Port,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                emailConfiguration.SenderEmail, emailConfiguration.SenderPassword
            ),
            EnableSsl = true,
        };

    private async Task<Validation<Error, Unit>> AttemptSend(
        SmtpClient client,
        MailMessage message,
        string logInfo,
        int tries
    )
    { 
        var attempt = await TryAsync(async () => 
        {
            await client.SendMailAsync(message);
            _logger.LogInformation($"Message sent successfully! {logInfo} \nAttempt: {tries}");
            return unit;
        });

        var returnValue = attempt
            .ToValidation(ex => 
            {
                _logger.LogInformation($"Message failed to send! {logInfo} \nAttempt: {tries}");
                return Error.New(ex.ToString());
            });

        return returnValue;
    }
    public async Task<Validation<Error, Unit>> TrySendAsync(string toAddress)
    {
        if (!IsValidEmail(toAddress)) return Error.New("Not Valid Email Address");

        var mailMessage = GetMessage(toAddress);
        var today = DateTime.Today.ToShortDateString();
        var logInfo =
            from config in _emailConfiguration
            from message in mailMessage
            select 
                $"\nRecipient: {toAddress}, "
                + $"\nSender: {config.SenderEmail}, "
                + $"\nSubject: {message.Subject}, "
                + $"\nBody: {message.Body}, "
                + $"\nDate: {today}, ";

        for (var tries = 1; tries <= 3; tries++)
        {
            var sendTask =
                from client in GetSmtpClient()
                from message in mailMessage
                from info in logInfo
                select AttemptSend(client, message, info, tries);
            var awaitedTask = await sendTask.Match(
                async (task) =>
                {
                    return await task;
                },
                err => 
                {
                    return Task.FromResult(Fail<Error, Unit>(err));
                }
            );

            if(awaitedTask.IsSuccess) break;
            if(tries == 3) return awaitedTask;
        }

        return unit;
    }
}
