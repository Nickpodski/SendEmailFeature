using LanguageExt.Common;
using Microsoft.Extensions.Configuration;

namespace SendEmailFeature.Lib;

public record EmailConfiguration(
    string SenderEmail,
    string Host,
    int Port,
    string SenderPassword,
    string SenderName
);

public static class IConfigurationExtensions
{
    public static Validation<Error, EmailConfiguration> ToEmailConfiguration(
        this IConfiguration configuration
    )
    {
        var senderEmail = configuration.ToValidation(
            "EmailSettings:senderEmail",
            "Missing Sender Email in Configuration"
        );
        var host = configuration.ToValidation(
            "EmailSettings:host",
            "Missing Sender Host in Configuration"
        );
        var port = configuration.ToValidation(
            "EmailSettings:port",
            "Missing Sender Port in Configuration"
        ).Bind(port => parseInt(port).ToValidation(Error.New("Cannot parse port into int")));
        var senderPassword = configuration.ToValidation(
            "EmailSettings:senderPassword",
            "Missing Sender Password in Configuration"
        );
        var senderName = configuration.ToValidation(
            "EmailSettings:senderName",
            "Missing Sender Name in Configuration"
        );

        var emailConfiguration = (senderEmail, host, port, senderPassword, senderName).Apply(
            (senderEmail, host, port, senderPassword, senderName) => new EmailConfiguration(
                SenderEmail: senderEmail,
                Host: host,
                Port: port,
                SenderPassword: senderPassword,
                SenderName: senderName
            )
        );
        return emailConfiguration;
    }

    private static Validation<Error, string> ToValidation(
        this IConfiguration configuration,
        string key,
        string errorMessage
    ) => Optional(configuration[key])
        .Bind(val => val == "" ? None : Some(val))
        .ToValidation(Error.New(errorMessage));

}