using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions
{
    public class GenerateVerificationCodeHttp(ILogger<GenerateVerificationCodeHttp> logger, IVerificationService verificationService, ServiceBusClient serviceBusClient)
    {
        private readonly ILogger<GenerateVerificationCodeHttp> _logger = logger;
        private readonly IVerificationService _verificationService = verificationService;
        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

        [Function("GenerateVerificationCodeHttp")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var verificationRequest = JsonConvert.DeserializeObject<VerificationRequestModel>(requestBody);

                if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
                {
                    var code = _verificationService.GenerateCode();

                    var saved = await _verificationService.SaveVerificationRequest(verificationRequest, code);
                    if (!saved)
                    {
                        throw new Exception("Failed to save verification request.");
                    }

                    var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
                    var payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);

                    var sender = _serviceBusClient.CreateSender("email_request");
                    var message = new ServiceBusMessage(payload);
                    await sender.SendMessageAsync(message);

                    _logger.LogInformation("Message sent to Service Bus successfully.");

                    
                    var response = new OkObjectResult(new { Status = 200, Message = "Verification code generated and email request created.", Payload = payload });
                    return response;
                }

                
                var badRequestResponse = new BadRequestObjectResult(new { Status = 400, Message = "Invalid verification request." });
                return badRequestResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR: {ex.Message}");
                var badRequestResponse = new BadRequestObjectResult(new { Status = 500, Message = "Internal server error." });
                return badRequestResponse;
            }
        }
    }
}
