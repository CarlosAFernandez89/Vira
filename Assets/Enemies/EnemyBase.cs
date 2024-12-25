using System;
using System.Collections;
using System.Collections.Generic;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Attributes;
using UnityEngine;

namespace Enemies
{
    
    public class EnemyBase : MonoBehaviour, IAbilitySystemComponent
    {

        private Rigidbody2D _rigidbody2D;
        private Transform _transform;
        
        private AttributesComponent _attributesComponent;

        [Header("On Damage Effects")] 
        [ColorUsage(true, true)] [SerializeField] private Color _damageFlashColor = Color.white;
        [SerializeField] private AnimationCurve _damageFlashCurve;
        private float _damageFlashTime = 0.25f;

        private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
        private SpriteRenderer _spriteRenderer;
        private Material _spriteMaterial;
        private Coroutine _damageFlashCoroutine;
        
        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return GetComponent(typeof(AbilitySystemComponent)) as AbilitySystemComponent;
        }

        private void Awake()
        {
            BindAttributeEvents();
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
                        
            _spriteMaterial = new Material(_spriteRenderer.material);
            _spriteMaterial = _spriteRenderer.material;
            _damageFlashTime = _damageFlashCurve.keys[_damageFlashCurve.length - 1].time;
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
        
        private void BindAttributeEvents()
        {
            GetAbilitySystemComponent().OnDamageTaken += OnDamageTaken;
        }

        private void OnDamageTaken()
        {
            _damageFlashCoroutine = StartCoroutine(DamageFlash());
        }

        private IEnumerator DamageFlash()
        {
            SetFlashColor();

            float elapsedTime = 0f;

            while (elapsedTime < _damageFlashTime)
            {
                elapsedTime += Time.deltaTime;
                
                SetFlashAmount(Mathf.Lerp(0f, _damageFlashCurve.Evaluate(elapsedTime), elapsedTime / _damageFlashTime));
                
                yield return null;
                
            }
            
            SetFlashAmount(0);
        }

        private void SetFlashColor()
        {
            _spriteMaterial.SetColor(FlashColor, _damageFlashColor);
        }

        private void SetFlashAmount(float flashAmount)
        {
            _spriteMaterial.SetFloat(FlashAmount, flashAmount);
        }
    }
}
