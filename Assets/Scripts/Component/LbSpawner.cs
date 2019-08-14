using Unity.Entities;

/// <summary>
/// Spawner entity
/// </summary>
public struct LbSpawner : IComponentData
{
    
    /// <summary>
    /// Prefab entity to instantiate
    /// </summary>
    public Entity Prefab;
    
    /// <summary>
    /// Maximum number of instances to create
    /// </summary>
    public int MaxAmount;
    
    /// <summary>
    /// How often an instance is made
    /// </summary>
    public float Frequency;
}
