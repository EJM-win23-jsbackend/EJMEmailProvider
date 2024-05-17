using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using EJMEmailProvider.Models;
using EJMEmailProvider.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EJMEmailProvider.Functions
{
    public class EmailSender
    {
        private readonly ILogger<EmailServices> _logger;
        private readonly EmailServices _emailServices;

        public EmailSender(EmailServices emailServices, ILogger<EmailServices> logger)
        {
            _emailServices = emailServices;
            _logger = logger;
        }

        [Function(nameof(EmailSender))]
        public async Task Run(
            [ServiceBusTrigger("email_request", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            try
            {
                var emailRequest = _emailServices.UnpackEmailRequest(message);
                if (emailRequest != null && !string.IsNullOrEmpty(emailRequest.To))
                {
                    await messageActions.CompleteMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("EmailSender : Run ::" + ex.Message);
            }

        }
    }
}
