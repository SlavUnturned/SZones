namespace SZones;

public abstract class BoxColliderZone<TController> : Zone<TController>
    where TController : ZoneController<BoxCollider>
{
    public override SVector3 Position
    {
        get => Controller?.Collider.center ?? base.Position;
        set
        {
            base.Position = value;
            if (Controller is null) return;
            Controller.Collider.center = Position;
        }
    }
}