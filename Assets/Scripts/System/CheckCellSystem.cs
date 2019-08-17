using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;

public class CheckCellSystem : JobComponentSystem
{
    private const short kHoleFlag = 0x100;
    private const short kHomebaseFlag = 0x800;

    private EntityQuery m_ReachQuery;
    private EntityQuery m_BoardQuery;

    private EntityCommandBufferSystem m_Barrier;

    protected override void OnCreate()
    {
        m_ReachQuery = GetEntityQuery(ComponentType.ReadOnly<LbReachCell>(), ComponentType.ReadOnly<Translation>(), typeof(LbDirection), typeof(LbMovementTarget));
        m_BoardQuery = GetEntityQuery(ComponentType.ReadOnly<LbBoard>(), ComponentType.ReadOnly<LbDirectionMap>());

        m_Barrier = World.GetOrCreateSystem<LbCheckBarrier>();
    }

    struct CheckCellNewJob : IJobChunk
    {
        [ReadOnly] public int Size;
        [ReadOnly] public NativeArray<LbDirectionMap> Buffer;

        [ReadOnly] public ArchetypeChunkEntityType EntityType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkComponentType<LbRat> RatType;

        public ArchetypeChunkComponentType<LbDirection> DirectionType;
        public ArchetypeChunkComponentType<LbMovementTarget> TargetType;
        public ArchetypeChunkComponentType<LbDistanceToTarget> DistanceToTargetType;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var entities = chunk.GetNativeArray(EntityType);
            var translations = chunk.GetNativeArray(TranslationType);
            var directions = chunk.GetNativeArray(DirectionType);
            var targets = chunk.GetNativeArray(TargetType);
            var distanceTargets = chunk.GetNativeArray(DistanceToTargetType);
            var areRats = chunk.Has(RatType);

            for (int i=0; i<chunk.Count; ++i)
            {
                var entity = entities[i];
                var translation = translations[i].Value;
                var direction = directions[i];

                var index = ((int)translation.z) * Size + (int)translation.x;
                if (index < 0 || index >= Buffer.Length)
                    continue;

                var cellMapValue = Buffer[index].Value;
                if ((cellMapValue & kHoleFlag) == kHoleFlag)
                {
                    CommandBuffer.RemoveComponent<LbDirection>(chunkIndex, entity);

                    CommandBuffer.AddComponent(chunkIndex, entity, new LbFall());
                    if (areRats)
                        CommandBuffer.AddComponent(chunkIndex, entity, new LbLifetime() { Value = 1.0f });
                    else
                        CommandBuffer.SetComponent(chunkIndex, entity, new LbLifetime() { Value = 1.0f });
                    CommandBuffer.SetComponent(chunkIndex, entity, new LbMovementSpeed() { Value = -2.5f });
                }
                else if ((cellMapValue & kHomebaseFlag) == kHomebaseFlag)
                {
                    CommandBuffer.AddComponent(chunkIndex, entity, new LbDestroy());

                    var player = (cellMapValue >> 9) & 0x3;

                    var scoreEntity = CommandBuffer.CreateEntity(chunkIndex);
                    if (areRats)
                        CommandBuffer.AddComponent(chunkIndex, scoreEntity, new LbRatScore() { Player = (byte)player });
                    else
                        CommandBuffer.AddComponent(chunkIndex, scoreEntity, new LbCatScore() { Player = (byte)player });
                }
                else
                {
                    var nextDirectionByte = (byte)((cellMapValue >> LbDirection.GetByteShift(direction.Value)) & 0x3);
                    directions[i] = new LbDirection() { Value = nextDirectionByte };
                    targets[i] = new LbMovementTarget() { From = translation, To = translation + LbDirection.GetDirection(nextDirectionByte) };
                    distanceTargets[i] = new LbDistanceToTarget() { Value = 0.0f };
                }

                CommandBuffer.RemoveComponent<LbReachCell>(chunkIndex, entity);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var board = m_BoardQuery.GetSingleton<LbBoard>();
        var boardEntity = m_BoardQuery.GetSingletonEntity();
        var bufferLookup = GetBufferFromEntity<LbDirectionMap>();

        var buffer = bufferLookup[boardEntity];
        var bufferArray = buffer.AsNativeArray();

        var entityType = GetArchetypeChunkEntityType();
        var translationType = GetArchetypeChunkComponentType<Translation>();
        var directionType = GetArchetypeChunkComponentType<LbDirection>();
        var targetType = GetArchetypeChunkComponentType<LbMovementTarget>();
        var distanceTargetType = GetArchetypeChunkComponentType<LbDistanceToTarget>();
        var ratType = GetArchetypeChunkComponentType<LbRat>();
        
        var job = new CheckCellNewJob
        {
            Size = board.SizeY,
            Buffer = bufferArray,
            
            EntityType = entityType,
            TranslationType = translationType,
            RatType = ratType,

            DirectionType = directionType,
            TargetType = targetType,
            DistanceToTargetType = distanceTargetType,

            CommandBuffer = m_Barrier.CreateCommandBuffer().ToConcurrent()
        }.Schedule(m_ReachQuery, inputDeps);
        
        m_Barrier.AddJobHandleForProducer(job);
        return job;
    }
}
