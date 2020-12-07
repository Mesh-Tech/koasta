using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    public class Coordinates {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

    public interface ICoordinatesService
    {
         Task<Maybe<Coordinates>> GetCoordinates(string postcode);
    }
}
