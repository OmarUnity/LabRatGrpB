using Unity.Entities;

/// <summary>
/// Used by entities that are moving in the Up direction
/// 3D Vector (0, 0, 1)
/// </summary>
public struct LbUpDirection : IComponentData
{
}

/// <summary>
/// Used by entities that are moving in the Down direction
/// 3D Vector (0, 0, -1)
/// </summary>
public struct LbDownDirection : IComponentData
{
}

/// <summary>
/// Used by entities that are moving in the Up direction
/// 3D Vector (1, 0, 0)
/// </summary>
public struct LbRightDirection : IComponentData
{
}

/// <summary>
/// Used by entities that are moving in the Left direction
/// 3D Vector (-1, 0, 0)
/// </summary>
public struct LbLeftDirection : IComponentData
{
}
