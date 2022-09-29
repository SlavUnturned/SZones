namespace SZones;

/// <summary>
/// This class is XML serializing to config file. <br/>
/// Docs: <see href="https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer?view=netframework-4.7.2"/>
/// </summary>
public partial class Config : IRocketPluginConfiguration
{
    public List<Zone> Zones = new();
    public void LoadDefaults()
    {
        SVector3 position = new(19, 35, -54);
        var size = 5f;
        Zones.Add(new SpheroidZone()
        {
            Name = "Spheroid1",
            Position = position,
            Radius = size
        });
        Zones.Add(new CuboidZone()
        {
            Name = "Cuboid1",
            Position = position,
            Size = new(size, size, size)
        });
        Zones.Add(new CustomZone()
        {
            Name = "Custom1",
            Position = position,
            Nodes = new(new List<Vector3>
            {
                new(0, 0, 0), new(0, 0, 1), new(0, 1, 0), new(1, 0, 0),
                new(1, 1, 1), new(0, 1, 1), new(1, 1, 0), new(1 ,0, 1)
            }.Select(x => x * size).Select(SVector3.Convert))
        });
    }
}