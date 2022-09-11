﻿using UnityEngine;
using UnityEngine.UIElements;

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
    public string Syntax => "create/delete/info";


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
        var lowerZoneName = zoneName.ToLower();
        Zone zone = null;
        object message = $"/{Name} [{Syntax}] [zone] |[type] [size]|";
        bool FindZone() => (zone = conf.Zones.FirstOrDefault(x => x.Name.ToLower().Contains(lowerZoneName))) is not null;
        void ZoneNotFound() => message = $"Zone {zoneName} not found.";
        switch (action)
        {
            case "create":
                var zoneType = GetArgument(2, true);
                var zoneSize = GetArgument(3, true);
                float.TryParse(zoneSize, out var size);
                size = Math.Max(0.1f, size);
                switch (zoneType)
                {
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
                message = $"Zone {zone.Name = zoneName} created.";
                zone.Position = position;
                conf.Create(zone);
                break;

            case "delete":
            case "remove":
                if(!FindZone())
                {
                    ZoneNotFound();
                    break;
                }
                conf.Delete(zone);
                message = $"Zone {zone.Name} deleted.";
                break;

            case "info":
            case "information":
                if (!FindZone())
                {
                    ZoneNotFound();
                    break;
                }
                message = zone;
                break;
        }
        player.ReceiveMessage(message);
    }
}
