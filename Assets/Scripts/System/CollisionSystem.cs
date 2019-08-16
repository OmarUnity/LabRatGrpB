using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CollisionSystem : JobComponentSystem
{
    EntityCommandBufferSystem m_Barrier;
    EntityQuery m_BoardQuery;

    protected override void OnCreate()
    {
        m_Barrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_BoardQuery = GetEntityQuery(typeof(LbBoard), typeof(LbCatMap));
    }

    struct CollisionJob : IJobForEachWithEntity<LbRat, Translation>
    {
        public const int kBitsInWord = sizeof(int) * 8;

        public int Size;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        [ReadOnly] public NativeArray<LbCatMap> CatLocationBuffer;

        public void Execute(Entity entity, int jobIndex, [ReadOnly] ref LbRat rat, [ReadOnly] ref Translation translation)
        {
            var position = translation.Value;

            var bufferBitIndex = ((int)position.z) * Size + (int)position.x;
            var bufferWordIndex = bufferBitIndex / kBitsInWord;
            var bitOffset = bufferBitIndex % kBitsInWord;

            var currentWord = CatLocationBuffer[bufferWordIndex].Value;
            var bit = 1 << bitOffset;

            if ((currentWord & bit) == bit)
            {
                CommandBuffer.AddComponent(jobIndex, entity, new LbDestroy());
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var boardEntity = m_BoardQuery.GetSingletonEntity();
        var board = m_BoardQuery.GetSingleton<LbBoard>();
        var bufferLookup = GetBufferFromEntity<LbCatMap>();

        var collisionJob = new CollisionJob
        {
            Size = board.SizeY,
            CatLocationBuffer = bufferLookup[boardEntity].AsNativeArray(),
            CommandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(this, inputDeps);

        m_Barrier.AddJobHandleForProducer( collisionJob );

        return collisionJob;
    }
}