using Koasta.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    public class PasswordDictionaryChecker : IPasswordValidator<Employee>
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly HashSet<string> dictionary;

        public PasswordDictionaryChecker(IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
            dictionary = LoadDictionary();
        }

        public Task<IdentityResult> ValidateAsync(UserManager<Employee> manager, Employee user, string password)
        {
            if (dictionary.Contains(password))
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "TOOCOMMON", Description = "This password is commonly used on the internet and can leave your account vulnerable to attack. Please try another." }));
            }
            return Task.FromResult(IdentityResult.Success);
        }

        private HashSet<string> LoadDictionary()
        {
            var filename = Path.Combine(hostingEnvironment.ContentRootPath, "Data", "password-dictionary.txt");
            return new HashSet<string>(File.ReadLines(filename));
        }
    }
}
