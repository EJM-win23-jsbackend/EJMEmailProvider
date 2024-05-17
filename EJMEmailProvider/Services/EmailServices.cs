using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Azure;
using EJMEmailProvider.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<string> UnpackUnsubscriberAsync(ServiceBusReceivedMessage message)
        {
            try
            {
                var request = message.Body.ToString();

                if (request != null)
                {
                    return request;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UnpackUnsubscriberAsync : Run ::" + ex.Message);

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

        public async Task<bool> SendUnsubscribeEmailAsync(string email)
        {
            try
            {
                var emailToSend = new EmailRequest
                {
                    To = email,
                    Subject = "You have been unsubscribed",
                    Body = $"<html><body><strong>Hello,</strong><br><br>You have been unsubscribed from our mailing list.</body></html>",
                    PlainText = "Hello, you have been unsubscribed from our mailing list."

                };

                var response = await _emailClient.SendAsync(WaitUntil.Completed,
                    senderAddress: Environment.GetEnvironmentVariable("SenderAddress"),
                    recipientAddress: emailToSend.To,
                    subject: emailToSend.Subject,
                    htmlContent: emailToSend.Body,
                    plainTextContent: emailToSend.PlainText);

                if (response.HasCompleted)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {email}. StatusCode: {ex.Message}");
            }

            return false;
        }
    }
}
