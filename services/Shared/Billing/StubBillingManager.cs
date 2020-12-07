using System.Collections.Generic;
using System.Threading.Tasks;
using Koasta.Shared.Models;

namespace Koasta.Shared.Billing
{
    public class StubBillingManager : IBillingManager
    {
        public Task<string> AddSubscriptionPackage(BillingSubscriptionPackage newPlan)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> CreateCustomer(User user)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> CreateEphemeralKey(string customerId)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> CreateSubscription(string customerId, List<string> packageIdentifiers)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteSubscription(string subscriptionId)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteSubscriptionPackage(string planId)
        {
            throw new System.NotImplementedException();
        }

        public Task FinaliseOrderPayment(Order order)
        {
            return Task.CompletedTask;
        }

        public Task<BillingSubscriptionPackage> GetSubscriptionPackage(string planId)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> RegisterBillingAccount(Company company, BillingAccountDetails newDetails)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> RegisterBillingCustomer(Company company, BillingAccountDetails newDetails)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> ReserveOrderPayment(User user, BillingOrder order, string paymentSourceId, string destinationAccountId, string verificationReference)
        {
            return Task.FromResult("abc");
        }

        public Task UpdateBillingAccount(Company company, BillingAccountDetails newDetails)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateBillingCustomer(Company company, BillingAccountDetails newDetails)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateSubscription(string customerId, string subscriptionId, List<string> packageIdentifiers)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateSubscriptionPackage(string planId, BillingSubscriptionPackage updatedPlan)
        {
            throw new System.NotImplementedException();
        }
    }
}
