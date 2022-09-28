namespace SZones;

[XmlType("Cuboid")]
public class CuboidZone : Zone<CuboidZoneController>
{
    public override string Name => base.Name;

    public static float SizeModifier = 1.4f;
    protected Vector3 size;
    [XmlElement]
    public virtual SVector3 Size
    {
        get => size;
        set
        {
            size = value;
            if (Controller is null) return;
            Controller.Collider.size = size * SizeModifier;
        }
    }
    public override SVector3 Position
    {
        get => base.Position;
        set
        {
            base.Position = value;
            if (Controller is null) return;
            Controller.Collider.center = Position;
        }
    }

    public CuboidZone() : base()
    {
    }
    internal override void Initialize()
    {
        base.Initialize();
        // update collider
        Size = size;
    }
}
