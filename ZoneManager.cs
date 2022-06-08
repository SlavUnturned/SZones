namespace SZones;

public sealed partial class ZoneManager : RocketPlugin<Config>
{
    public static List<Zone> Zones => conf.Zones;

    public static void Save() => inst.Configuration.Save();

    protected override void Unload()
    {
        foreach (var zone in Zones)
            zone.Dispose();
    }
    protected override void Load()
    {
        foreach (var zone in Zones)
        {
            zone.Initialize();
#if DEBUG // information for debug
            zone.Controller.OnPlayerEnter += player => PlayerEnterHandler(player, zone);
            zone.Controller.OnPlayerExit += player => PlayerExitHandler(player, zone);
#endif
        }
    }

    public static Zone Get(string name) => Instance[name];
    public Zone this[string name] => Zones.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    static void PlayerExitHandler(Player player, Zone zone)
    {
        player.ReceiveMessage($"Exit {zone.Name}");
    }

    static void PlayerEnterHandler(Player player, Zone zone)
    {
        player.ReceiveMessage($"Enter {zone.Name}");
    }
}