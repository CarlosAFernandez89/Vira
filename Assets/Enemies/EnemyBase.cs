using System;
using GameplayAbilitySystem;
using UnityEngine;

namespace Enemies
{
    
    public class EnemyBase : MonoBehaviour, IAbilitySystemComponent
    {
        private Rigidbody2D _rigidbody2D;
        private Transform _transform;

        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return GetComponent(typeof(AbilitySystemComponent)) as AbilitySystemComponent;
        }

        private void Start()
        {
            _transform = GetComponent<Transform>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            TurnAndChangeDirection();
        }
        
        private void TurnAndChangeDirection()
        {
            Quaternion wantedRotation = _rigidbody2D.linearVelocity.x < 0 ?
                Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f);
            
            _transform.rotation = wantedRotation;
            
        }
    }
}
