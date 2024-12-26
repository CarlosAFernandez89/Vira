using System;
using Unity.Behavior;

namespace AI.BT.Enums
{
    [BlackboardEnum]
    public enum NPCState
    {
        Idle,
        Investigating,
        Attacking,
        KnockBack
    }
}