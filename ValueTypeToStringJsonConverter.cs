namespace SZones;

internal sealed class ValueTypeToStringJsonConverter : JsonConverter
{
    public override bool CanRead => false;
    public override bool CanWrite => true;
    public override bool CanConvert(Type type) => type.IsValueType;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue(value.ToString());
    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) => null;
}
