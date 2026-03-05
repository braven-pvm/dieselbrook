using BrevoApiHelpers.Models;
public interface IConversationService
{
    Task<List<Conversation>> GetConversationsAsync(DateTime from, DateTime to);
}