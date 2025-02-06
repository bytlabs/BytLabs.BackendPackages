using MongoDB.Bson;
using System.Globalization;

namespace BytLabs.DataAccess.MongoDB.DynamicData
{
    public static class BsonDocumentExtensions
    {
        public static void ConvertIsoStringsToBsonDates(this BsonDocument document)
        {
            foreach (var element in document.ToList()) // Avoid modifying while iterating
            {
                if (element.Value.IsString)
                {
                    string strValue = element.Value.AsString;

                    if (DateTime.TryParse(strValue, null, DateTimeStyles.RoundtripKind, out DateTime dateTime))
                    {
                        document[element.Name] = new BsonDateTime(dateTime);
                    }
                }
                else if (element.Value.IsBsonDocument)
                {
                    element.Value.AsBsonDocument.ConvertIsoStringsToBsonDates();
                }
                else if (element.Value.IsBsonArray)
                {
                    ConvertIsoStringsInBsonArray(element.Value.AsBsonArray);
                }
            }
        }

        private static void ConvertIsoStringsInBsonArray(BsonArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].IsString)
                {
                    string strValue = array[i].AsString;

                    if (DateTime.TryParse(strValue, null, DateTimeStyles.RoundtripKind, out DateTime dateTime))
                    {
                        array[i] = new BsonDateTime(dateTime);
                    }
                }
                else if (array[i].IsBsonDocument)
                {
                    array[i].AsBsonDocument.ConvertIsoStringsToBsonDates();
                }
                else if (array[i].IsBsonArray)
                {
                    ConvertIsoStringsInBsonArray(array[i].AsBsonArray);
                }
            }
        }

        public static void ConvertBsonDatesToIsoStrings(this BsonDocument document)
        {
            foreach (var element in document.ToList())
            {
                if (element.Value.IsBsonDateTime)
                {
                    document[element.Name] = element.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else if (element.Value.IsBsonDocument)
                {
                    element.Value.AsBsonDocument.ConvertBsonDatesToIsoStrings();
                }
                else if (element.Value.IsBsonArray)
                {
                    ConvertBsonDatesInBsonArray(element.Value.AsBsonArray);
                }
            }
        }

        private static void ConvertBsonDatesInBsonArray(BsonArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i].IsBsonDateTime)
                {
                    array[i] = array[i].ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                }
                else if (array[i].IsBsonDocument)
                {
                    array[i].AsBsonDocument.ConvertBsonDatesToIsoStrings();
                }
                else if (array[i].IsBsonArray)
                {
                    ConvertBsonDatesInBsonArray(array[i].AsBsonArray);
                }
            }
        }
    }
}
