using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using Koasta.Shared.Types;

namespace Koasta.Shared.Database
{
    public partial class EmployeeRepository : RepositoryBase<Employee>
    {
        /// <summary>
        /// Fetches a single Employee by their username
        /// </summary>
        /// <param name="username">The username of the Employee you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Employee>>> FetchEmployeeByUsername(string username)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Employee>("SELECT * FROM \"Employee\" WHERE username = @Username", new { Username = username }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<Employee>.None);
                }

                return Result.Ok(Maybe<Employee>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Employee>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Employees for a given company id
        /// </summary>
        /// <param name="companyId">The company id for this request</param>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Employee>>>> FetchEmployeesForCompany(int companyId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Employee>("SELECT * FROM \"Employee\" WHERE companyId = @CompanyId LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count, CompanyId = companyId }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Employee>>.None);
                }

                return Result.Ok(Maybe<List<Employee>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Employee>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Employees for a given venue id
        /// </summary>
        /// <param name="venueId">The venue id for this request</param>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Employee>>>> FetchEmployeesForVenue(int venueId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Employee>("SELECT * FROM \"Employee\" WHERE venueId = @VenueId LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count, VenueId = venueId }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Employee>>.None);
                }

                return Result.Ok(Maybe<List<Employee>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Employee>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces an employee record with the given ID
        /// </summary>
        /// <param name="employeeId">The employee id for this request</param>
        /// <param name="employee">The new employee data</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<bool>> ReplaceEmployee(int employeeId, Employee employee)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync(
                    @"UPDATE ""Employee"" 
                    SET 
                    employeeName = @EmployeeName,
                    username = @Username,
                    passwordHash = @PasswordHash,
                    companyId = @CompanyId,
                    venueId = @VenueId,
                    roleId = @RoleId,
                    securityStamp = @SecurityStamp,
                    confirmed = @Confirmed
                    WHERE employeeId = @EmployeeId"
                , new {
                    EmployeeId = employeeId,
                    employee.EmployeeName,
                    employee.Username,
                    employee.PasswordHash,
                    employee.CompanyId,
                    employee.VenueId,
                    employee.RoleId,
                    employee.SecurityStamp,
                    employee.Confirmed
                }).ConfigureAwait(false);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches Employees by their role id
        /// </summary>
        /// <param name="roleId">The role id of the Employees you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<List<Employee>>>> FetchEmployeeByRoleId(int roleId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<List<Employee>>("SELECT * FROM \"Employee\" WHERE roleId = @RoleId", new { RoleId = roleId }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Employee>>.None);
                }

                return Result.Ok(Maybe<List<Employee>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Employee>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Employees
        /// </summary>
        /// <param name="companyId">The company id to query against</param>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Employee>>>> FetchCountedCompanyEmployees(int companyId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"Employee\" WHERE companyId = @CompanyId; SELECT * FROM \"Employee\" WHERE companyId = @CompanyId LIMIT @Limit OFFSET @Offset", new { CompanyId = companyId, Limit = count, Offset = page * count }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Employee>().ToList();

                var paginatedData = new PaginatedResult<Employee>
                {
                    Data = data ?? new List<Employee>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Employee>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Employee>>>(ex.ToString());
            }
        }
    }
}
