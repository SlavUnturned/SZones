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
        Zones.Add(new SpheroidZone()
        {
            Name = "Spheroid1",
            Position = new(829, 29, 714),
            Radius = 5
        });
        Zones.Add(new CuboidZone()
        {
            Name = "Cuboid1",
            Position = new(829, 29, 714),
            Size = new(5, 5, 5)
        });
    }
    public void Create(Zone zone)
    {
        conf.Zones.Add(zone);
        zone.Initialize();
        Save();
    }
    public void Delete(Zone zone)
    {
        zone.Dispose();
        conf.Zones.Remove(zone);
        Save();
    }
    public void Save()
    {
        var config = inst.Configuration;
        lock (config)
            config.Save();
    }
}