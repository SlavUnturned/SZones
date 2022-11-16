namespace SZones;

public sealed partial class ZoneManager : RocketPlugin<Config>
{
    public static List<Zone> Zones => conf.Zones;

    public static bool Create(Zone zone)
    {
        if (zone is null || Get(zone.Name) is not null) return false;
        Zones.Add(zone);
        Save();
        SafeInitialize(zone);
        return true;
    }
    public static void Delete(Zone zone)
    {
        if (zone is null) return;
        SafeFinalize(zone);
        Zones.Remove(zone);
        Save();
    }
    public static void Save()
    {
        var config = inst?.Configuration;
        lock (config) config.Save();
    }

    public static void SafeFinalize(Zone zone)
    {
        try
        {
            zone.Finalize();
        }
        catch (Exception ex) { Logger.Log(ex); }
    }
    public static void SafeInitialize(Zone zone)
    {
        try
        {
            zone.Initialize();
        }
        catch (Exception ex) { Logger.Log(ex); }
    }

    protected override void Unload()
    {
        foreach (var zone in Zones)
            SafeFinalize(zone);
    }
    protected override void Load()
    {
        foreach (var zone in Zones)
            SafeInitialize(zone);
    }

    public static Zone Get(string name) => conf.Zones.FindByName(x => x.Name, name);
    public static TZone Get<TZone>(string name) where TZone : Zone => conf.Zones.OfType<TZone>().FindByName(x => x.Name, name);
    public Zone this[string name] => Get(name);
}