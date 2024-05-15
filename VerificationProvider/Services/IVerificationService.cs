using Azure.Messaging.ServiceBus;
using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public interface IVerificationService
    {
        string GenerateCode();
        EmailRequestModel GenerateEmailRequest(VerificationRequestModel verificationRequest, string code);
        string GenerateServiceBusEmailRequest(EmailRequestModel emailRequest);
        Task<bool> SaveVerificationRequest(VerificationRequestModel verificationRequest, string code);
        VerificationRequestModel UnPackVerificationRequest(ServiceBusReceivedMessage message);
    }
}