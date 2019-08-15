using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class RotationSystem : JobComponentSystem
{
    public struct RotationJob : IJobForEach<Rotation, LbRotationSpeed, Translation>
    {
        public float    deltaTime;
        public float3   rotationDirection;

        public void Execute(ref Rotation rotation, [ReadOnly] ref LbRotationSpeed rotationSpeed, [ReadOnly] ref Translation translation)
        {
            //rotation.Value = math.mul( math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotationSpeed.Value * deltaTime) );

            float3 forward  = math.forward( rotation.Value );
            float3 dir      = forward - (rotationDirection * deltaTime * rotationSpeed.Value);
            dir.y           = 0.0f;

            rotation.Value = quaternion.LookRotation( dir, math.up() );
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Mathf.Clamp(Time.deltaTime, 0.0f, 0.3f);

        var job_Rotation = new RotationJob
        {
            deltaTime = deltaTime,
            rotationDirection = new float3( 1f, 0, 0 )
        };

        return job_Rotation.Schedule(this, inputDeps);
    }
}
