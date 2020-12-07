using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koasta.Shared.Database
{
    public partial class CompanyRepository : RepositoryBase<Company>
    {
        /// <summary>
        /// Fetches a single Company
        /// </summary>
        /// <param name="venueId">The id of the Venue whose Company you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<BillingCompanyVenuePair>>> FetchCompanyByVenue(int venueId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<BillingCompanyVenuePair>("SELECT cmp.companyId, cmp.externalAccountId, ven.venueId, ven.venueName FROM \"Venue\" ven INNER JOIN \"Company\" cmp ON ven.companyId = cmp.companyId WHERE ven.venueId = @VenueId", new { VenueId = venueId }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<BillingCompanyVenuePair>.None);
                }

                return Result.Ok(Maybe<BillingCompanyVenuePair>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<BillingCompanyVenuePair>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<IEnumerable<CompanyTokenData>>>> GetExipringCompanyTokens(int numberOfDaysForExpiry)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<CompanyTokenData>("SELECT companyid, externalaccesstoken, externalrefreshtoken, externaltokenexpiry FROM \"Company\" WHERE externaltokenexpiry <= NOW() + INTERVAL '4 days'", new { expiryDays = numberOfDaysForExpiry }).ConfigureAwait(false));
                if (data == null)
                {
                    return Result.Ok(Maybe<IEnumerable<CompanyTokenData>>.None);
                }

                return Result.Ok(Maybe<IEnumerable<CompanyTokenData>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<IEnumerable<CompanyTokenData>>>(ex.ToString());
            }
        }
    }
}
