using System;
using Unity.Entities;

[InternalBufferCapacity(100)]
public struct LbDirectionMap : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator short(LbDirectionMap e) { return e.Value; }
    public static implicit operator LbDirectionMap(short e) { return new LbDirectionMap { Value = e }; }

    // Actual value each buffer element will store.
    public short Value;
}