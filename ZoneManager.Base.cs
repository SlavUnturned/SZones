namespace SZones;

// this base handling shutdown and multiple instances of this component
partial class ZoneManager
{
    private ZoneManager() { }
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

    private static bool InstanceExists() => Instance is not null;

    private static void AssertInstance(bool exists)
    {
        if (InstanceExists() != exists)
            throw new Exception($"{typeof(ZoneManager).FullName} instance already {(exists ? "not exists" : $"exists, use {nameof(ZoneManager)}.{nameof(Instance)} to access it's instance")}.");
    }
    public static ZoneManager Instance { get; private set; }

    private void StopHandlingShutdown()
    {
        Application.quitting -= OnShutdown;
        Provider.onCommenceShutdown -= OnShutdown;
    }

    private void StartHandlingShutdown()
    {
        StopHandlingShutdown();
        Application.quitting += OnShutdown;
        Provider.onCommenceShutdown += OnShutdown;
    }

    private void OnShutdown()
    {
        if (Instance is null)
            return;
        StopHandlingShutdown();
        UnloadPlugin();
    }

    private void OnApplicationQuit() => OnShutdown();
    private void OnDestroy() => OnShutdown();
}