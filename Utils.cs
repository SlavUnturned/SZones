global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Xml.Serialization;
global using Rocket.API;
global using Rocket.API.Collections;
global using Rocket.API.Serialisation;
global using Rocket.Core;
global using Rocket.Core.Assets;
global using Rocket.Core.Logging;
global using Rocket.Core.Plugins;
global using Rocket.Unturned;
global using Rocket.Unturned.Chat;
global using Rocket.Unturned.Events;
global using Rocket.Unturned.Player;
global using SDG.Unturned;
global using Steamworks;
global using UnityEngine;
global using static SZones.Utils;
global using IRP = Rocket.API.IRocketPlayer;
global using Logger = Rocket.Core.Logging.Logger;
global using UP = Rocket.Unturned.Player.UnturnedPlayer;
global using V = SDG.Unturned.InteractableVehicle;
global using P = SDG.Unturned.Player;
global using SP = SDG.Unturned.SteamPlayer;
global using Color = UnityEngine.Color;
global using static UnityEngine.Object;
using Rocket.Core.Utils;

namespace SZones;

public static partial class Utils
{
    // just shorthands, you can simply remove/rename them if you want to.
    public static ZoneManager inst => ZoneManager.Instance;
    public static Config conf => ZoneManager.Instance.Configuration.Instance;
    //
    public static void ReceiveMessage(this P player, string message, Color? color = null)
    {
        TaskDispatcher.QueueOnMainThread(() => {
            ChatManager.serverSendMessage(message, color ?? Color.cyan, null, player.channel.owner);
        });
    }
}
