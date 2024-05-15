using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VerificationProvider.Data.Contexts;

namespace VerificationProvider.Services;

public class VerificationCleanerService(ILogger<VerificationCleanerService> logger, DataContext dataContext) : IVerificationCleanerService
{
    private readonly ILogger<VerificationCleanerService> _logger = logger;
    private readonly DataContext _dataContext = dataContext;

    public async Task RemoveExpiredRecordsAsync()
    {
        try
        {
            var expired = await _dataContext.AspNetVerificationRequests.Where(x => x.ExpirationDate <= DateTime.Now).ToListAsync();
            _dataContext.RemoveRange(expired);
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            _logger.LogError($"ERROR: VerificationCleanerService.RemoveExpiredRecordsAsync :: {ex.Message}");
        }
    }
}
