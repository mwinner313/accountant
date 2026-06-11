namespace Bazaar.Identity.Domain;

public class Otp
{
    public const string FixedCode = "1111";

    public int OtpId { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string Code { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedOn { get; set; }

    public static Otp Create(string phoneNumber)
    {
        return new Otp
        {
            PhoneNumber = phoneNumber,
            Code = FixedCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            CreatedOn = DateTime.UtcNow
        };
    }

    public bool IsValid(string code) =>
        Code == code && !IsUsed && ExpiresAt > DateTime.UtcNow;
}
