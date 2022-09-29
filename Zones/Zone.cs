namespace SZones;

[XmlInclude(typeof(SpheroidZone))]
[XmlInclude(typeof(CuboidZone))]
[XmlInclude(typeof(CustomZone))]
public abstract partial class Zone
{
    [XmlAttribute]
    public virtual string Name { get; set; }

    protected Vector3 position;
    [XmlElement]
    public virtual SVector3 Position
    {
        get => Object?.transform.position ?? position;
        set
        {
            position = value;
            if (Object is null) return;
            Object.transform.position = value;
        }
    }
    public virtual bool ShouldSerializePosition() => true;    

    [XmlIgnore, JsonIgnore]
    public virtual ZoneController Controller { get; protected set; }

    protected GameObject Object;
    private static GameObject _prefab;
    protected static GameObject Prefab => _prefab ??= (Assets.find(EAssetType.ITEM, 325) as ItemBarricadeAsset).barricade; // spawn locker serverside with collision. original idea: Greenorine

    internal virtual void Initialize()
    {
        if (Object is not null) return;
        UnityObject.DontDestroyOnLoad(Object = UnityObject.Instantiate(Prefab));
        UnityObject.Destroy(Object.GetComponent<Rigidbody>());
        Position = position; // update position
    }
    internal virtual void Dispose()
    {
        UnityObject.Destroy(Object);
        Controller.Dispose();
    }

    public override string ToString() => ToJsonString(this, true, new ValueTypeToStringJsonConverter());
}
public abstract class Zone<TController> : Zone
    where TController : ZoneController
{
    [XmlIgnore, JsonIgnore]
    public virtual new TController Controller { get => (TController)base.Controller; private set => base.Controller = value; }

    internal override void Initialize()
    {
        base.Initialize();
        (Controller = Object.GetOrAddComponent<TController>()).Initialize(this);
#if DEBUG // information for debug
        Controller.OnPlayerEnter += PlayerEnterHandler;
        Controller.OnPlayerExit += PlayerExitHandler;
#endif
    }
    internal override void Dispose()
    {
#if DEBUG
        Controller.OnPlayerEnter -= PlayerEnterHandler;
        Controller.OnPlayerExit -= PlayerExitHandler;
#endif
        base.Dispose();
    }
    private void PlayerEnterHandler(Player player)
    {
        player.ReceiveMessage($"Enter {Name}");
    }
    private void PlayerExitHandler(Player player)
    {
        player.ReceiveMessage($"Exit {Name}");
    }
}
