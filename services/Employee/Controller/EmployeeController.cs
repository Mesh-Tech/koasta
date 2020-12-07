using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Koasta.Service.EmployeeService.Models;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using Koasta.Shared.Crypto;

namespace Koasta.Service.EmployeeService
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/employee")]
    public class EmployeeController : Controller
    {
        private readonly EmployeeRepository employees;
        private readonly EmployeeRoleRepository employeeRoles;
        private readonly ICryptoHelper crypto;

        public EmployeeController(EmployeeRepository employees, EmployeeRoleRepository employeeRoles, ICryptoHelper crypto)
        {
            this.crypto = crypto;
            this.employees = employees;
            this.employeeRoles = employeeRoles;
        }

        [HttpGet]
        [ActionName("list_employees")]
        [ProducesResponseType(typeof(List<DtoEmployee>), 200)]
        public async Task<IActionResult> GetEmployees([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await employees.FetchEmployees(page, count)
              .OnSuccess(e => e.HasValue ? e.Value.Select(employee => (DtoEmployee)employee).ToList() : new List<DtoEmployee>())
              .OnBoth(e => e.IsFailure ? StatusCode(500, "") : StatusCode(200, e.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("list_company_employees")]
        [Route("company/{companyId}")]
        [ProducesResponseType(typeof(List<DtoEmployee>), 200)]
        public async Task<IActionResult> GetCompanyEmployees([FromRoute(Name = "companyId")] int companyId, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await employees.FetchEmployeesForCompany(companyId, page, count)
              .OnSuccess(e => e.HasValue ? e.Value.Select(employee => (DtoEmployee)employee).ToList() : new List<DtoEmployee>())
              .OnBoth(e => e.IsFailure ? StatusCode(500, "") : StatusCode(200, e.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("list_venue_employees")]
        [Route("venue/{venueId}")]
        [ProducesResponseType(typeof(List<DtoEmployee>), 200)]
        public async Task<IActionResult> GetVenueEmployees([FromRoute(Name = "venueId")] int venueId, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await employees.FetchEmployeesForVenue(venueId, page, count)
              .OnSuccess(e => e.HasValue ? e.Value.Select(employee => (DtoEmployee)employee).ToList() : new List<DtoEmployee>())
              .OnBoth(e => e.IsFailure ? StatusCode(500, "") : StatusCode(200, e.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("list_employee_roles")]
        [Route("roles")]
        [ProducesResponseType(typeof(List<EmployeeRole>), 200)]
        public async Task<IActionResult> GetRoles()
        {
            return await employeeRoles.FetchEmployeeRoles(0, 2000)
              .OnSuccess(e => e.HasValue ? e.Value : new List<EmployeeRole>())
              .OnBoth(e => e.IsFailure ? StatusCode(500, "") : StatusCode(200, e.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [ActionName("create_employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] DtoCreateEmployeeRequest request)
        {
            if (ModelState.ValidationState != ModelValidationState.Valid)
            {
                return BadRequest();
            }

            return await employees.CreateEmployee(new Employee
            {
                VenueId = request.VenueId,
                CompanyId = request.CompanyId,
                Username = request.Username,
                PasswordHash = crypto.Generate(request.Password),
                EmployeeName = request.EmployeeName,
                RoleId = request.RoleId
            })
            .Ensure(e => e.HasValue, "Employee was created")
            .OnBoth(e => e.IsFailure ? StatusCode(500) : StatusCode(201))
            .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_employee")]
        [Route("{employeeId}")]
        [ProducesResponseType(typeof(DtoEmployee), 200)]
        public async Task<IActionResult> GetEmployee([FromRoute(Name = "employeeId")] int employeeId)
        {
            return await employees.FetchEmployee(employeeId)
              .Ensure(e => e.HasValue, "Employee exists")
              .OnBoth(e => e.IsFailure ? StatusCode(404, "") : StatusCode(200, e.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [ActionName("delete_employee")]
        [Route("{employeeId}")]
        public async Task<IActionResult> DropEmployee([FromRoute(Name = "employeeId")] int employeeId)
        {
            return await employees.DropEmployee(employeeId)
              .OnBoth(e => e.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [ActionName("update_employee")]
        [Route("{employeeId}")]
        public async Task<IActionResult> ReplaceEmployee([FromRoute(Name = "employeeId")] int employeeId, [FromBody] DtoCreateEmployeeRequest request)
        {
            if (ModelState.ValidationState != ModelValidationState.Valid)
            {
                return BadRequest();
            }

            return await employees.ReplaceEmployee(new Employee
            {
                EmployeeId = employeeId,
                VenueId = request.VenueId,
                CompanyId = request.CompanyId,
                Username = request.Username,
                PasswordHash = crypto.Generate(request.Password),
                EmployeeName = request.EmployeeName,
                RoleId = request.RoleId
            })
            .OnBoth(e => e.IsFailure ? StatusCode(500) : StatusCode(200))
            .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_own_employee")]
        [Route("me")]
        [ProducesResponseType(typeof(DtoOwnEmployeeResponse), 200)]
        public IActionResult GetOwnEmployee()
        {
            var authContext = this.GetAuthContext();
            if (!authContext.Employee.HasValue || !authContext.EmployeeRole.HasValue)
            {
                return NotFound();
            }

            return Ok(new DtoOwnEmployeeResponse
            {
                EmployeeId = authContext.Employee.Value.EmployeeId,
                EmployeeName = authContext.Employee.Value.EmployeeName,
                Username = authContext.Employee.Value.Username,
                CompanyId = authContext.Employee.Value.CompanyId,
                VenueId = authContext.Employee.Value.VenueId,
                RoleId = authContext.Employee.Value.RoleId,
                CanWorkWithVenue = authContext.EmployeeRole.Value.CanWorkWithVenue,
                CanAdministerVenue = authContext.EmployeeRole.Value.CanAdministerVenue,
                CanWorkWithCompany = authContext.EmployeeRole.Value.CanWorkWithCompany,
                CanAdministerCompany = authContext.EmployeeRole.Value.CanAdministerCompany,
                CanAdministerSystem = authContext.EmployeeRole.Value.CanAdministerSystem,
            });
        }
    }
}
