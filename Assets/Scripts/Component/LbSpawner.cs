using Unity.Entities;

public struct LbSpawner : IComponentData
{
    public Entity entityPrefab;
    public int maxAmount;
    public int frecuency;
}
