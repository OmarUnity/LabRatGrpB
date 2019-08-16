using System;
using Unity.Entities;

public struct LbArrowMap : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator byte(LbArrowMap e) { return e.Value; }
    public static implicit operator LbArrowMap(byte e) { return new LbArrowMap { Value = e }; }

    // Actual value each buffer element will store.

    //  North,   00   0,0,1
    //  East,    01   1,0,0
    //  South,   10   0,0,-1
    //  West     11   -1,0,0

    //            North South West East |  1 bit change flag | New Dir
    // NextDir //  [][]  [][]  [][] [][]          []            [][]
    public byte Value;
}
