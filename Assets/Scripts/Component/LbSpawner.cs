using Unity.Entities;

public struct LbSpawner : IComponentData
{
    public Entity Prefab;
    public int MaxAmount;
    public float Frequency;
}
