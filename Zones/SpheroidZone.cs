namespace SZones;

[XmlType("Spheroid")]
public class SpheroidZone : Zone<SpheroidZoneController>
{
    public static float RadiusModifier = 1f;

    protected float radius;
    [XmlAttribute]
    public virtual float Radius
    {
        get => radius;
        set
        {
            radius = value;
            if (!controller) return;
            Controller.Collider.radius = radius * RadiusModifier;
        }
    }

    internal override void Initialize()
    {
        base.Initialize();

        // update collider

        Radius = radius;
    }
}
