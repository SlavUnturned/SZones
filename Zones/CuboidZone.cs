namespace SZones;

[XmlType("Cuboid")]
public class CuboidZone : BoxColliderZone<CuboidZoneController>
{
    public static float SizeModifier = 1.4f;

    protected Vector3 size;
    [XmlElement]
    public virtual SVector3 Size
    {
        get => size;
        set
        {
            size = value;
            if (!controller) return;
            Controller.Collider.size = size * SizeModifier;
        }
    }

    internal override void Initialize()
    {
        base.Initialize();
        
        // update collider

        Size = size;
    }
}
