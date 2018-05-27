using System;
using System.Threading;
using EmailService.Contracts;
using Microsoft.Extensions.Logging;
using MockEmailClient;

namespace EmailService.Service
{
    public class EmailingService : IEmailService
    {
        private readonly ILogger<EmailingService> _logger;
        private readonly IEmailClient _emailClient;
        private Email email;

        public EmailingService(IEmailClient emailClient, ILogger<EmailingService> logger)
        {
            _emailClient = emailClient;
            _logger = logger;
        }

        public string SendEmail(Email email)
        {
            this.email = email;
            var status = "";
            _logger.LogInformation($"Sending email to {email.To}");
            try
            {
                status = SendEmailHelper();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error sending email to {email.To}");
                status = "Failure.";
            }
            finally
            {
                try
                {
                    Close();

                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error closing email to {email.To}");
                    status = "Failure.";
                }
            }
            return status;
        }
        public string Close()
        {
            try
            {
                _emailClient.Close();
                return "Success!";
            }
        }

        private String SendEmailHelper()
        {
            _logger.LogInformation($"Sending email to {email.To}");
            try
            {
                _emailClient.SendEmail(email.To, email.Body);
                return "Success!";
            }
        }
    }
}


