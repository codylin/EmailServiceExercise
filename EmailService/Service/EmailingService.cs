﻿using System;
using System.Threading;
using EmailService.Contracts;
using Microsoft.Extensions.Logging;
using MockEmailClient;
using Polly;

namespace EmailService.Service
{
    public class EmailingService : IEmailService
    {
        private readonly ILogger<EmailingService> _logger;
        private readonly IEmailClient _emailClient;

        public EmailingService(IEmailClient emailClient, ILogger<EmailingService> logger)
        {
            _emailClient = emailClient;
            _logger = logger;
        }

        public string SendEmail(Email email)
        {
            var status = "";
            _logger.LogInformation($"Sending email to {email.To}");
            try
            {
                status = SendEmailHelper(email);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error sending email to {email.To}");
                status = "Failure.";
                // quick and dirty retry. numbers are hardcoded for now
                _logger.LogError(e, $"Begin Retry");
                var policyResult = Policy
                    .Handle<Exception>(ex => ex.Message == "Connection Failed")
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(5))
                    .ExecuteAndCapture(() => { SendEmailHelper(email); });
                status = (policyResult.FinalException == null) ? "Success!" : "Failure.";
                
            }
            finally
            {
                try
                {
                    _logger.LogInformation($"closing email to {email.To}");
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
        //Separate Close() Method for unit testing
        public string Close()
        {
                _emailClient.Close();
                return "Success!";
        }
        //Separate SendEmailHelper() Method for unit testing
        public String SendEmailHelper(Email email)
        {
                _emailClient.SendEmail(email.To, email.Body);
                return "Success!";
        }
    }
}


