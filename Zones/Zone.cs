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
        if (Object) return;
        UnityObject.DontDestroyOnLoad(Object = UnityObject.Instantiate(Prefab));
        foreach(var body in Object.GetComponents<Rigidbody>())
            UnityObject.Destroy(body);
        Position = position; // update position
    }
    internal virtual void Dispose()
    {
        if (Controller) UnityObject.Destroy(Controller);
        if (Object) UnityObject.Destroy(Object);
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
        Controller.OnPlayerEnter += Debug_PlayerEnterHandler;
        Controller.OnPlayerExit += Debug_PlayerExitHandler;
    }
    internal override void Dispose()
    {
        Controller.OnPlayerEnter -= Debug_PlayerEnterHandler;
        Controller.OnPlayerExit -= Debug_PlayerExitHandler;
        base.Dispose();
    }
    private void Debug_PlayerEnterHandler(Player player)
    {
        if (!conf.DebugInformation) return;
        player.ReceiveMessage($"Enter {Name}");
    }
    private void Debug_PlayerExitHandler(Player player)
    {
        if (!conf.DebugInformation) return;
        player.ReceiveMessage($"Exit {Name}");
    }
}
