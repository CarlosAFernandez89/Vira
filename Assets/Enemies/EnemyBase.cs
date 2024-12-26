using System;
using System.Collections;
using System.Collections.Generic;
using AI.BT.Enums;
using AI.BT.EventChannels;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Attributes;
using Unity.Behavior;
using UnityEngine;

namespace Enemies
{
    
    public class EnemyBase : MonoBehaviour, IAbilitySystemComponent
    {

        private Rigidbody2D _rigidbody2D;
        private Transform _transform;
        
        private AttributesComponent _attributesComponent;
        
        [Header("References")]
        [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;

        [Header("On Damage Effects")] 
        [ColorUsage(true, true)] [SerializeField] private Color _damageFlashColor = Color.white;
        [SerializeField] private AnimationCurve _damageFlashCurve;
        private float _damageFlashTime = 0.25f;
        private Coroutine _damageFlashCoroutine;
        [SerializeField] private GameObject _shockWavePrefab;
        [Range(0.1f,1f)][SerializeField] private float _shockWaveTime = 0.25f;
        private Coroutine _shockWaveCoroutine;
        private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");
        private Material _shockWaveMaterial;
        [SerializeField] private float _knockBackDuration = 0.25f;

        private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
        private SpriteRenderer _spriteRenderer;
        private Material _spriteMaterial;
        
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

        private void OnDamageTaken(Vector3 inHitLocation, float damage)
        {
            ApplyKnockBack(inHitLocation, damage);
            _damageFlashCoroutine = StartCoroutine(DamageFlash());
            _shockWaveCoroutine = StartCoroutine(SpawnShockWave());
        }

        private void ApplyKnockBack(Vector3 knockBackLocation, float damageAmount)
        {
            if (_rigidbody2D != null && knockBackLocation != Vector3.zero)
            {
                if (behaviorGraphAgent.SetVariableValue("State", NPCState.KnockBack))
                {
                    if (behaviorGraphAgent.BlackboardReference.GetVariableValue("NPC State Channel", out NPCStateEventChannel stateChannel))
                    {
                        stateChannel.SendEventMessage(NPCState.KnockBack);
                        Debug.Log($"Set blackboard value to Knockback");
                    }
                }
                
                Vector3 knockBackDirection = (transform.position - knockBackLocation).normalized;
                Debug.Log($"Applying knock back towards {knockBackDirection}, with force {damageAmount * -1}");

                _rigidbody2D.AddForce(knockBackDirection * 200, ForceMode2D.Force);
                
                Invoke(nameof(StopKnockBack), _knockBackDuration);
            }
        }

        private void StopKnockBack()
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            if(behaviorGraphAgent.SetVariableValue("State", NPCState.Attacking))
            {
                if (behaviorGraphAgent.BlackboardReference.GetVariableValue("NPC State Channel", out NPCStateEventChannel stateChannel))
                {
                    stateChannel.SendEventMessage(NPCState.Attacking);
                    Debug.Log("Set blackboard value to Attacking");
                }
            }
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

        private IEnumerator SpawnShockWave()
        {
            float elapsedTime = 0f;
            
            var shockWaveInstance = Instantiate(_shockWavePrefab, _transform.position, Quaternion.identity);
            _shockWaveMaterial = shockWaveInstance.GetComponent<SpriteRenderer>().material;

            while (elapsedTime < _shockWaveTime)
            {
                elapsedTime += Time.deltaTime;
                var lerpAmount = Mathf.Lerp(-0.1f,0.5f, elapsedTime/ _shockWaveTime);
                _shockWaveMaterial.SetFloat(_waveDistanceFromCenter, lerpAmount);
                Debug.Log($"Setting material to {lerpAmount}");
                yield return null;
            }
            
            Destroy(shockWaveInstance);
        }
    }
}
