using Unity.Entities;

/// <summary>
/// Used by entities that are moving in the Up direction
/// 3D Vector (0, 0, 1)
/// </summary>
public struct LbNorthDirection : IComponentData
{
}

/// <summary>
/// Used by entities that are moving in the Down direction
/// 3D Vector (0, 0, -1)
/// </summary>
public struct LbSouthDirection : IComponentData
{
}

/// <summary>
/// Used by entities that are moving in the Up direction
/// 3D Vector (1, 0, 0)
/// </summary>
public struct LbEastDirection : IComponentData
{
}

/// <summary>
/// Used by entities that are moving in the Left direction
/// 3D Vector (-1, 0, 0)
/// </summary>
public struct LbWestDirection : IComponentData
{
}
