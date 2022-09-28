namespace SZones;

/// <summary>
/// BoxCollider with boundaries of maximum and minimum nodes.<br/> Starts calculating position of inner collider if it was inside.
/// </summary>
public class CustomZoneController : ZoneController<BoxCollider>
{
    private void Start()
    {
        StartCoroutine(FindEnteredColliders());
    }

    public void SetNodes(IEnumerable<Vector3> nodes)
    {
        this.nodes.Clear();
        this.nodes.AddRange(nodes);
        Limits = new(nodes);
        Collider.center = Limits.GetVector(x => x.Center);
        Collider.size = Limits.GetVector(x => x.Delta);
    }

    private Vector3Limit Limits;

    protected readonly List<Vector3> nodes = new();
    public IReadOnlyCollection<Vector3> Nodes => nodes;

    private new readonly List<Collider> enteredColliders = new();

    protected override void SetEnterState(Collider other, bool state)
    {
        if (state != enteredColliders.Contains(other))
        {
            if (state) enteredColliders.Add(other);
            else enteredColliders.Remove(other);
        }
        UpdateEnterState(other);
    }

    protected virtual bool UpdateEnterState(Collider other)
    {
        if (other is null) return false;
        var state = IsInside(other.ClosestPointOnBounds(Collider.center));
        base.SetEnterState(other, state);
        return state;
    }

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

    private IEnumerator FindEnteredColliders()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            for (int i = 0; i < enteredColliders.Count;)
            {
                var collider = enteredColliders.ElementAtOrDefault(i);
                if (collider is null)
                {
                    enteredColliders.Remove(collider);
                    i--;
                    continue;
                }
                UpdateEnterState(collider);
                i++;
            }
        }
    }
}