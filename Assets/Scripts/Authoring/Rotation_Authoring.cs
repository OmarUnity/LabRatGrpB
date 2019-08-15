using UnityEngine;
using Unity.Entities;

public class Rotation_Authoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public enum MovementDirection { Up, Down, Right, Left };
    public MovementDirection    direction = MovementDirection.Up;
    public float                rotationSpeed = 1.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if ( direction == MovementDirection.Up )
            dstManager.AddComponentData(entity, new LbNorthDirection());
        if ( direction == MovementDirection.Down )
            dstManager.AddComponentData(entity, new LbSouthDirection());
        if ( direction == MovementDirection.Right )
            dstManager.AddComponentData(entity, new LbEastDirection());
        if ( direction == MovementDirection.Left )
            dstManager.AddComponentData(entity, new LbWestDirection());

        dstManager.AddComponentData(entity, new LbRotationSpeed { Value = rotationSpeed } );
    }
}
