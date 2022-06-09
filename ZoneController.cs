namespace SZones;

public abstract class ZoneController : MonoBehaviour
{
    public event Action<GameObject> OnEnter, OnExit;
    public event Action<Player> OnPlayerEnter, OnPlayerExit;

    public Zone Zone { get; private set; }
    internal void Initialize(Zone zone)
    {
        Zone = zone;
    }

    private void SetEnterState(Collider other, bool state)
    {
        var @object = other?.gameObject;
        if (state) OnEnter?.Invoke(@object);
        else OnExit?.Invoke(@object);
        var player = @object.GetComponent<Player>();
        SetEnterState(player, state);
    }
    protected void OnTriggerEnter(Collider other) => SetEnterState(other, true);
    protected void OnTriggerExit(Collider other) => SetEnterState(other, false);

    private readonly List<CSteamID> enteredPlayers = new();
    public IReadOnlyCollection<CSteamID> EnteredPlayers => enteredPlayers;
    protected void SetEnterState(Player player, bool state)
    {
        if (!player)
            return;
        var id = player.channel.owner.playerID.steamID;
        var entered = enteredPlayers.Contains(id);
        if (entered != state)
        {
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
    }
}
public class ZoneController<TCollider> : ZoneController
    where TCollider : Collider
{
    public TCollider Collider => gameObject.GetOrAddComponent<TCollider>();

    protected virtual void Awake()
    {
        foreach(var collider in GetComponents<Collider>()) // remove previous colliders
            Destroy(collider); 
        Collider.isTrigger = true; // initialize new collider
    }
    protected virtual void OnDestroy()
    {

    }
}
