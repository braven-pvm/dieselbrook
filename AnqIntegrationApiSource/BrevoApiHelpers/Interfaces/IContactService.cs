using BrevoApiHelpers.Models;
public interface IContactService
{
    Task<bool> ContactExistsAsync(string email);
    Task<BrevoEmailResponse> AddContactAsync(ContactModel contact, List<int> listIds);
    Task<BrevoEmailResponse> UpdateContactAsync(string email, Dictionary<string, object> attributes);

    Task<int?> GetContactIdByEmailAsync(string email,CancellationToken ct = default);

}
