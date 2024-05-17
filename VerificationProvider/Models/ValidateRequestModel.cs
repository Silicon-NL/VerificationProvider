namespace VerificationProvider.Models;

public class ValidateRequestModel
{
    public string Email { get; set; } = null!;
    public string VerificationCode { get; set; } = null!;
}
