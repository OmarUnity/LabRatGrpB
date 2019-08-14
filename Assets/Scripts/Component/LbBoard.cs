using System;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Board entity
/// </summary>
[Serializable]
public struct LbBoard : IComponentData
{
    /// <summary>
    /// Board size
    /// </summary>
    public int2 Size;
}
