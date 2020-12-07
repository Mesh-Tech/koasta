using Koasta.Shared.Models;
using System;

namespace Koasta.Shared.Queueing.Models
{
    public enum VenueSyncAction
    {
        Create, Update, Delete
    }

    public class DtoVenueSyncMessage
    {
        public Guid Id { get; private set; }

        public VenueSyncAction ActionType { get; private set; }

        public string AccessToken { get; private set; }

        public Company Company { get; private set; }

        public DtoVenueSyncMessage(Guid id, VenueSyncAction actionType, string accessToken, Company company)
        {
            Id = id;
            ActionType = actionType;
            AccessToken = accessToken;
            Company = company;
        }
    }
}
