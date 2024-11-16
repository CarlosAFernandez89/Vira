using System;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    enum MoveDirection
    {
        Right,
        Left
    }
    
    public class EnemyBasicPatrol : MonoBehaviour
    {
        [Header("Patrol Points")]
        [SerializeField] private Transform leftEdge;
        [SerializeField] private Transform rightEdge;

        [Header("Enemy")] 
        [SerializeField] private Transform enemy;
        
        [Header("Movement Parameters")]
        [SerializeField] private float speed;
        
        [SerializeField] private MoveDirection direction = MoveDirection.Right;
        
        
        private void Start()
        {
            leftEdge.SetParent(null);
            rightEdge.SetParent(null);
        }
        
        private void OnDestroy()
        {
            // Destroy the patrol points (left and right edges) when the parent is destroyed
            if (leftEdge != null)
                Destroy(leftEdge.gameObject);
            if (rightEdge != null)
                Destroy(rightEdge.gameObject);
        }

        private void Update()
        {
            MoveInDirection();
        }

        private void MoveInDirection()
        {
            // Move the enemy
            int tempDirection = 0;
            switch (direction)
            {
                case MoveDirection.Right: tempDirection = 1; break;
                case MoveDirection.Left: tempDirection = -1; break;
                default:
                    break;
            }
            enemy.position = new Vector3(enemy.position.x + (Time.deltaTime * tempDirection * speed), enemy.position.y, enemy.position.z);
            
            // Check if the enemy has reached the left or right edge
            if (tempDirection > 0 && enemy.position.x >= rightEdge.position.x)  // Moving right and reached the right edge
            {
                TurnAndChangeDirection(false);  // Turn left
            }
            else if (tempDirection < 0 && enemy.position.x <= leftEdge.position.x)  // Moving left and reached the left edge
            {
                TurnAndChangeDirection(true);  // Turn right
            }
        }
        
        private void TurnAndChangeDirection(bool turnRight)
        {
            // Reverse the movement direction
            direction = turnRight ? MoveDirection.Right : MoveDirection.Left;

            // Rotate the enemy to face the new direction
            float rotationY = turnRight ? 0f : 180f;
            enemy.rotation = Quaternion.Euler(0f, rotationY, 0f);
        }
    }
}
