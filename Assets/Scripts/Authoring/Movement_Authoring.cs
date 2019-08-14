using UnityEngine;
using Unity.Entities;

/// <summary>
/// Class made for testing purposes. According the struct direction is going to be
/// added certain kind of Component
/// </summary>
public class Movement_Authoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public enum MovementDirection { Up, Down, Right, Left };
    public MovementDirection direction;
    public float speed = 1.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if ( direction == MovementDirection.Up )
            dstManager.AddComponentData( entity, new LbNorthDirection() );
        if (direction == MovementDirection.Down)
            dstManager.AddComponentData( entity, new LbSouthDirection() );
        if (direction == MovementDirection.Right)
            dstManager.AddComponentData( entity, new LbEastDirection() );
        if (direction == MovementDirection.Left)
            dstManager.AddComponentData( entity, new LbWestDirection() );

        dstManager.AddComponentData( entity, new LbMovementSpeed { Value = speed } );
        dstManager.AddComponentData( entity, new LbDistanceToTarget() { Value = 1 } );
    }
}
