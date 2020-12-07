using System.Collections.Generic;
using System.Threading.Tasks;
using Koasta.Shared.Models;

namespace Koasta.Shared.Billing
{
    public interface IBillingManager
    {
        Task<string> RegisterBillingAccount(Company company, BillingAccountDetails newDetails);
        Task UpdateBillingAccount(Company company, BillingAccountDetails newDetails);

        Task<string> RegisterBillingCustomer(Company company, BillingAccountDetails newDetails);
        Task UpdateBillingCustomer(Company company, BillingAccountDetails newDetails);

        Task<string> CreateSubscription(string customerId, List<string> packageIdentifiers);
        Task UpdateSubscription(string customerId, string subscriptionId, List<string> packageIdentifiers);
        Task DeleteSubscription(string subscriptionId);

        Task<string> AddSubscriptionPackage(BillingSubscriptionPackage newPlan);
        Task UpdateSubscriptionPackage(string planId, BillingSubscriptionPackage updatedPlan);
        Task DeleteSubscriptionPackage(string planId);
        Task<BillingSubscriptionPackage> GetSubscriptionPackage(string planId);

        Task<string> ReserveOrderPayment(User user, BillingOrder order, string paymentSourceId, string destinationAccountId, string verificationReference);
        Task FinaliseOrderPayment(Order order);

        Task<string> CreateEphemeralKey(string customerId);
        Task<string> CreateCustomer(User user);
    }
}
