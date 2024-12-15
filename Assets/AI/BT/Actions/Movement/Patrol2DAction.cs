using System;
using System.Collections.Generic;
using AI.BT.Enums;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace AI.BT.Actions.Movement
{
    
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Patrol 2D",
        description: "Moves a GameObject along way points (transform children of a GameObject).",
        category: "Action/Navigation",
        story: "[Agent] patrols 2D along [Waypoints]")]
    public class Patrol2DAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<GameObject> Agent;
        
        [SerializeReference] 
        public BlackboardVariable<List<GameObject>> Waypoints;
        
        [SerializeReference] 
        public BlackboardVariable<float> Speed;
        
        [SerializeReference] 
        public BlackboardVariable<float> WaypointWaitTime = new BlackboardVariable<float>(1.0f);
        
        [SerializeReference] 
        public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);
        
        [SerializeReference] public BlackboardVariable<NPCMovementType> NPCMovement = new BlackboardVariable<NPCMovementType>();

        [CreateProperty]
        private Vector3 m_CurrentTarget;
        
        [CreateProperty]
        private int m_CurrentPatrolPoint = 0;
        
        [CreateProperty]
        private bool m_Waiting;
        
        [CreateProperty]
        private float m_WaypointWaitTimer;
        
        private int _currentWaypointIndex = 0; 
        private float _waitTimer = 0.0f; 
        Rigidbody2D _rigidbody2D;

        protected override Status OnStart()
        {
            if (Agent.Value == null)
            {
                LogFailure("No agent assigned.");
                return Status.Failure;
            }
            
            if (Waypoints.Value == null || Waypoints.Value.Count == 0)
            {
                LogFailure("No waypoints to patrol assigned.");
                return Status.Failure;
            }
            
            _rigidbody2D = Agent.Value.GetComponent<Rigidbody2D>();

            m_Waiting = false;
            _waitTimer = 0.0f;

            MoveToNextWaypoint();
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Agent.Value == null || Waypoints.Value == null)
            {
                return Status.Failure;
            }
            
            if (m_Waiting)
            {
                if (_waitTimer > 0.0f)
                {
                    _waitTimer -= Time.deltaTime;
                }
                else
                {
                    _waitTimer = 0f;
                    m_Waiting = false;
                    MoveToNextWaypoint();
                }
            }
            else
            {
                // Move agent towards the current waypoint
                Vector3 agentPosition = Agent.Value.transform.position;
                Vector3 targetPosition = Waypoints.Value[_currentWaypointIndex].transform.position;
                Vector3 movement = (targetPosition - agentPosition).normalized * Speed;

                if (NPCMovement.Value == NPCMovementType.Ground)
                {
                    movement.y = 0.0f;
                }

                // Update position
                if (_rigidbody2D != null)
                {
                    _rigidbody2D.linearVelocity = movement;
                }

                // Check if the agent has reached the waypoint
                if (Vector3.Distance(agentPosition, targetPosition) <= DistanceThreshold.Value)
                {
                    m_Waiting = true;
                    _waitTimer = m_WaypointWaitTimer;
                }
            }

            return Status.Running;
        }
        
        private void MoveToNextWaypoint()
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % Waypoints.Value.Count;
        }
    }
}