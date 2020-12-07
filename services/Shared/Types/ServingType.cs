namespace Koasta.Shared.Types
{
    public enum ServingType
    {
        BarService,
        TableService,
        TableAndBarService
    }

    public static class ServingTypeHelper {
        public static bool ValidateServingType(int servingType)
        {
            return servingType <= 2;
        }

        public static ServingType[] AllValues()
        {
            return new ServingType[]
            {
                ServingType.BarService,
                ServingType.TableService,
                ServingType.TableAndBarService
            };
        }

        public static string FriendlyName(this ServingType type)
        {
            return type switch
            {
                ServingType.BarService => "Bar Service",
                ServingType.TableService => "Table Service",
                ServingType.TableAndBarService => "Table and Bar Service",
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
