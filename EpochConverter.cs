using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// Credit
// https://stackoverflow.com/users/65387/mpen
// https://stackoverflow.com/a/19972214
public class EpochConverter : DateTimeConverterBase
{
    private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteRawValue(((DateTime)value - _epoch).TotalMilliseconds + "000");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
        {
            return null;
        }

        // Handle timestamp format in string with decimal
        if (reader.Value.GetType() == typeof(System.String))
        {
            decimal output;
            if (Decimal.TryParse((string)reader.Value, out output))
            {
                return DateTimeOffset.FromUnixTimeSeconds((long)output).LocalDateTime;
            }

            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds((long)reader.Value).LocalDateTime;
    }
}