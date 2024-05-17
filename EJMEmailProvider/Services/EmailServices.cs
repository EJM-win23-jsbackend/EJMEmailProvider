using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Azure;
using EJMEmailProvider.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace EJMEmailProvider.Services
{
    public class EmailServices
    {
        private readonly ILogger<EmailServices> _logger;
        private readonly EmailClient _emailClient;

        public EmailServices(ILogger<EmailServices> logger, EmailClient emailClient)
        {
            _logger = logger;
            _emailClient = emailClient;
        }

        //ServiceBusReceivedMessage häntar data från en queue. 
        public EmailRequest UnpackEmailRequest(ServiceBusReceivedMessage message)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<EmailRequest>(message.Body.ToString());

                if (request != null)
                {
                    return request;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UnpackEmailRequest : Run ::" + ex.Message);

            }
            return null!;
        }

        public bool SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                var result = _emailClient.Send(WaitUntil.Completed,
                    senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                    recipientAddress: emailRequest.To,
                    subject: emailRequest.Subject,
                    htmlContent: emailRequest.Body,
                    plainTextContent: emailRequest.PlainText);

                if (result.HasCompleted == true)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SendEmailAsync ::" + ex.Message);

            }

            return false;
        }
    }
}
