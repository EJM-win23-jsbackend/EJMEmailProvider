using System;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using EJMEmailProvider.Models;
using EJMEmailProvider.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EJMEmailProvider.Functions
{
    public class UnSubscribeEmailSender
    {
        private readonly ILogger<UnSubscribeEmailSender> _logger;
        private readonly EmailServices _emailServices;
        private readonly IConfiguration _configuration;

        public UnSubscribeEmailSender(ILogger<UnSubscribeEmailSender> logger, EmailServices emailServices, IConfiguration configuration)
        {
            _logger = logger;
            _emailServices = emailServices;
            _configuration = configuration;
        }

        [Function(nameof(UnSubscribeEmailSender))]
        public async Task Run(
            [ServiceBusTrigger("email_unsubscribe", Connection = "ServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            try
            {
                var emailRequest = await _emailServices.UnpackUnsubscriberAsync(message);
                if (emailRequest != null && !string.IsNullOrEmpty(emailRequest))
                {
                    await _emailServices.SendUnsubscribeEmailAsync(emailRequest);
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
