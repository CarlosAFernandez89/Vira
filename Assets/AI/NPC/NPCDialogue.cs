using System;
using System.Collections.Generic;
using AI.Dialogue;
using Character;
using Dialogue.Runtime;
using Interface;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace AI.NPC
{
    public class NPCDialogue : MonoBehaviour, IInteract
    {
        
        [Header("Dialogue")]
        [SerializeField] private DialogueContainer dialogue;
        [SerializeField] private int currentDialogueIndex = 0;
        
        [Header("Interact Options")]
        [SerializeField] private string interactText;

        private TextMeshProUGUI _interactText;
        
        private DialogueParser _dialogueParser;
        
        private GameObject _playerCharacter;
        private PlayerInput _playerInput;

        private InputAction _exitDialogue;

        private void Awake()
        {
            _dialogueParser = gameObject.GetComponent<DialogueParser>();
            
            SetupInteractUI();
        }

        private void SetupInteractUI()
        {
            _interactText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            _interactText.text = interactText;
        }

        public DialogueContainer GetCurrentDialogue()
        {
            return dialogue != null ? dialogue : null;
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
                _interactText.enabled = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerCharacter.GetComponent<PlayerActions>().ClearInteractTarget();
                _playerCharacter = null;
                _interactText.enabled = false;
            }
        }
    }
}