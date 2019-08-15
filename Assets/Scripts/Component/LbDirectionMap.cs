using System;
using Unity.Entities;

public struct LbDirectionMap : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator short(LbDirectionMap e) { return e.Value; }
    public static implicit operator LbDirectionMap(short e) { return new LbDirectionMap { Value = e }; }

   // Actual value each buffer element will store.
    
  //  North,   00   0,0,1
  //  East,    01   1,0,0
  //  South,   10   0,0,-1
  //  West     11   -1,0,0
    
   //            North South West East |  1 bit change flag | New Dir
  // NextDir //  [][]  [][]  [][] [][]          []            [][]
   public short Value;
}