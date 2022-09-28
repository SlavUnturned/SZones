namespace SZones;

[XmlType("Custom")]
public class CustomZone : Zone<CustomZoneController>
{
    [XmlArrayItem("Node"), JsonIgnore]
    public List<SVector3> Nodes = new();
    public CustomZone() : base() { }

    public override bool ShouldSerializePosition() => false;

    internal override void Initialize()
    {
        base.Initialize();
        // update collider
        if (Controller is null) return;
        Controller.SetNodes(Nodes.Select(SVector3.Convert));
    }
}
