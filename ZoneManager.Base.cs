namespace SZones;

// this base handling shutdown and multiple instances of this component
partial class ZoneManager
{
    ZoneManager() { }
    public override void LoadPlugin()
    {
        AssertInstance(false);
        Instance = this;
        base.LoadPlugin();
        StartHandlingShutdown();
    }
    public override void UnloadPlugin(PluginState state = PluginState.Unloaded)
    {
        base.UnloadPlugin(state);
        Instance = null;
    }
    static bool InstanceExists() => Instance is not null;
    static void AssertInstance(bool exists)
    {
        if (InstanceExists() != exists)
            throw new Exception($"{typeof(ZoneManager).FullName} instance already exists, use {nameof(ZoneManager)}.{nameof(Instance)} to access it's instance.");
    }
    public static ZoneManager Instance { get; private set; }
    void StopHandlingShutdown()
    {
        Application.quitting -= OnShutdown;
        Provider.onCommenceShutdown -= OnShutdown;
    }
    void StartHandlingShutdown()
    {
        StopHandlingShutdown();
        Application.quitting += OnShutdown;
        Provider.onCommenceShutdown += OnShutdown;
    }
    void OnShutdown()
    {
        if (Instance is null)
            return;
        StopHandlingShutdown();
        UnloadPlugin();
    }
    void OnApplicationQuit() => OnShutdown();
    void OnDestroy() => OnShutdown();
}