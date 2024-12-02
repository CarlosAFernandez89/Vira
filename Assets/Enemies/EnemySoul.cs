using System;
using BandoWare.GameplayTags;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Attributes;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemySoul : MonoBehaviour
    {
        private float _arcHeight = 2f; // Maximum height of the arc
        public float duration = 1f; // Time to complete the arc
        public Vector2 explosionRange = new Vector2(2f, 2f); // Random range for explosion spread
        public Vector2 randomHeightRange = new Vector2(1f, 3f); // Random range for arc height
        public bool lerpToPlayer = false; // Enable or disable lerping towards the player
        public float lerpToPlayerSpeed = 5f;
        [Range(1f, 2f)] public float lerpToPlayerWaitTime = 1f;
        public LayerMask groundLayer;

        private int _soulActualValue = 1;
        [SerializeField] [Range(1,50)] private int soulMinValue = 1;
        [SerializeField] [Range(1,50)] private int soulMaxValue = 5;


        private GameObject _player;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _timer = 0f;
        private float _waitTime = 0f;
        private bool _landed = false;
        
        private SpriteRenderer _renderer;
        private float _yOffset;

        public void Initialize(GameObject player)
        {
            _player = player;
            _renderer = this.GetComponent<SpriteRenderer>();
            _yOffset = _renderer.sprite.bounds.size.y / 4;
            
            lerpToPlayerWaitTime = Random.Range(lerpToPlayerWaitTime - 0.5f, lerpToPlayerWaitTime + 0.5f);
            duration = Random.Range(duration - 0.3f, duration + 0.3f);
            _soulActualValue = Random.Range(soulMinValue, soulMaxValue);
            
            if (_player == null)
            {
                lerpToPlayer = false;
            }
            _startPosition = transform.position;
            
            // Generate a random target position within the explosion range
            Vector3 randomOffset = new Vector3(
                Random.Range(-explosionRange.x, explosionRange.x), 
                Random.Range(0f, explosionRange.y), 
                0f
            );
            
            Vector3 tentativeTarget = _startPosition + randomOffset;
            
            // Find the ground below the tentative target position
            RaycastHit2D hit = Physics2D.Raycast(tentativeTarget, Vector2.down, Mathf.Infinity, groundLayer);
            if (hit.collider != null)
            {
                _targetPosition = new Vector3(tentativeTarget.x, hit.point.y + _yOffset, tentativeTarget.z);
            }
            else
            {
                Debug.LogWarning("No ground detected! Using original target position.");
                _targetPosition = new Vector3(tentativeTarget.x, tentativeTarget.y + _yOffset, tentativeTarget.z); // Fallback if no ground is found
            }
            
            // Randomize arc height for varied movement
            _arcHeight = Random.Range(randomHeightRange.x, randomHeightRange.y);
        }

        private void SoulMovement()
        {
            _timer += Time.deltaTime / duration;
            float height = Mathf.Sin(_timer * Mathf.PI) * _arcHeight;
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, _timer) + new Vector3(0f, height, 0f);
                    
            // Check for ground collision
            // Cast a ray downward and debug draw it
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundLayer);

            if (hit.collider != null)
            {
                //Debug.DrawRay(transform.position, Vector2.down * hit.distance, Color.red); // Draw the ray to the hit point

                if (hit.distance <  _yOffset) // Close enough to ground
                {
                    //transform.position = hit.point; // Snap to ground
                    _landed = true;
                }
            }
            else
            {
                //Debug.DrawRay(transform.position, Vector2.down * 10f, Color.yellow); // Draw a ray in case of no hit
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_landed && !lerpToPlayer)
            {
                SoulMovement();
            }
            else if (lerpToPlayer && _player != null)
            {
                if (_landed)
                {
                    _waitTime += Time.deltaTime;

                    if (_waitTime >= lerpToPlayerWaitTime)
                    {
                        transform.position = Vector3.Lerp(transform.position, _player.transform.position,
                            Time.deltaTime * lerpToPlayerSpeed);
                    }
                }
                else
                {
                    SoulMovement();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                //Give soul to player
                AttributeBase souls = collision.gameObject.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Souls");
                souls.CurrentValue += _soulActualValue;

                Destroy(this.gameObject);
            }
        }
    }
}
