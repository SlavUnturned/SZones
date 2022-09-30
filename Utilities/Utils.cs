global using Rocket.API.Collections;
global using Rocket.API;
global using Rocket.API.Serialisation;
global using Rocket.Core;
global using Rocket.Core.Assets;
global using Rocket.Core.Logging;
global using Rocket.Core.Plugins;
global using Rocket.Core.Utils;
global using Rocket.Unturned.Chat;
global using Rocket.Unturned.Events;
global using Rocket.Unturned;
global using Rocket.Unturned.Player;
global using SDG.Unturned;
global using SDG.NetPak;
global using SDG.NetTransport;
global using SDG.Framework.Landscapes;
global using SDG.Framework.Utilities;
global using System;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Runtime.Serialization;
global using System.Collections;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Xml.Serialization;
global using Steamworks;
global using UnityEngine;
global using Newtonsoft.Json;
global using Color = UnityEngine.Color;
global using UnityComponent = UnityEngine.Component;
global using UnityObject = UnityEngine.Object;
global using UnityBehaviour = UnityEngine.MonoBehaviour;
global using Logger = Rocket.Core.Logging.Logger;
global using UP = Rocket.Unturned.Player.UnturnedPlayer;
global using UPlayer = Rocket.Unturned.Player.UnturnedPlayer;
global using IRP = Rocket.API.IRocketPlayer;
global using IRPlayer = Rocket.API.IRocketPlayer;
global using SP = SDG.Unturned.SteamPlayer;
global using SPlayer = SDG.Unturned.SteamPlayer;
global using P = SDG.Unturned.Player;
global using Player = SDG.Unturned.Player;
global using Vehicle = SDG.Unturned.InteractableVehicle;
global using Storage = SDG.Unturned.InteractableStorage;
global using InventoryPage = SDG.Unturned.Items;
global using static SZones.Utils;

namespace SZones;

public static partial class Utils
{
    // just shorthands, you can simply remove/rename them if you want to.
    public static ZoneManager inst => ZoneManager.Instance;
    public static Config conf => ZoneManager.Instance.Configuration.Instance;
    //

    public static T FindByName<T>(this IEnumerable<T> enumerable, Func<T, string> nameGetter, string name, bool fullName = false)
    {
        var lowerName = name.ToLower();
        return enumerable.FirstOrDefault(fullName ?
            (x => nameGetter(x).Equals(lowerName, StringComparison.OrdinalIgnoreCase)) :
            (x => nameGetter(x).ToLower().Contains(lowerName))
        );
    }

    public static void ReceiveMessage(this P player, object message, Color? color = null)
    {
        foreach (var text in ("" + message).Split('\n'))
            TaskDispatcher.QueueOnMainThread(() =>
            {
                ChatManager.serverSendMessage(text, color ?? Color.cyan, null, player.channel.owner);
            });
    }
    public static string ToJsonString(object obj, bool showType = false, params JsonConverter[] converters) => obj is null ? "null" :
        (showType ? (obj.GetType().Name + " ") : "") +
        JsonConvert.SerializeObject(obj, converters: converters)
        .Replace("\":", ": ")
        .Replace("\",", "\", ")
        .Replace("\"", "");

    public static List<Transform> GetBarricades(this IList<RegionCoordinate> regions, Func<Vector3, bool> isInside)
    {
        List<Transform> result = new();
        if (BarricadeManager.vehicleRegions is null) return result;
        for (int i = 0; i < regions.Count; i++)
        {
            var regionCoordinate = regions[i];
            var region = BarricadeManager.regions[regionCoordinate.x, regionCoordinate.y];
            if (region is null) continue;
            foreach (var drop in region.drops)
            {
                var model = drop.model;
                if (!model || !isInside(model.position))
                    continue;
                result.Add(model);
            }
        }
        return result;
    }
    public static List<Vehicle> GetVehicles(Func<Vector3, bool> isInside)
    {
        List<Vehicle> result = new();
        foreach (var vehicle in VehicleManager.vehicles)
            if (isInside(vehicle.transform.position))
                result.Add(vehicle);
        return result;
    }
    public static List<Zombie> GetZombies(Vector3 center, Func<Vector3, bool> isInside)
    {
        List<Zombie> result = new();
        if (ZombieManager.regions is null)
            return result;
        if (!LevelNavigation.tryGetNavigation(center, out var regionIdx))
            return result;
        var region = ZombieManager.regions[regionIdx];
        if (region is null || region.zombies is null)
            return result;
        for (int i = 0; i < region.zombies.Count; i++)
        {
            var zombie = region.zombies[i];
            if (zombie is null || !isInside(zombie.transform.position)) continue;
            result.Add(zombie);
        }
        return result;
    }
    public static List<Animal> GetAnimals(Func<Vector3, bool> isInside)
    {
        List<Animal> result = new();
        if (AnimalManager.animals is null) return result;
        for (int i = 0; i < AnimalManager.animals.Count; i++)
        {
            var animal = AnimalManager.animals[i];
            if (isInside(animal.transform.position))
                result.Add(animal);
        }
        return result;
    }

    public static List<RegionCoordinate> GetRegions(this Bounds bounds)
    {
        List<RegionCoordinate> result = new();
        var center = bounds.center;
        var extents = bounds.extents;
        Vector3 startPoint = new(center.x - extents.x, center.y, center.z - extents.z);
        Vector3 endPoint = new(center.x + extents.x, center.y, center.z + extents.z);
        getUnsafeCoordinates(startPoint, out var x1, out var y1);
        getUnsafeCoordinates(endPoint, out var x2, out var y2);
        if (x1 >= Regions.WORLD_SIZE || y1 >= Regions.WORLD_SIZE || x2 < 0 || y2 < 0)
            return result;
        x1 = Mathf.Max(x1, 0);
        x2 = Mathf.Max(x2, 0);
        y1 = Mathf.Min(y1, Regions.WORLD_SIZE - 1);
        y2 = Mathf.Min(y2, Regions.WORLD_SIZE - 1);
        for (byte x = (byte)x1; x <= x2; x++)
            for (byte y = (byte)y1; y <= y2; y++)
                result.Add(new(x, y));
        return result;
    }

    private static void getUnsafeCoordinates(Vector3 point, out int x, out int y)
    {
        x = Mathf.FloorToInt((point.x + 4096f) / Regions.REGION_SIZE);
        y = Mathf.FloorToInt((point.z + 4096f) / Regions.REGION_SIZE);
    }
}
