using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace AI.BT.Conditions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "CheckFoVAggro",
        story: "[Agent] has [Target] in view below [Angle] degrees and closer than [MaxDistance]",
        category: "Conditions",
        id: "393ab8da90ed3819d91b49f30d17a71d")]
    public partial class CheckFoVAggroSequence : Condition
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> Angle;
        [SerializeReference] public BlackboardVariable<float> MaxDistance;

        public override bool IsTrue()
        {
            if (Agent.Value == null || Target.Value == null)
            {
                return false;
            }

            float distance = Vector3.Distance(Agent.Value.transform.position, Target.Value.transform.position);
            
            return IsLookingAtTarget(Agent.Value.transform, Target.Value.transform.position, Angle)
                   && HasTargetInView(Agent.Value.transform.position, Target.Value.transform) && distance <= MaxDistance;
        }
        
        private static bool IsLookingAtTarget(Transform origin, Vector3 targetPosition, float angle)
        {
            var forward = origin.forward;
            var toTarget = targetPosition - origin.position;
            var currentAngle = Vector3.Angle(forward, toTarget);
            return currentAngle < angle * 0.5f; // andlge is divided by 2 as it is angles in both direction.
        }

        private static bool HasTargetInView(Vector3 origin, Transform target)
        {
            if (Physics.Linecast(origin, target.position, out var t))
            {
                return t.collider.gameObject == target.gameObject;
            }

            return false;
        }

    }
}

