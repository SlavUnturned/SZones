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
            if (!Object) return;
            Object.transform.position = value;
        }
    }
    public virtual bool ShouldSerializePosition() => true;

    protected ZoneController controller;
    [XmlIgnore, JsonIgnore]
    public virtual ZoneController Controller
    {
        get
        {
            TryReinitialize();
            return controller;
        }
        protected set => controller = value;
    }

    protected GameObject Object;
    private static GameObject _prefab;
    protected static GameObject Prefab => _prefab ??= (Assets.find(EAssetType.ITEM, 325) as ItemBarricadeAsset).barricade; // spawn locker serverside with collision. original idea: Greenorine

    internal virtual void Initialize()
    {
        if (Object) return;
        UnityObject.DontDestroyOnLoad(Object = UnityObject.Instantiate(Prefab));

        foreach(var body in Object.GetComponents<Rigidbody>())
            UnityObject.Destroy(body);

        InitializeController();

        Position = position; // update position
    }

    internal virtual void InitializeController() { }

    internal virtual void Finalize()
    {
        FinalizeController();
        UnityObject.Destroy(Object);
    }

    internal virtual void FinalizeController()
    {
        UnityObject.Destroy(controller);
    }

    public virtual void Reinitialize()
    {
        Finalize();
        Initialize();
    }

    public virtual void ReinitializeController()
    {
        FinalizeController();
        InitializeController();
    }

    public void TryReinitialize()
    {
        if (!Object) Reinitialize();
        else if (!controller) ReinitializeController();
    }

    public virtual bool IsValid() => Object && controller;

    public override string ToString() => ToJsonString(this, true, new ValueTypeToStringJsonConverter());
}
public abstract class Zone<TController> : Zone
    where TController : ZoneController
{
    [XmlIgnore, JsonIgnore]
    public virtual new TController Controller 
    { 
        get => (TController)base.Controller;
        private set => base.Controller = value; 
    }

    internal override void InitializeController()
    {
        base.InitializeController();
        if (controller) return;
        (controller = Object.GetOrAddComponent<TController>()).Initialize(this);
        controller.OnPlayerEnter += Debug_PlayerEnterHandler;
        controller.OnPlayerExit += Debug_PlayerExitHandler;
    }

    internal override void FinalizeController()
    {
        if (!controller) return;
        controller.OnPlayerEnter -= Debug_PlayerEnterHandler;
        controller.OnPlayerExit -= Debug_PlayerExitHandler;
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
