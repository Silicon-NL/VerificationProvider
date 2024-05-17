using Microsoft.AspNetCore.Http;
using VerificationProvider.Models;

namespace VerificationProvider.Services
{
    public interface IValidateVerificationCodeService
    {
        Task<ValidateRequestModel> UnpackValidateRequestAsync(HttpRequest req);
        Task<bool> ValidateCodeAsync(ValidateRequestModel validateRequest);
    }
}