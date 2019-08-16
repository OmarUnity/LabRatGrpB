using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class CatMapSystem : JobComponentSystem
{
    EntityQuery m_Query;
    EntityQuery m_CatQuery;

    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(typeof(LbBoard), typeof(LbCatMap));
        m_CatQuery = GetEntityQuery(typeof(LbCat), typeof(Translation));
    }

    struct CatMapJob : IJob
    {
        public int Size;
        public NativeArray<LbCatMap> Buffer;

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> Translations;

        public void Execute()
        {
            var bitsInWord = sizeof(int) * 8;

            for (int i = 0; i < Translations.Length; ++i)
            {
                var translation = Translations[i].Value;

                var bufferBitIndex = ((int)translation.z) * Size + (int)translation.x;
                var bufferWordIndex = bufferBitIndex / bitsInWord;
                var bitOffset = bufferBitIndex % bitsInWord;

                var currentWord = Buffer[bufferWordIndex].Value;

                var bit = 1 << bitOffset;
                currentWord |= bit;

                Buffer[bufferWordIndex] = new LbCatMap() { Value = currentWord };
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var boardEntity = m_Query.GetSingletonEntity();
        var board = m_Query.GetSingleton<LbBoard>();
        var bufferLookup = GetBufferFromEntity<LbCatMap>();

        var buffer = bufferLookup[boardEntity];
        var bufferArray = buffer.AsNativeArray();

        var cleanJobHandle = new MemsetNativeArray<LbCatMap>()
        {
            Source = bufferArray,
            Value = new LbCatMap()
        }.Schedule(bufferArray.Length, 32, inputDeps);

        var jobHandle = new CatMapJob
        {
            Size = board.SizeY,
            Buffer = bufferArray,
            Translations = m_CatQuery.ToComponentDataArray<Translation>(Allocator.TempJob)
        }.Schedule(cleanJobHandle);

        return jobHandle;
    }
}
