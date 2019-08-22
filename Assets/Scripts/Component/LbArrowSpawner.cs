using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Tag to identify arrow spawners.
/// </summary>
public struct LbArrowSpawner : IComponentData
{
    Entity Prefab;
    byte PlayerId;
    byte Direction;
    float3 Location;
}
