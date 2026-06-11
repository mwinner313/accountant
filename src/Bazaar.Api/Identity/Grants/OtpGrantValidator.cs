using Bazaar.Identity.Data;
using Bazaar.Identity.Domain;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bazaar.Identity.Grants;

public class OtpGrantValidator : IExtensionGrantValidator
{
    private readonly IdentityDbContext _dbContext;

    public OtpGrantValidator(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string GrantType => "otp_verification";

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var phone = context.Request.Raw.Get("phone");
        var otpCode = context.Request.Raw.Get("otp_code");

        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(otpCode))
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "phone and otp_code are required.");
            return;
        }

        var otp = await _dbContext.Otps
            .Where(o => o.PhoneNumber == phone && !o.IsUsed)
            .OrderByDescending(o => o.CreatedOn)
            .FirstOrDefaultAsync();

        if (otp is null || !otp.IsValid(otpCode))
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid or expired OTP.");
            return;
        }

        // Mark OTP as used
        otp.IsUsed = true;

        // Find or create user
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                PhoneNumber = phone,
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
        }

        await _dbContext.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.MobilePhone, phone)
        };

        context.Result = new GrantValidationResult(
            subject: user.Id.ToString(),
            authenticationMethod: "otp",
            claims: claims);
    }
}
