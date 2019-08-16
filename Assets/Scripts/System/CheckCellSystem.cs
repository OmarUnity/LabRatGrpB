using System.Diagnostics;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

public class CheckCellSystem : JobComponentSystem
{
    private const short kHoleFlag = 0x100;
    private const short kHomebaseFlag = 0x800;

    private EntityQuery m_Reachquery;
    private EntityQuery m_Boardquery;

    private EntityCommandBufferSystem m_Barrier;

    
    struct CheckCellJob : IJobChunk
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        
        [ReadOnly] public int2 BoardSize;
        [ReadOnly] public Entity BoardEntity;
        [ReadOnly] public ArchetypeChunkEntityType EntityType;
        [ReadOnly] public BufferFromEntity<LbDirectionMap> FlowMap;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkComponentType<LbNorthDirection> DirectionNorthType;
        [ReadOnly] public ArchetypeChunkComponentType<LbSouthDirection> DirectionSouthType;
        [ReadOnly] public ArchetypeChunkComponentType<LbEastDirection> DirectionEastType;
        [ReadOnly] public ArchetypeChunkComponentType<LbWestDirection> DirectionWestType;

        [ReadOnly] public ArchetypeChunkComponentType<LbRat> RatType;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var buffer = FlowMap[BoardEntity];
            var entities = chunk.GetNativeArray(EntityType);
            var isRats = chunk.Has(RatType);

            var   byteShift = 0;
            var   componentType = 0;
            
            if (chunk.Has(DirectionNorthType))
            {
                byteShift = 6;
                componentType = 0; 
            }
            else if (chunk.Has(DirectionSouthType))
            {
                byteShift = 2;
                componentType = 1;
            }
            else if (chunk.Has(DirectionWestType))
            {
                byteShift = 0;
                componentType = 2;
            }
            else if (chunk.Has(DirectionEastType))
            {
                byteShift = 4;
                componentType = 3;
            }

            // Make issue for chunk component removal in a CommandBuffer
            
            var chunkTranslation = chunk.GetNativeArray(TranslationType);
            
            for (var i = 0; i < chunk.Count; i++)
            {
                var entity = entities[i];
                var translation = chunkTranslation[i].Value;
                var index = ((int) translation.z) * BoardSize.y + (int) translation.x;
                var cellMapValue = buffer[index].Value;

                if ((cellMapValue & kHoleFlag) == kHoleFlag)
                {
                    RemoveMovement(chunkIndex, entity, componentType);
                    CommandBuffer.AddComponent(chunkIndex, entity, new LbFall());
                    CommandBuffer.AddComponent(chunkIndex, entity, new LbLifetime() { Value = 1.0f });
                    CommandBuffer.SetComponent(chunkIndex, entity, new LbMovementSpeed() { Value = -2.5f });
                }
                else if ((cellMapValue & kHomebaseFlag) == kHomebaseFlag)
                {
                    CommandBuffer.AddComponent(chunkIndex, entity, new LbDestroy());

                    var player = (cellMapValue >> 9) & 0x3;

                    var scoreEntity = CommandBuffer.CreateEntity(chunkIndex);
                    if (isRats)
                        CommandBuffer.AddComponent(chunkIndex, scoreEntity, new LbRatScore() { Player = (byte)player });
                    else
                        CommandBuffer.AddComponent(chunkIndex, scoreEntity, new LbCatScore() { Player = (byte)player });
                }
                else
                {
                    var nextDir = (cellMapValue >> byteShift) & 0x3;
                    RemoveMovement(chunkIndex, entity, componentType);
                    MoveToNextCell(chunkIndex, entity, nextDir);
                }

                CommandBuffer.RemoveComponent<LbReachCell>(chunkIndex, entity);
            } 
        }

        private  void RemoveMovement(int chunkIndex, Entity entity, int componentType)
        {
            switch (componentType)
            {
                case 0:
                    CommandBuffer.RemoveComponent<LbNorthDirection>(chunkIndex, entity);
                    break;
                case 1:
                    CommandBuffer.RemoveComponent<LbSouthDirection>(chunkIndex, entity);
                    break;
                case 2:
                    CommandBuffer.RemoveComponent<LbWestDirection>(chunkIndex, entity);
                    break;
                case 3:
                    CommandBuffer.RemoveComponent<LbEastDirection>(chunkIndex, entity);
                    break;
            }
        }

        /// <summary>
        /// Move the entity to the next cell
        /// </summary>
        private void MoveToNextCell(int chunkIndex, Entity entity, int nextDir)
        {
            switch (nextDir)
            {
                //North
                case 0x0:
                    CommandBuffer.AddComponent<LbNorthDirection>(chunkIndex, entity);
                    break;

                //South
                case 0x2:
                    CommandBuffer.AddComponent<LbSouthDirection>(chunkIndex, entity);
                    break;

                //West
                case 0x3:
                    CommandBuffer.AddComponent<LbWestDirection>(chunkIndex, entity);
                    break;

                //East
                case 0x1:
                    CommandBuffer.AddComponent<LbEastDirection>(chunkIndex, entity);
                    break;
            }
        }
    }

    protected override void OnCreate()
    {
        m_Reachquery = GetEntityQuery(ComponentType.ReadOnly<LbReachCell>(),ComponentType.ReadOnly<Translation>());
        m_Boardquery = GetEntityQuery(ComponentType.ReadOnly<LbBoard>());
        m_Barrier = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Get the board entity
        var array = m_Boardquery.ToEntityArray(Allocator.TempJob);
		var boardEntity = array[0];
		array.Dispose();

		// Get the board Component Data
		var arrayLbBoard = m_Boardquery.ToComponentDataArray<LbBoard>(Allocator.TempJob);
		var lbBoard = arrayLbBoard[0];
		arrayLbBoard.Dispose();
		

		var commandBuffer  =  m_Barrier.CreateCommandBuffer().ToConcurrent();
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var directionNorthType = GetArchetypeChunkComponentType<LbNorthDirection>();
        var directionSouthType = GetArchetypeChunkComponentType<LbSouthDirection>();
        var directionWestType = GetArchetypeChunkComponentType<LbWestDirection>();
        var directionEastType = GetArchetypeChunkComponentType<LbEastDirection>();
        var entityType = GetArchetypeChunkEntityType();
        var ratType = GetArchetypeChunkComponentType<LbRat>();
        
        var lookup = GetBufferFromEntity<LbDirectionMap>();

        var job = new CheckCellJob
        {

            
            BoardEntity = boardEntity,
            EntityType = entityType,
            FlowMap = lookup,
            CommandBuffer = commandBuffer,
            TranslationType = translationType,
            DirectionNorthType = directionNorthType,
            DirectionSouthType = directionSouthType,
            DirectionWestType = directionWestType,
            DirectionEastType = directionEastType,
            RatType = ratType,
            BoardSize = new int2(lbBoard.SizeX,lbBoard.SizeY)
            
        }.Schedule(m_Reachquery, inputDeps);
        
        m_Barrier.AddJobHandleForProducer(job);
        return job;
    }
}
