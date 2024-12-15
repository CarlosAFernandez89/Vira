using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace AI.BT.Actions.Base
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "SetPlayerReference",
        story: "Set [Target] Reference",
        category: "Action/Blackboard",
        id: "10750e34fac66178d7d9437f31eeb53d")]
    public partial class SetPlayerReferenceAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        protected override Status OnStart()
        {
            //Check if value has been set by another instance of this graph.
            if (Target.Value != null)
            {
                return Status.Success;
            }

            Target.Value = GameObject.FindGameObjectWithTag("Player");
            
            return Target.Value == null ? Status.Failure : Status.Success;
        }
        
    }
}

