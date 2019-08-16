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
        [ReadOnly] public ArchetypeChunkEntityType                  entityCatType;

        public EntityCommandBuffer.Concurrent                       commandBuffer;

        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation>                  entitiesToVerifyCollision;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Entity>                       entityRats;

        // chunkIndex == Job id
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations   = chunk.GetNativeArray( translationType );
            var chunkEntities       = chunk.GetNativeArray( entityCatType );

            for (int i = 0; i < chunk.Count; i++)
            {
                Translation catTranslation  = chunkTranslations[ i ];
                //Entity catEntity            = chunkEntities[ i ];

                for (int j = 0; j < entitiesToVerifyCollision.Length; j++)
                {
                    Translation ratTranslation  = entitiesToVerifyCollision[i];
                    Entity      ratEntity       = entityRats[i];

                    if ( IsColliding( catTranslation.Value, ratTranslation.Value, radius ) )
                    {
                        commandBuffer.AddComponent<LbDestroy>(chunkIndex, ratEntity);
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
            radius                      = 70.0f,
            translationType             = GetArchetypeChunkComponentType<Translation>(),
            entityCatType               = GetArchetypeChunkEntityType(),

            entitiesToVerifyCollision   = ratsGroup.ToComponentDataArray<Translation>( Allocator.TempJob ),
            entityRats                  = ratsGroup.ToEntityArray( Allocator.TempJob ),

            commandBuffer               = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule( catsGroup, inputDeps );

        m_Barrier.AddJobHandleForProducer( collisionJob );

        return collisionJob;
    }
}