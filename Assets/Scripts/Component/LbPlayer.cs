using Unity.Entities;
using System;

/// <summary>
/// Player Component
/// </summary>
[Serializable]
public struct LbPlayer : IComponentData
{
    // public const short kPlayer1Flag = 0x000;
    public const short kPlayer2Flag = 0x200;
    public const short kPlayer3Flag = 0x400;
    public const short kPlayer4Flag = 0x600;

    public byte Value;
}

