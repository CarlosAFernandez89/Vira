using UnityEngine;

namespace Character.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("References")]
        private Transform _playerTransform;

        [Header("Flip Rotation Stats")] 
        [SerializeField] private float flipYRotationTime = 0.5f;
        
        private Coroutine _turnCoroutine;
        
        private PlayerMovement _playerMovement;
        

        private void Start()
        {
            if (_playerTransform == null)
            {
                _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
                _playerMovement = _playerTransform.gameObject.GetComponent<PlayerMovement>();
            }
        }

        private void Update()
        {
            transform.position = _playerTransform.position;
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
