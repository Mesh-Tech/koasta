using CSharpFunctionalExtensions;
using System;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    class GetCoordinatesResult {
        public int Status { get; set; }
        public Coordinates Result { get; set; }
    }

    public class CoordinatesService : ICoordinatesService
    {
        private readonly IWebRequestHelper web;
        public CoordinatesService(IWebRequestHelper web)
        {
            this.web = web;
        }

        public async Task<Maybe<Coordinates>> GetCoordinates(string postcode)
        {
            try {
                var result = await web.GetAsync<GetCoordinatesResult>($"https://api.postcodes.io/postcodes/{postcode.Trim()}").ConfigureAwait(false);

                return (result == null || result.Result == null) ? null : result.Result;
            }
            catch (Exception)
            {
                return Maybe<Coordinates>.None;
            }
        }
    }
}
