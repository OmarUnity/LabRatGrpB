using UnityEngine;
using Unity.Entities;

public class Rotation_Authoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public enum MovementDirection { North, South, West, East };
    public MovementDirection    direction = MovementDirection.North;
    public float                rotationSpeed = 1.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if ( direction == MovementDirection.North)
            dstManager.AddComponentData(entity, new LbNorthDirection());
        if ( direction == MovementDirection.South)
            dstManager.AddComponentData(entity, new LbSouthDirection());
        if ( direction == MovementDirection.West)
            dstManager.AddComponentData(entity, new LbWestDirection());
        if (direction == MovementDirection.East)
            dstManager.AddComponentData(entity, new LbEastDirection());

        dstManager.AddComponentData(entity, new LbRotationSpeed { Value = rotationSpeed } );
    }
}
