namespace SZones;

[XmlType("Spheroid")]
public class SpheroidZone : Zone<SpheroidZoneController>
{
    public override string Name => base.Name;

    protected float radius;
    [XmlAttribute]
    public virtual float Radius
    {
        get => radius;
        set
        {
            radius = value;
            if (Controller is { })
                Controller.Collider.radius = radius;
        }
    }
    public SpheroidZone() : base()
    {
    }
    internal override void Initialize()
    {
        base.Initialize();
        // update collider
        Radius = radius;
    }
}
