using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Character
{
    public class PlayerActions : MonoBehaviour
    {
        [Header("References")] 
        private IInteract _interactInstance;
        [SerializeField] private InputActionReference interactAction;


        private void OnEnable()
        {
            interactAction.action.started += Interact;
        }

        private void OnDisable()
        {
            interactAction.action.started -= Interact;
        }

        #region Interaction
        
        private void Interact(InputAction.CallbackContext context)
        {
            if (_interactInstance != null)
            {
                _interactInstance.Interact();
            }
        }

        public void SetInteractTarget(IInteract target)
        {
            _interactInstance = target;
        }

        public void ClearInteractTarget()
        {
            _interactInstance = null;
        }
        
        #endregion

    }
}
