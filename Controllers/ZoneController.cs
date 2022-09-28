namespace SZones;

public abstract class ZoneController : UnityBehaviour
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

    public abstract bool IsInside(Vector3 position);
    public virtual bool IsInside(Collider collider) => enteredColliders.Contains(collider);
    public virtual bool IsInside(Player player) => IsInside(player.channel.owner.playerID.steamID);
    public virtual bool IsInside(CSteamID steamId) => enteredPlayers.Contains(steamId);

    public IEnumerable<Transform> Barricades => Zone is null ?
        Enumerable.Empty<Transform>() :
        Collider.bounds.GetRegions().GetBarricades(IsInside);
    public IEnumerable<T> GetBarricades<T>() where T : UnityComponent => Barricades.Select(x => x.GetComponent<T>()).Where(x => x is not null);
    public IEnumerable<BarricadeDrop> BarricadeDrops => Barricades.Select(BarricadeManager.FindBarricadeByRootTransform).Where(x => x is not null);
    public IEnumerable<Zombie> Zombies => Zone is null ? Enumerable.Empty<Zombie>() : GetZombies(Collider.bounds.center, IsInside);
    public IEnumerable<Vehicle> Vehicles => Zone is null ? Enumerable.Empty<Vehicle>() : GetVehicles(IsInside);
    public IEnumerable<Animal> Animals => Zone is null ? Enumerable.Empty<Animal>() : GetAnimals(IsInside);


    protected readonly List<Collider> enteredColliders = new();
    public virtual IReadOnlyCollection<Collider> EnteredColliders => enteredColliders;
    protected virtual void SetEnterState(Collider other, bool state)
    {
        var entered = enteredColliders.Contains(other);
        if (state == entered) return;
        var @object = other?.gameObject;
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
        SetEnterState(@object?.GetComponent<Player>(), state);
        SetEnterState(@object?.GetComponent<Vehicle>(), state);
        SetEnterState(@object?.GetComponent<Animal>(), state);
        SetEnterState(@object?.GetComponent<Zombie>(), state);
    }
    protected virtual void OnTriggerEnter(Collider other) => SetEnterState(other, true);
    protected virtual void OnTriggerExit(Collider other) => SetEnterState(other, false);

    protected readonly List<CSteamID> enteredPlayers = new();
    public virtual IReadOnlyCollection<CSteamID> EnteredPlayers => enteredPlayers;
    protected void SetEnterState(Player player, bool state)
    {
        if (!player) return;
        var id = player.channel.owner.playerID.steamID;
        var entered = enteredPlayers.Contains(id);
        if (entered == state) return;
        if (state)
        {
            enteredPlayers.Add(id);
            OnPlayerEnter?.Invoke(player);
        }
        else
        {
            enteredPlayers.RemoveAll(x => x == id);
            OnPlayerExit?.Invoke(player);
        }
    }
    protected void SetEnterState(Vehicle vehicle, bool state)
    {
        if (!vehicle) return;
        if (state) OnVehicleEnter?.Invoke(vehicle);
        else OnVehicleExit?.Invoke(vehicle);
    }
    protected void SetEnterState(Animal animal, bool state)
    {
        if (!animal) return;
        if (state) OnAnimalEnter?.Invoke(animal);
        else OnAnimalExit?.Invoke(animal);
    }
    protected void SetEnterState(Zombie zombie, bool state)
    {
        if (!zombie) return;
        if (state) OnZombieEnter?.Invoke(zombie);
        else OnZombieExit?.Invoke(zombie);
    }
}
public class ZoneController<TCollider> : ZoneController
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

    public override bool IsInside(Vector3 position) => Collider.bounds.Contains(position);

    protected virtual void Awake()
    {
        foreach (var collider in GetComponents<Collider>()) // remove previous colliders
            Destroy(collider);
        Collider.isTrigger = true; // initialize new collider
    }
    protected virtual void OnDestroy()
    {

    }
}
