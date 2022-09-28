namespace SZones;

[XmlType("Spheroid")]
public class SpheroidZone : Zone<SpheroidZoneController>
{
    public override string Name => base.Name;

    public static float RadiusModifier = 1f;
    protected float radius;
    [XmlAttribute]
    public virtual float Radius
    {
        get => radius;
        set
        {
            radius = value;
            if (Controller is null) return;
            Controller.Collider.radius = radius * RadiusModifier;
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
