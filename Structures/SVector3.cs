namespace SZones;

[XmlType("Vector3")]
public struct SVector3
{
    [XmlAttribute]
    public float X, Y, Z;

    public SVector3() { }
    public SVector3(float x = 0f, float y = 0f, float z = 0f)
    {
        X = x; 
        Y = y; 
        Z = z;
    }

    public static implicit operator Vector3(SVector3 vector) => Convert(vector);
    public static implicit operator SVector3(Vector3 vector) => Convert(vector);
    public static Vector3 Convert(SVector3 vector) => new(vector.X, vector.Y, vector.Z);
    public static SVector3 Convert(Vector3 vector) => new(vector.x, vector.y, vector.z);

    public override string ToString() => ToJsonString(this);
}