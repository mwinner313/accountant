namespace Bazaar.Identity.Domain;

public class AppUser
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}
