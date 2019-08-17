using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct LbDirection : IComponentData
{
    public byte Value;

    public static  float3 GetDirection(byte directionByte)
    {
        if (directionByte == 0x0)
        {
            return new float3(0.0f, 0.0f, 1.0f);
        }
        else if (directionByte == 0x1)
        {
            return new float3(1.0f, 0.0f, 0.0f);
        }
        else if (directionByte == 0x2)
        {
            return new float3(0.0f, 0.0f, -1.0f);
        }
        else
        {
            return new float3(-1.0f, 0.0f, 0.0f);
        }
    }

    public static int GetByteShift(byte direction)
    {
        switch (direction)
        {
            // North
            case 0x0:
                return 6;

            // South
            case 0x2:
                return 2;

            // West
            case 0x3:
                return 0;

            // East
            case 0x1:
                return 4;

            default:
                return 0;
        }
    }
}