using System;
using Unity.Entities;

/// <summary>
/// Speed value for Entities that can move
/// </summary>
[Serializable]
public struct LbMovementSpeed : IComponentData
{
    public float Value;
}
