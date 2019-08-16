using System;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Target Position
/// </summary>
[Serializable]
public struct LbArrowPosition : IComponentData
{
    /// <summary>
    /// Player's position
    /// </summary>
    public float3 Value;
}
