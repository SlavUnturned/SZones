namespace SZones;

public abstract class BoxColliderZone<TController> : Zone<TController>
    where TController : ZoneController<BoxCollider>
{
    public override SVector3 Position
    {
        get
        {
            if(Controller is null) return base.Position;
            var center = Controller.Collider.center;
            center.y = Controller.Collider.bounds.min.y;
            return center;
        }
        set
        {
            base.Position = value;
            if (Controller is null) return;
            Controller.Collider.center = Position;
        }
    }
}