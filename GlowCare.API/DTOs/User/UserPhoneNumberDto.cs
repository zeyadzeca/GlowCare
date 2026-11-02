namespace GlowCare.API.DTOs.User;

public class UserPhoneNumberDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreatePhoneNumberDto
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class UpdatePhoneNumberDto
{
    public string PhoneNumber { get; set; } = string.Empty;
}
