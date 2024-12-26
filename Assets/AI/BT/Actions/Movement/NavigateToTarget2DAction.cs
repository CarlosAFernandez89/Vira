using System;
using AI.BT.Enums;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace AI.BT.Actions.Movement
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "NavigateToTarget2D",
        story: "[Agent] navigates to [Target] in 2D",
        category: "Action",
        id: "c2ece8e4228c1cdf7a7e06289c580cb0")]
    public partial class NavigateToTarget2DAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(1.0f);
        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);
        [SerializeReference] public BlackboardVariable<NPCMovementType> NPCMovement = new BlackboardVariable<NPCMovementType>();
        [SerializeReference] public BlackboardVariable<NPCState> _NPCState = new BlackboardVariable<NPCState>();
        
        private float m_PreviousStoppingDistance;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_ColliderAdjustedTargetPosition;
        private float m_ColliderOffset;
        Rigidbody2D _rigidbody2D;
        
        protected override Status OnStart()
        {
            if (Agent.Value == null || Target.Value == null)
            {
                return Status.Failure;
            }

            return Initialize();
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null || Target.Value == null || _rigidbody2D == null)
            {
                return Status.Failure;
            }

            if (_NPCState == NPCState.KnockBack)
            {
                return Status.Success;
            }
            
            // Check if the target position has changed.
            bool boolUpdateTargetPosition = !Mathf.Approximately(m_LastTargetPosition.x, Target.Value.transform.position.x) || !Mathf.Approximately(m_LastTargetPosition.y, Target.Value.transform.position.y) || !Mathf.Approximately(m_LastTargetPosition.z, Target.Value.transform.position.z);
            if (boolUpdateTargetPosition)
            {
                m_LastTargetPosition = Target.Value.transform.position;
                m_ColliderAdjustedTargetPosition = GetPositionColliderAdjusted();
            }
            
            
            // Move agent towards the current waypoint
            Vector3 agentPosition = Agent.Value.transform.position;
            Vector3 movement = (m_ColliderAdjustedTargetPosition - agentPosition).normalized * Speed;

            if (NPCMovement.Value == NPCMovementType.Ground)
            {
                movement.y = Physics.gravity.y;
            }
            
            if (Vector3.Distance(agentPosition, m_ColliderAdjustedTargetPosition) <= DistanceThreshold.Value)
            {
                return Status.Success;
            }
            
            // Update position
            if (_rigidbody2D != null)
            {
                _rigidbody2D.linearVelocity = movement;
            }
            
            return Status.Running;
        }

        private Status Initialize()
        {
            m_LastTargetPosition = Target.Value.transform.position;
            m_ColliderAdjustedTargetPosition = GetPositionColliderAdjusted();
            
            // Add the extents of the colliders to the stopping distance.
            m_ColliderOffset = 0.0f;
            Collider agentCollider = Agent.Value.GetComponentInChildren<Collider>();
            if (agentCollider != null)
            {
                Vector3 colliderExtents = agentCollider.bounds.extents;
                m_ColliderOffset += Mathf.Max(colliderExtents.x, colliderExtents.z);
            }
            
            _rigidbody2D = Agent.Value.GetComponent<Rigidbody2D>();
            
            Vector3 agentPosition = Agent.Value.transform.position;
            if (Vector3.Distance(agentPosition, m_ColliderAdjustedTargetPosition) <= (DistanceThreshold + m_ColliderOffset))
            {
                return Status.Success;
            }
            
            return Status.Running;
        }
        
        private Vector3 GetPositionColliderAdjusted()
        {
            Collider targetCollider = Target.Value.GetComponentInChildren<Collider>();
            if (targetCollider != null)
            {
                return targetCollider.ClosestPoint(Agent.Value.transform.position);
            }
            return Target.Value.transform.position;
        }
        
    }
}

