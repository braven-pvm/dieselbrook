using BrevoApiHelpers.Models;
public interface IDealService
{
    Task<BrevoEmailResponse> CreateDealAsync(string contactEmail, DealModel deal);
    Task<BrevoEmailResponse> UpdateDealAsync(string dealId, DealModel deal);
}