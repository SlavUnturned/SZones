namespace SZones;

/// <summary>
/// BoxCollider with boundaries of maximum and minimum nodes.<br/> Starts calculating position of inner collider if it was inside.
/// </summary>
public class CustomZoneController : ZoneController<BoxCollider>
{
    public void SetNodes(IEnumerable<Vector3> nodes)
    {
        this.nodes.Clear();
        this.nodes.AddRange(nodes);
        Limits = new(this.nodes);
        Collider.center = Limits.GetVector(x => x.Center);
        Collider.size = Limits.GetVector(x => x.Delta);
    }

    private Vector3Limit Limits;

    protected readonly List<Vector3> nodes = new();
    public IReadOnlyCollection<Vector3> Nodes => nodes;

    private readonly List<Collider> colliders = new();

    protected new bool SetEnterState(Collider other, bool state)
    {
        if (state != colliders.Contains(other))
        {
            if (state) colliders.Add(other);
            else colliders.Remove(other);
        }
        state = IsPositionInside(other);
        return base.SetEnterState(other, state);
    }
    protected override void OnTriggerEnter(Collider other) => SetEnterState(other, true);
    protected override void OnTriggerExit(Collider other) => SetEnterState(other, false);

    public override bool IsInside(Vector3 point)
    {
        // pnpoly, algorithm to find out is point lies within a polygon (https://wrfranklin.org/Research/Short_Notes/pnpoly.html)
        var inside = false;
        for (int i = 0, j = nodes.Count - 1; i < nodes.Count; j = i++)
        {
            var inode = nodes[i];
            var jnode = nodes[j];
            if (inode.z > point.z != jnode.z > point.z &&
                 point.x < (jnode.x - inode.x) * (point.z - inode.z) / (jnode.z - inode.z) + inode.x)
                inside = !inside;
        }
        return inside && Limits.Y.IsInRange(point.y);
    }

    protected override void UpdateEnteredColliders() => UpdateEnteredColliders(colliders);
}