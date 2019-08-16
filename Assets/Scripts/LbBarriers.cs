using Unity.Entities;



public class LbSimulationBarrier : EntityCommandBufferSystem { }

public class LbCheckBarrier : EntityCommandBufferSystem { }

[UpdateAfter(typeof(LbSimulationBarrier))]
[UpdateAfter(typeof(LbCheckBarrier))]
public class LbDestroyBarrier : EntityCommandBufferSystem { }