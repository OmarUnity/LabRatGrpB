using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CollisionSystem : JobComponentSystem
{
    EntityCommandBufferSystem   m_Barrier;
    EntityQuery                 catsGroup;
    EntityQuery                 ratsGroup;

    protected override void OnCreate()
    {
        m_Barrier = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        catsGroup = GetEntityQuery( ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<LbCat>() );
        ratsGroup = GetEntityQuery( ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<LbRat>() );
    }

    struct CollisionJob : IJobChunk
    {
        public float radius;
        [ReadOnly] public ArchetypeChunkComponentType<Translation>  translationType;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation>                  entitiesToVerifyCollision;

        public EntityCommandBuffer.Concurrent                       commandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray( translationType );

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation catTranslation = chunkTranslations[ i ];

                for (int j = 0; j < entitiesToVerifyCollision.Length; j++)
                {
                    Translation ratTranslation = entitiesToVerifyCollision[i];

                    if ( IsColliding( catTranslation.Value, ratTranslation.Value, radius ) )
                    {
                        // commandBuffer.AddComponent(chunkIndex, ); =(
                    }
                }
            }
        }

        static bool IsColliding(float3 pos1, float3 pos2, float radiusSqr)
        {
            float3 delta        = pos1 - pos2;
            float distanceSqr   = delta.x * delta.x + delta.z * delta.z;

            return distanceSqr <= radiusSqr;
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var collisionJob = new CollisionJob
        {
            radius                      = 0.5f,
            translationType             = GetArchetypeChunkComponentType<Translation>(),
            entitiesToVerifyCollision   = ratsGroup.ToComponentDataArray<Translation>( Allocator.TempJob ),

            commandBuffer               = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule( catsGroup, inputDeps );

        m_Barrier.AddJobHandleForProducer( collisionJob );

        return collisionJob;
    }
}