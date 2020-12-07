namespace Koasta.Shared.Models
{
    public enum AuthType
    {
        Unknown = 0,
        Facebook = 1,
        Apple = 2,
        Google = 3
    }

    public static class AuthTypeMethods
    {
        public static AuthType ToAuthType(this string s1)
        {
            if (s1 == null)
            {
                return AuthType.Unknown;
            }

            return (s1.ToUpperInvariant()) switch
            {
                "FACEBOOK" => AuthType.Facebook,
                "APPLE" => AuthType.Apple,
                "GOOGLE" => AuthType.Google,
                _ => AuthType.Unknown,
            };
        }
    }
}
