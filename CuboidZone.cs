namespace SZones;

[XmlType("Cuboid")]
public class CuboidZone : Zone<CuboidZoneController>
{
    Vector3 size;
    [XmlElement]
    public Vector3 Size
    {
        get => size;
        set
        {
            size = value;
            if (Controller is { })
                Controller.Collider.size = size*1.4f;
        }
    }
    public override Vector3 Position
    {
        get => base.Position;
        set
        {
            base.Position = value;
            if (Controller is { })
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
