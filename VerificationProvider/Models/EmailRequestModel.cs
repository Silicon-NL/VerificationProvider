namespace VerificationProvider.Models;

public class EmailRequestModel
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string HtmlContent { get; set; } = null!;
    public string TextContent { get; set; } = null!;
}
