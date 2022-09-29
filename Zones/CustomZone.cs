namespace SZones;

[XmlType("Custom")]
public class CustomZone : BoxColliderZone<CustomZoneController>
{
    [XmlArrayItem("Node"), JsonIgnore]
    public List<SVector3> Nodes = new();

    public override bool ShouldSerializePosition() => false;

    internal override void Initialize()
    {
        base.Initialize();

        // update collider

        Controller.SetNodes(Nodes.Select(SVector3.Convert));
    }
}
