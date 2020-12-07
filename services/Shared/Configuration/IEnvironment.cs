namespace Koasta.Shared.Configuration
{
    public interface IEnvironment
    {
        bool IsProduction { get; }

        bool IsTest { get; }

        bool IsDevelopment { get; }
    }
}
