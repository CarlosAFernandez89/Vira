using System;
using AI.Dialogue;
using Character;
using Interface;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AI.NPC
{
    public class NPCDialogue : MonoBehaviour, IInteract
    {
        private DialogueParser _dialogueParser;
        
        private GameObject _playerCharacter;
        private PlayerInput _playerInput;

        private InputAction _exitDialogue;

        private void Awake()
        {
            _dialogueParser = gameObject.GetComponent<DialogueParser>();
        }

        public void Interact()
        {
            if (_dialogueParser != null)
            {
                StartDialogue();
            }
        }

        private void StartDialogue()
        {
            _playerInput.actions.FindActionMap("Player").Disable();
            _playerInput.actions.FindActionMap("Dialogue").Enable();
            
            BindInputActions();
            
            _dialogueParser.EnableDialogue();

        }

        public void EndDialogue()
        {
            _dialogueParser.DisableDialogue();
            
            UnbindInputActions();
            
            _playerInput.actions.FindActionMap("Dialogue").Disable();
            _playerInput.actions.FindActionMap("Player").Enable();
            
        }

        private void EndDialogue(InputAction.CallbackContext context)
        {
            EndDialogue();
        }

        private void BindInputActions()
        {
            if (_playerInput != null)
            {
                _exitDialogue = _playerInput.actions.FindActionMap("Dialogue").FindAction("ExitDialogue");
                _exitDialogue.performed += EndDialogue;
            }
        }

        private void UnbindInputActions()
        {
            if (_exitDialogue != null)
            {
                _exitDialogue.performed -= EndDialogue;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerCharacter = other.gameObject;
                _playerCharacter.GetComponent<PlayerActions>().SetInteractTarget(this);
                _playerInput = _playerCharacter.GetComponent<PlayerInput>();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerCharacter.GetComponent<PlayerActions>().ClearInteractTarget();
                _playerCharacter = null;
            }
        }
    }
}