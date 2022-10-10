namespace SZones;

public class ZoneCommand : IRocketCommand
{
    public ZoneCommand()
    {
        permissions = new() { Name };
        aliases = new() { };
    }
    public AllowedCaller AllowedCaller => AllowedCaller.Player;

    public string Name => "zone";

    public string Help => Name;
    public string Syntax => "create/delete/node/tp/info";


    private readonly List<string> aliases = new();
    public List<string> Aliases => aliases;

    private readonly List<string> permissions;
    public List<string> Permissions => permissions;


    public void Execute(IRP caller, string[] command)
    {
        string GetArgument(int idx, bool toLower = false)
        {
            var arg = "" + command.ElementAtOrDefault(idx);
            return toLower ? arg.ToLower() : arg;
        }
        var up = (UP)caller;
        var player = up.Player;
        var position = up.Position;
        var action = GetArgument(0, true);
        var zoneName = GetArgument(1);
        Zone zone = null;
        object message = TranslateHelp(Name, Syntax);
        void Return(string msg = null)
        {
            message = msg ?? message;
            throw new WrongUsageOfCommandException(null, null);
        }
        void TryFindZone()
        {
            if (!FindZone())
                ZoneNotFound();
        }
        bool FindZone() => (zone = ZoneManager.Get(zoneName)) is not null;
        void ZoneNotFound() => Return(TranslateNotFound(zoneName));
        try
        {
            switch (action)
            {
                case "create":
                    {
                        if (FindZone()) ZoneManager.Delete(zone);
                        var zoneType = GetArgument(2, true);
                        var zoneSize = GetArgument(3, true);
                        float.TryParse(zoneSize, out var size);
                        size = Math.Max(0.1f, size);
                        switch (zoneType)
                        {
                            case "custom":
                                zone = new CustomZone { };
                                break;
                            case "cube":
                            case "cuboid":
                                zone = new CuboidZone { Size = size * Vector3.one };
                                break;
                            default:
                            case "sphere":
                            case "spheroid":
                                zone = new SpheroidZone { Radius = size };
                                break;
                        }
                        message = TranslateCreate(zone.Name = zoneName);
                        zone.Position = position;
                        ZoneManager.Create(zone);
                        break;
                    }

                case "delete":
                case "remove":
                    {
                        TryFindZone();
                        ZoneManager.Delete(zone);
                        message = TranslateDelete(zoneName);
                        break;
                    }

                case "node":
                    {
                        TryFindZone();
                        if (zone is not CustomZone customZone)
                            ZoneNotFound();
                        customZone.Nodes.Add(position);
                        ZoneManager.Save();
                        message = TranslateNode(zone.Name);
                        break;
                    }
                case "tp":
                case "teleport":
                    {
                        TryFindZone();
                        up.Teleport(zone.Position, up.Rotation);
                        message = TranslateTeleport(zone.Name);
                        break;
                    }

                case "info":
                case "information":
                    {
                        TryFindZone();
                        message = zone;
                        break;
                    }
            }
        }
        catch (WrongUsageOfCommandException) { }
        player.ReceiveMessage(message);
    }
}
