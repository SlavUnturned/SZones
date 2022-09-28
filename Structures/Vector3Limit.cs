namespace SZones;

internal readonly struct Vector3Limit
{
    public Vector3Limit(IEnumerable<Vector3> nodes)
    {
        X = VectorLimit.Get(nodes, x => x.x);
        Y = VectorLimit.Get(nodes, x => x.y);
        Z = VectorLimit.Get(nodes, x => x.z);
    }
    public readonly VectorLimit X, Y, Z;
    public Vector3 GetVector(Func<VectorLimit, float> vectorGetter) => new(vectorGetter(X), vectorGetter(Y), vectorGetter(Z));
}