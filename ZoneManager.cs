namespace SZones;

public sealed partial class ZoneManager : RocketPlugin<Config>
{
    public static List<Zone> Zones => conf.Zones;

    public static bool Create(Zone zone)
    {
        if (zone is null || Get(zone.Name) is not null) return false;
        Zones.Add(zone);
        zone.Initialize();
        Save();
        return true;
    }
    public static void Delete(Zone zone)
    {
        if (zone is null) return;
        zone.Dispose();
        Zones.Remove(zone);
        Save();
    }
    public static void Save()
    {
        var config = inst?.Configuration;
        lock (config)
            config.Save();
    }

    protected override void Unload()
    {
        foreach (var zone in Zones)
            zone.Dispose();
    }
    protected override void Load()
    {
        foreach (var zone in Zones)
            zone.Initialize();
        Physics.queriesHitTriggers = false;
    }

    public static Zone Get(string name) => conf.Zones.FindByName(x => x.Name, name);
    public static TZone Get<TZone>(string name) where TZone : Zone => conf.Zones.OfType<TZone>().FindByName(x => x.Name, name);
    public Zone this[string name] => Get(name);
}