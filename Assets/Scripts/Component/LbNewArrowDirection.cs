using System;
using Unity.Entities;

/// <summary>
/// Directional arrow
/// </summary>
[Serializable]
public class LbNewArrowDirection : IComponentData
{
    public const short kCustomDirectionFlag = 0x10;

    public byte Direciton;
    public byte Player;
}
