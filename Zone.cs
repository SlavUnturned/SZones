namespace SZones;

[XmlInclude(typeof(SpheroidZone))]
[XmlInclude(typeof(CuboidZone))]
public abstract class Zone
{
    [XmlAttribute]
    public virtual string Name { get; set; }

    protected Vector3 position;
    [XmlElement]
    public virtual Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            if (Object is { })
                Object.transform.position = value;
        }
    }

    [XmlIgnore]
    public virtual ZoneController Controller { get; protected set; }

    [XmlIgnore]
    protected GameObject Object;
    private static GameObject _prefab;
    protected static GameObject Prefab => _prefab ??= (Assets.find(EAssetType.ITEM, 325) as ItemBarricadeAsset).barricade; // spawn locker serverside with collision. original idea: Greenorine
    public Zone() { }
    internal virtual void Initialize()
    {
        if (Object is { })
            return;
        UnityObject.DontDestroyOnLoad(Object = UnityObject.Instantiate(Prefab));
        UnityObject.Destroy(Object.GetComponent<Rigidbody>());
        Position = position;
    }
    internal virtual void Dispose()
    {
        UnityObject.Destroy(Object);
    }
}
public abstract class Zone<TController> : Zone
    where TController : ZoneController
{
    [XmlIgnore]
    public virtual new TController Controller { get => (TController)base.Controller; private set => base.Controller = value; }

    public Zone() { }
    internal override void Initialize()
    {
        base.Initialize();
        (Controller = Object.GetOrAddComponent<TController>()).Initialize(this);
    }
}
