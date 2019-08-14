using System;
using Unity.Entities;

/// <summary>
/// Lifetime value, when 0 the entity is marked for destruction
/// </summary>
[Serializable]
public struct LbScore : IComponentData
{
    public static byte kLbScoreType_RemovePoint = 0x00;
    public static byte kLbScoreType_AddPoint = 0x01;
    
    /// <summary>
    /// Score type
    /// </summary>
    public byte Type;

    /// <summary>
    /// Identifier for the player that will add or reduce score points
    /// </summary>
    public byte PlayerId;
}
