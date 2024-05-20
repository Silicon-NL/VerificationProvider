using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Models;
using VerificationProvider.Services;

namespace VerificationProvider.Functions
{
    public class GenerateVerificationCodeHttp(ILogger<GenerateVerificationCodeHttp> logger, IVerificationService verificationService)
    {
        private readonly ILogger<GenerateVerificationCodeHttp> _logger = logger;
        private readonly IVerificationService _verificationService = verificationService;

        [Function("GenerateVerificationCodeHttp")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var verificationRequest = JsonConvert.DeserializeObject<VerificationRequestModel>(requestBody);

                if (verificationRequest != null)
                {
                    var code = _verificationService.GenerateCode();
                    if (!string.IsNullOrEmpty(code))
                    {
                        if (await _verificationService.SaveVerificationRequest(verificationRequest, code))
                        {
                            var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
                            if (emailRequest != null)
                            {
                                var payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);
                                if (!string.IsNullOrEmpty(payload))
                                {
                                    return new OkObjectResult(new { Status = 200, Message = "Verification code generated and email request created.", Payload = payload });
                                }
                            }
                        }
                    }
                }
                return new BadRequestObjectResult(new { Status = 400, Message = "Invalid verification request." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : GenerateVerificationCodeHttp.Run :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }  
}
