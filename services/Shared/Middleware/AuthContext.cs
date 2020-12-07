using CSharpFunctionalExtensions;
using Koasta.Shared.Models;
using Koasta.Shared.Types;

namespace Koasta.Shared.Middleware
{
    public class AuthContext
    {
        public Maybe<string> AuthHeader { get; set; }
        public Maybe<string> AuthToken { get; set; }
        public AuthType AuthType { get; set; }
        public UserType UserType { get; set; }
        public Maybe<Employee> Employee { get; set; }
        public Maybe<EmployeeRole> EmployeeRole { get; set; }
        public Maybe<User> User { get; set; }
    }
}
