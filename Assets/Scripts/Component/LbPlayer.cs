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

    /// <summary>
    /// Convert Players enum to byte
    /// </summary>
    public static byte ConvertToByte(Players player)
    {
        byte playerId = 0;
        switch (player)
        {
            case Players.Player1:
                playerId = LbPlayer.kPlayer1;
                break;

            case Players.Player2:
                playerId = LbPlayer.kPlayer2;
                break;

            case Players.Player3:
                playerId = LbPlayer.kPlayer3;
                break;

            case Players.Player4:
                playerId = LbPlayer.kPlayer4;
                break;
        }

        return playerId;
    }
}

