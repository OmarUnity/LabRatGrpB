using Unity.Entities;
using System;

/// <summary>
/// Player Component
/// </summary>
[Serializable]
public struct LbPlayer : IComponentData
{
    public const byte kPlayer1 = 0x00;
    public const byte kPlayer2 = 0x01;
    public const byte kPlayer3 = 0x10;
    public const byte kPlayer4 = 0x11;

    public byte Value;
}

