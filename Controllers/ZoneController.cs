namespace SZones;

public abstract partial class ZoneController : UnityBehaviour
{
    public event Action<GameObject> OnEnter, OnExit;
    public event Action<Player> OnPlayerEnter, OnPlayerExit;
    public event Action<Zombie> OnZombieEnter, OnZombieExit;
    public event Action<Animal> OnAnimalEnter, OnAnimalExit;
    public event Action<Vehicle> OnVehicleEnter, OnVehicleExit;

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
    protected virtual void SetEnterState(Collider other, bool state)
    {
        if (!other) return;
        var entered = enteredColliders.Contains(other);
        if (state == entered) return;

        var @object = other.gameObject;
        SetEnterState(@object?.GetComponent<Player>(), state);
        SetEnterState(@object?.GetComponent<Vehicle>(), state);
        SetEnterState(@object?.GetComponent<Animal>(), state);
        SetEnterState(@object?.GetComponent<Zombie>(), state);
        if (state)
        {
            enteredColliders.Add(other);
            OnEnter?.Invoke(@object);
        }
        else
        {
            enteredColliders.Remove(other);
            OnExit?.Invoke(@object);
        }
    }
    protected virtual bool UpdateEnterState(Collider other)
    {
        if (!other) return false;
        var state = IsPositionInside(other);
        SetEnterState(other, state);
        return state;
    }
    protected virtual void OnTriggerEnter(Collider other) => SetEnterState(other, true);
    protected virtual void OnTriggerExit(Collider other) => SetEnterState(other, false);

    protected readonly List<CSteamID> enteredPlayers = new();
    public virtual IReadOnlyCollection<CSteamID> EnteredPlayers => enteredPlayers;

    protected void SetEnterState(Player player, bool state)
    {
        if (!player) return;
        if (!SetEnterState(player.channel.owner.playerID.steamID, state)) return;

        if (state) OnPlayerEnter?.Invoke(player);
        else OnPlayerExit?.Invoke(player);
    }
    protected bool SetEnterState(CSteamID id, bool state)
    {
        if (IsInside(id) == state) return false;

        if (state) enteredPlayers.Add(id);
        else enteredPlayers.RemoveAll(x => x == id);

        return true;
    }
    private void SetEnterState(Vehicle vehicle, bool state)
    {
        if (!vehicle) return;
        if (IsInside(vehicle) == state) return;

        if (state) OnVehicleEnter?.Invoke(vehicle);
        else OnVehicleExit?.Invoke(vehicle);

        foreach (var passanger in vehicle.passengers)
            if (passanger.player is { } splayer)
                SetEnterState(splayer.player, state);
    }
    private void SetEnterState(Animal animal, bool state)
    {
        if (!animal) return;
        if (IsInside(animal) == state) return;

        if (state) OnAnimalEnter?.Invoke(animal);
        else OnAnimalExit?.Invoke(animal);
    }
    private void SetEnterState(Zombie zombie, bool state)
    {
        if (!zombie) return;
        if (IsInside(zombie) == state) return;

        if (state) OnZombieEnter?.Invoke(zombie);
        else OnZombieExit?.Invoke(zombie);
    }

    protected virtual float UpdateCollidersDelay { get; } = 0.2f;
    protected void UpdateEnteredColliders(IList<Collider> colliders)
    {
        for (int i = 0; i < colliders.Count;)
        {
            var collider = colliders.ElementAtOrDefault(i);
            if (!collider)
            {
                colliders.Remove(collider);
                i--;
                continue;
            }
            UpdateEnterState(collider);
            i++;
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
    public IEnumerable<TComponent> GetBarricades<TComponent>() where TComponent : UnityComponent =>
        Barricades.Select(x => x.GetComponent<TComponent>()).Where(x => x is not null);
    public IEnumerable<BarricadeDrop> BarricadeDrops => Barricades.Select(BarricadeManager.FindBarricadeByRootTransform).Where(x => x is not null);
    public IEnumerable<Zombie> Zombies => Zone is null ? Enumerable.Empty<Zombie>() : GetZombies(Collider.bounds.center, IsInside);
    public IEnumerable<Vehicle> Vehicles => Zone is null ? Enumerable.Empty<Vehicle>() : GetVehicles(IsInside);
    public IEnumerable<Animal> Animals => Zone is null ? Enumerable.Empty<Animal>() : GetAnimals(IsInside);
    public IEnumerable<SPlayer> SPlayers => Zone is null ? Enumerable.Empty<SPlayer>() : Provider.clients.Where(x => enteredPlayers.Contains(x.playerID.steamID));
    public IEnumerable<Player> Players => SPlayers.Select(x => x.player);
    public IEnumerable<TComponent> GetPlayers<TComponent>() where TComponent : UnityComponent =>
        Players.Select(x => x.GetComponent<TComponent>()).Where(x => x is not null);
    #endregion

    #region Other
    public virtual bool IsInside(Vector3 position) => Collider.bounds.Contains(position);
    public virtual bool IsInside(Collider collider) => enteredColliders.Contains(collider);
    public virtual bool IsInside(GameObject @object) => EnteredObjects.Contains(@object);
    public virtual bool IsInside(Transform transform) => IsInside(transform.gameObject);
    public virtual bool IsInside(UnityComponent component) => IsInside(component.gameObject);
    public virtual bool IsInside(Player player) => IsInside(player.channel.owner.playerID.steamID);
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
        base.Awake();
    }
}
