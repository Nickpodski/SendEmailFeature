using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace SendEmailFeature.Lib;

public class EmailSender(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    private string _senderEmail =>
        _configuration["senderEmail"]
        ?? throw new NullReferenceException("Missing Sender Email in Configuration");

    private string _host =>
        _configuration["host"] ?? throw new NullReferenceException("Missing Host in Configuration");

    private int _port =>
        int.Parse(
            _configuration["port"]
                ?? throw new NullReferenceException("Missing Port in Configuration")
        );

    private string _senderPassword =>
        _configuration["senderPassword"]
        ?? throw new NullReferenceException("Missing Sender Password in Configuration");

    private MailAddress GetSender() => new(_senderEmail, _configuration["senderName"]);

    private MailMessage GetMessage(string toAddress) =>
        new(GetSender(), new(toAddress))
        {
            Subject = "Testing Gmail SMTP in C#",
            Body = "Hello, this is a test email sent from a C# application using Gmail SMTP."
        };

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            email = Regex.Replace(
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
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250)
            );
        }
        catch (Exception)
        {
            return false;
        }
    }

    private SmtpClient GetSmtpClient() =>
        new()
        {
            Host = _host,
            Port = _port,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_senderEmail, _senderPassword),
            EnableSsl = true,
        };

    public async Task TrySendAsync(string toAddress)
    {
        if (!IsValidEmail(toAddress))
        {
            throw new NullReferenceException("Not Valid Email Address");
        }

        for (var tries = 1; tries <= 3; tries++)
        {
            try
            {
                await GetSmtpClient().SendMailAsync(GetMessage(toAddress));
                Console.WriteLine($"Message sent successfully to: {toAddress}.");
                break;
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"Message failed to send attempt: {tries}");
                if (tries == 3)
                    throw new(ex.Message);
            }
        }
    }
}
