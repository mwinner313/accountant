using Bazaar.Identity.Data;
using Bazaar.Identity.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bazaar.Identity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly IdentityDbContext _dbContext;
    private readonly ILogger<OtpController> _logger;

    public OtpController(IdentityDbContext dbContext, ILogger<OtpController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest("Phone number is required.");

        // Invalidate previous unused OTPs for this phone
        var existing = await _dbContext.Otps
            .Where(o => o.PhoneNumber == request.PhoneNumber && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var e in existing)
            e.IsUsed = true;

        var otp = Otp.Create(request.PhoneNumber);
        _dbContext.Otps.Add(otp);
        await _dbContext.SaveChangesAsync(ct);

        // In production: send via SMS. For now, log the code.
        _logger.LogInformation("OTP for {Phone}: {Code}", request.PhoneNumber, otp.Code);

        return Ok(new { Message = "OTP sent successfully." });
    }
}

public class RequestOtpRequest
{
    public string PhoneNumber { get; set; } = default!;
}
