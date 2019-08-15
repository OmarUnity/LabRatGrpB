using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class RotationSystem : JobComponentSystem
{
    public struct RotationJob : IJobForEach<Rotation, LbRotationSpeed>
    {
        public float    deltaTime;
        public float3   rotationDirection;

        public void Execute(ref Rotation rotation, [ReadOnly] ref LbRotationSpeed rotationSpeed)
        {
            Debug.Log( "Rotate entity" );
            rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotationSpeed.Value * deltaTime));
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Mathf.Clamp(Time.deltaTime, 0.0f, 0.3f);

        var job_Rotation = new RotationJob
        {
            deltaTime = deltaTime,
            rotationDirection = new float3( 1, 0, 0 )
        };

        return job_Rotation.Schedule(this, inputDeps);
    }
}
