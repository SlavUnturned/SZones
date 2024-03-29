﻿using UnityEngine;

namespace SZones;

public abstract partial class ZoneController : UnityBehaviour
{
    public delegate void StateUpdateHandler<T>(T target);
    public event StateUpdateHandler<GameObject> OnEnter, OnExit;
    public event StateUpdateHandler<Player> OnPlayerEnter, OnPlayerExit;
    public event StateUpdateHandler<Zombie> OnZombieEnter, OnZombieExit;
    public event StateUpdateHandler<Animal> OnAnimalEnter, OnAnimalExit;
    public event StateUpdateHandler<Vehicle> OnVehicleEnter, OnVehicleExit;

    public Zone Zone { get; private set; }
    public virtual Collider Collider { get; protected set; }

    internal void Initialize(Zone zone)
    {
        Zone = zone;
    }

    #region Entered State
    protected readonly HashSet<Collider> enteredColliders = new();
    public virtual IReadOnlyCollection<Collider> EnteredColliders => enteredColliders;
    public virtual IReadOnlyCollection<GameObject> EnteredObjects => enteredColliders.Where(x => x).Select(x => x.gameObject).Where(x => x).ToList();
    protected virtual bool SetEnterState(Collider other, bool state)
    {
        var entered = enteredColliders.Contains(other);
        if (state == entered) return state;
        if (!other) return state;

        var @object = other.gameObject;

        bool Set<T>(Func<T, bool, bool> func) => state = func(@object.GetComponent<T>(), state);
        Set<Player>(SetEnterState);
        Set<Vehicle>(SetEnterState);
        Set<Animal>(SetEnterState);
        Set<Zombie>(SetEnterState);

        if (state == entered) return state;

        if (state) enteredColliders.Add(other);
        else enteredColliders.Remove(other);

        InvokeEventsSafe(@object, state, OnEnter, OnExit);
        return state;
    }

    protected virtual bool UpdateEnterState(Collider other)
    {
        if (!other) return false;
        var state = IsPositionInside(other) && TryCheck(other);
        return SetEnterState(other, state);
    }
    protected virtual bool UpdateEnterState(UnityComponent component) => UpdateEnterState(component?.GetComponent<Collider>());
    protected virtual void OnTriggerStay(Collider other) => SetEnterState(other, true);
    protected virtual void OnTriggerExit(Collider other) => SetEnterState(other, false);

    protected readonly HashSet<CSteamID> enteredPlayers = new();
    public virtual IReadOnlyCollection<CSteamID> EnteredPlayers => enteredPlayers;

    protected bool SetEnterState(SPlayer target, bool state) => SetEnterState(target?.player, state);
    protected bool SetEnterState(Player target, bool state)
    {
        if (!target) return state;
        if (!TrySetEnterState(target.channel.owner.playerID.steamID, state)) return state;

        InvokeEventsSafe(target, state, OnPlayerEnter, OnPlayerExit);
        return state;
    }
    protected bool TrySetEnterState(CSteamID id, bool state)
    {
        if (IsInside(id) == state) return false;

        if (state) enteredPlayers.Add(id);
        else enteredPlayers.Remove(id);

        return true;
    }
    protected bool SetEnterState(Vehicle target, bool state)
    {
        if (!target) return state;
        state = SetEnterState(target, state, OnVehicleEnter, OnVehicleExit);

        foreach (var passanger in target.passengers)
            SetEnterState(passanger.player, state);

        return state;
    }
    protected bool SetEnterState(Animal target, bool state) => SetEnterState(target, state, OnAnimalEnter, OnAnimalExit);
    protected bool SetEnterState(Zombie target, bool state) => SetEnterState(target, state, OnZombieEnter, OnZombieExit);

    protected bool SetEnterState<T>(T target, bool state, StateUpdateHandler<T> enter, StateUpdateHandler<T> exit)
        where T : UnityComponent
    {
        if (!target) return state;
        if (IsInside(target) == state) return state;

        InvokeEventsSafe(target, state, enter, exit);
        return state;
    }

    protected void InvokeEventsSafe<T>(T value, bool state, StateUpdateHandler<T> enter, StateUpdateHandler<T> exit)
    {
        try
        {
            (state ? enter : exit)?.Invoke(value);
        }
        catch { }
    }

    protected virtual float UpdateCollidersDelay { get; } = conf.DefaultUpdateDelay;
    protected virtual float LocateCollidersDelay { get; } = conf.DefaultLocateDelay;
    protected void UpdateStates(ICollection<Collider> colliders)
    {
        List<UnityComponent> other = new();
        for (int i = 0; i < colliders.Count; i++)
        {
            var collider = colliders.ElementAt(i);
            if (!collider)
            {
                colliders.Remove(collider);
                --i;
                continue;
            }

            var player = collider.GetComponent<Player>();
            if (player)
            {
                var vehicle = player.movement.getVehicle();
                if (vehicle)
                {
                    other.Add(vehicle);
                    continue;
                }
            }

            UpdateEnterState(collider);
        }
        foreach (var component in other.Distinct())
            UpdateEnterState(component);
    }
    protected void UpdateStates(ICollection<CSteamID> playersIds)
    {
        var ids = playersIds.ToList();
        foreach(var id in ids)
        {
            var player = PlayerTool.getPlayer(id);
            if (!player) TrySetEnterState(id, false);
            else UpdateEnterState(player);
        }
    }
    protected virtual void UpdateStates()
    {
        UpdateStates(enteredColliders);
        UpdateStates(enteredPlayers);
    }
    protected virtual IEnumerator UpdateEnteredCollidersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UpdateCollidersDelay);
            UpdateStates();
        }
    }
    protected virtual IEnumerator LocateEnteredCollidersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(LocateCollidersDelay);
            LocateEnteredColliders();
        }
    }
    protected virtual void LocateEnteredColliders()
    {
        if (Provider.clients is null) return;
        if (VehicleManager.vehicles is null) return;

        var colliders =
            Provider.clients.Select(x => x?.model?.GetComponent<Collider>())
            .Concat(VehicleManager.vehicles.Select(x => x?.GetComponent<Collider>()))
            .ToArray();
        foreach(var collider in colliders)
        {
            if (!collider) continue;
            UpdateEnterState(collider);
        }
    }
    #endregion

    #region Entities
    public IEnumerable<BarricadeDrop> BarricadeDrops => Zone is null ? 
        Enumerable.Empty<BarricadeDrop>() :
        Collider.bounds.GetRegions().GetBarricades(IsInside);
    public IEnumerable<Transform> Barricades => Zone is null ?
        Enumerable.Empty<Transform>() :
        BarricadeDrops.Select(x => x.model).ToList();
    public IEnumerable<TComponent> GetBarricades<TComponent>() where TComponent : UnityComponent =>
        Barricades.TryGetComponents<TComponent>();
    public IEnumerable<Zombie> Zombies => Zone is null ? 
        Enumerable.Empty<Zombie>() :
        GetZombies(Collider.bounds.center, IsInside);
    public IEnumerable<Vehicle> Vehicles => Zone is null ? 
        Enumerable.Empty<Vehicle>() :
        GetVehicles(IsInside);
    public IEnumerable<Animal> Animals => Zone is null ? 
        Enumerable.Empty<Animal>() :
        GetAnimals(IsInside);
    public IEnumerable<SPlayer> SPlayers => Zone is null ? 
        Enumerable.Empty<SPlayer>() :
        Provider.clients.Where(x => enteredPlayers.Contains(x.playerID.steamID)).ToList();
    public IEnumerable<Player> Players => SPlayers.Select(x => x.player);
    public IEnumerable<TComponent> GetPlayers<TComponent>() where TComponent : UnityComponent =>
        Players.TryGetComponents<TComponent>();
    #endregion

    #region Other
    public virtual bool IsInside(Vector3 position) => Collider.bounds.Contains(position);
    public virtual bool IsInside(Collider collider) => enteredColliders.Contains(collider);
    public virtual bool IsInside(GameObject @object) => EnteredObjects.Contains(@object);
    public virtual bool IsInside(UnityComponent component) => IsInside(component.gameObject);
    public virtual bool TryCheck(UnityComponent component)
    {
        foreach (var innerComponent in component.GetComponents<UnityComponent>())
            if (!Check(innerComponent))
                return false;
        return true;
    }
    public virtual bool Check(object @object) => @object switch
    {
        Player t => Check(t),
        Vehicle t => Check(t),
        Animal t => Check(t),
        Zombie t => Check(t),
        _ => true
    };
    public virtual bool Check(Player player) => !player?.life.isDead ?? false;
    public virtual bool Check(Vehicle vehicle) => !vehicle?.isDead ?? false;
    public virtual bool Check(Animal animal) => !animal?.isDead ?? false;
    public virtual bool Check(Zombie zombie) => !zombie?.isDead ?? false;
    public virtual bool IsInside(Player target) => IsInside(target.channel.owner.playerID.steamID);
    public virtual bool IsInside(CSteamID steamId) => enteredPlayers.Contains(steamId);
    public virtual bool IsPositionInside(Collider collider) => IsInside(collider.ClosestPointOnBounds(Zone.Position));
    public virtual bool IsPositionInside(Transform transform) => IsInside(transform.position);
    public virtual bool IsPositionInside(GameObject @object) => IsPositionInside(@object.transform);
    public virtual bool IsPositionInside(UnityComponent component) => IsPositionInside(component.transform);
    #endregion

    #region Lifetime
    protected virtual void OnDestroy() => Dispose();
    public virtual void Dispose()
    {
        StopAllCoroutines();
    }
    protected virtual void Awake()
    {
        StartCoroutine(UpdateEnteredCollidersRoutine());
        StartCoroutine(LocateEnteredCollidersRoutine());
    }
    #endregion
}
public abstract class ZoneController<TCollider> : ZoneController
    where TCollider : Collider
{
    public virtual new TCollider Collider
    {
        get
        {
            var collider = gameObject.GetOrAddComponent<TCollider>();
            base.Collider = collider;
            return collider;
        }
    }

    protected override void Awake()
    {
        foreach (var collider in GetComponents<Collider>()) // remove previous colliders
            Destroy(collider);
        Collider.isTrigger = true;
        Collider.material = Collider.sharedMaterial = null;
        gameObject.layer = LayerMasks.CLIP;
        base.Awake();
    }
}
