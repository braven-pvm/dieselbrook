using System.ComponentModel.DataAnnotations;

namespace BrevoApiHelpers.Models;

public sealed class ContactModel
{
    [Required]
    public string Email { get; set; } = "";

    private string _firstName = "";
    public string FirstName
    {
        get => _firstName;
        set => _firstName = (value ?? "").Trim();
    }

    private string _lastName = "";
    public string LastName
    {
        get => _lastName;
        set => _lastName = (value ?? "").Trim();
    }

    private string _phone = "";
    public string Phone
    {
        get => _phone;
        set => _phone = (value ?? "").Trim();
    }

    private string _whatsApp = "";
    public string WhatsApp
    {
        get => _whatsApp;
        set => _whatsApp = (value ?? "").Trim();
    }
}
