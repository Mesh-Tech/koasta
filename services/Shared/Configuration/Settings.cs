using System;

namespace Koasta.Shared.Configuration
{
    public interface ISettings : IDisposable
    {
        Data Connection { get; }
        Meta Meta { get; }
        Auth Auth { get; }
    }
}
