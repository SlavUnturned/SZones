namespace SZones;

internal struct VectorLimit
{
    public VectorLimit(float min, float max)
    {
        Min = min; Max = max;
    }
    public float Min, Max;
    public float Center => Min+(Delta / 2);
    public float Delta => Max - Min;
    public bool IsInRange(float value) => value >= Min && value <= Max;
    public static VectorLimit Get(IEnumerable<Vector3> nodes, Func<Vector3, float> vectorGetter)
    {
        float min = 0f, max = 0f;
        try
        {
            min = nodes.Min(vectorGetter);
        }
        catch { }
        try
        {
            max = nodes.Max(vectorGetter);
        }
        catch { }
        return new(min, max);
    }
}
