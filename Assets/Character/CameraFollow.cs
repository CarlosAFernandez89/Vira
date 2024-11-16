using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerTransform;

        [Header("Flip Rotation Stats")] 
        [SerializeField] private float flipYRotationTime = 0.5f;
        
        private Coroutine _turnCoroutine;
        
        private PlayerMovement _playerMovement;

        private void Awake()
        {
            _playerMovement = playerTransform.gameObject.GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            transform.position = playerTransform.position;
        }

        public void CallTurn()
        {
            LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationTime).setEaseInOutSine();
        }

        private float DetermineEndRotation()
        {
            if (_playerMovement.isFacingRight)
            {
                return 0f;
            }
            else
            {
                return 180f;
            }
        }
    }
}
