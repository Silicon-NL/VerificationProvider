using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<VerificationService> logger, IServiceProvider serviceProvider) : IVerificationService
{
    private readonly ILogger<VerificationService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public VerificationRequestModel UnPackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequestModel>(message.Body.ToString());
            if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
                return verificationRequest;
        }
        catch (Exception ex)
        {

            _logger.LogError($"ERROR: VerificationService.UnPackVerificationRequest :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);

            return code.ToString();
        }
        catch (Exception ex)
        {

            _logger.LogError($"ERROR: VerificationService.GenerateCode :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateServiceBusEmailRequest(EmailRequestModel emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: VerificationService.GenerateServiceBusEmailRequest :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SaveVerificationRequest(VerificationRequestModel verificationRequest, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();

            var existingRequest = await context.AspNetVerificationRequests.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);
            if (existingRequest != null)
            {
                existingRequest.VerificationCode = code;
                existingRequest.ExpirationDate = DateTime.Now.AddMinutes(5);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.AspNetVerificationRequests.Add(new Data.Entities.VerificationRequestEntity()
                {
                    Email = verificationRequest.Email,
                    VerificationCode = code,
                    ExpirationDate = DateTime.Now.AddMinutes(5)
                });
                await context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR: VerificationService.SaveVerificationRequest :: {ex.Message}");
        }
        return false;
    }

    public EmailRequestModel GenerateEmailRequest(VerificationRequestModel verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequestModel
                {
                    To = verificationRequest.Email,
                    Subject = $"Verification Code {code}",
                    HtmlContent = $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Verification Code</title>
                    </head>

                    <body style='font-family: Arial, sans-serif;'>
        
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2>Hi!</h2>
                        <p>Your verification code is: <strong>' + {code} + @'</strong></p>
                        <p>Use this code to verify your account.</p>
                        <p>If you didn't request this code, please ignore this message.</p>
                        <p>Best regards,<br>Silicon<</p>
                    </div>
        
                    </body>
                    </html>",
                    TextContent = $"please verify your account using this verification code {code}. If you didn't request this code, please ignore this message."
                };

                return emailRequest;
            }

        }
        catch (Exception ex)
        {

            _logger.LogError($"ERROR : VerificationService.GenerateEmailRequest :: {ex.Message}");
        }

        return null!;
    }
}

