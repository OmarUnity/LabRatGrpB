using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class CheckCellSystem : JobComponentSystem
{
    private EntityQuery m_Reachquery;
    private EntityQuery m_Boardquery;
    struct CheckCellJob : IJobChunk
    {
        [ReadOnly] public Entity BoardEntity;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public BufferFromEntity<LbDirectionMap> FlowMap;
        [ReadOnly] public int2 BoardSize;
                   public EntityCommandBuffer.Concurrent CommandBuffer;
        
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var buffer = FlowMap[BoardEntity];
            var chunkTranslation = chunk.GetNativeArray(TranslationType);
            
            for (var i = 0; i < chunk.Count; i++)
            {
                var translation = chunkTranslation[i].Value;
                int index = ((int) translation.z) * BoardSize.y + (int) translation.x;
            }
        }
    }

    protected override void OnCreate()
    {
        m_Reachquery = GetEntityQuery(ComponentType.ReadOnly<LbReachCell>(),ComponentType.ReadOnly<Translation>());
        m_Boardquery = GetEntityQuery(ComponentType.ReadOnly<LbBoard>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var lbBoardType = GetArchetypeChunkComponentType<LbBoard>();
        var array = m_Boardquery.ToEntityArray(Allocator.TempJob);
        var arrayLbBoard = m_Boardquery.ToComponentDataArray<LbBoard>(Allocator.TempJob);
        var boardEntity = array[0];
        var lbBoard = arrayLbBoard[0];
        array.Dispose();
        arrayLbBoard.Dispose();
        
        var lookup = GetBufferFromEntity<LbDirectionMap>();

        var job = new CheckCellJob
        {
            BoardEntity = boardEntity,
            FlowMap = lookup,
            TranslationType = translationType,
            BoardSize = new int2(lbBoard.SizeX,lbBoard.SizeY)
        }.Schedule(m_Reachquery, inputDeps);

        return job;
    }
}
