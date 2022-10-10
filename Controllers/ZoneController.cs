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
    protected readonly List<Collider> enteredColliders = new();
    public virtual IReadOnlyCollection<Collider> EnteredColliders => enteredColliders;
    public virtual IReadOnlyCollection<GameObject> EnteredObjects => enteredColliders.Select(x => x.gameObject).ToList();
    protected virtual bool SetEnterState(Collider other, bool state)
    {
        if (!other) return state;
        var @object = other.gameObject;

        var entered = enteredColliders.Contains(other);
        if (state == entered) return state;

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
    protected virtual void OnTriggerEnter(Collider other) => SetEnterState(other, true);
    protected virtual void OnTriggerExit(Collider other) => SetEnterState(other, false);

    protected readonly List<CSteamID> enteredPlayers = new();
    public virtual IReadOnlyCollection<CSteamID> EnteredPlayers => enteredPlayers;

    protected bool SetEnterState(Player target, bool state)
    {
        if (!target) return state;
        if (!SetEnterState(target.channel.owner.playerID.steamID, state)) return state;

        InvokeEventsSafe(target, state, OnPlayerEnter, OnPlayerExit);
        return state;
    }
    protected bool SetEnterState(CSteamID id, bool state)
    {
        if (IsInside(id) == state) return false;

        if (state) enteredPlayers.Add(id);
        else enteredPlayers.RemoveAll(x => x == id);

        return true;
    }
    protected bool SetEnterState(Vehicle target, bool state)
    {
        if (!target) return state;
        state = SetEnterState(target, state, OnVehicleEnter, OnVehicleExit);

        foreach (var passanger in target.passengers)
            if (passanger.player is { } splayer)
                SetEnterState(splayer.player, state);

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
        } catch { }
    }

    protected virtual float UpdateCollidersDelay { get; } = 0.2f;
    protected void UpdateEnteredColliders(IList<Collider> colliders)
    {
        for (int i = 0; i < colliders.Count;i++)
        {
            var collider = colliders.ElementAt(i);
            if (!collider)
            {
                colliders.RemoveAt(i--);
                continue;
            }
            UpdateEnterState(collider);
        }
    }
    protected virtual void UpdateEnteredColliders() => UpdateEnteredColliders(enteredColliders);
    protected virtual IEnumerator UpdateEnteredCollidersRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UpdateCollidersDelay);
            UpdateEnteredColliders();
        }
    }
    #endregion

    #region Entities
    public IEnumerable<Transform> Barricades => Zone is null ?
        Enumerable.Empty<Transform>() :
        Collider.bounds.GetRegions().GetBarricades(IsInside);
    public IEnumerable<BarricadeDrop> BarricadeDrops => Barricades.Select(BarricadeManager.FindBarricadeByRootTransform).Where(x => x is not null);
    public IEnumerable<TComponent> GetBarricades<TComponent>() where TComponent : UnityComponent =>
        BarricadeDrops.Select(x => x.model).TryGetComponents<TComponent>();
    public IEnumerable<Zombie> Zombies => Zone is null ? Enumerable.Empty<Zombie>() : GetZombies(Collider.bounds.center, IsInside);
    public IEnumerable<Vehicle> Vehicles => Zone is null ? Enumerable.Empty<Vehicle>() : GetVehicles(IsInside);
    public IEnumerable<Animal> Animals => Zone is null ? Enumerable.Empty<Animal>() : GetAnimals(IsInside);
    public IEnumerable<SPlayer> SPlayers => Zone is null ? Enumerable.Empty<SPlayer>() : Provider.clients.Where(x => enteredPlayers.Contains(x.playerID.steamID));
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
