using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AI.NPC;
using Dialogue.Runtime;
using NUnit.Framework;
using TMPro;
using Unity.AppUI.UI;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace AI.Dialogue
{
    public class DialogueParser : MonoBehaviour
    {
        private DialogueContainer _currentDialogue;

        [Header("Dialogue UI Elements")] 
        private VisualElement _root;
        [SerializeField] private UIDocument dialogueUIDocument;
        [SerializeField] private VisualTreeAsset dialogueUIAsset;
        [SerializeField] private VisualTreeAsset dialogueChoiceUIAsset;

        public bool TextAnimationEnabled { get; set; }
        public float TextAnimationSpeed { get; set; }

        private NPCDialogue _npcDialogue;

        
        private bool _isTyping = false;
        private Label _dialogueText;
        private string _currentDialogueText;
        private const string HTML_ALPHA = "<color=#00000000>";
        private const float MAX_TYPE_TIME = 0.1f;
        private Coroutine _typingCoroutine;

        private IEnumerable<NodeLinkData> _currentChoices;
        private VisualElement _currentDialogueChoicesContainer;
        
        private void Awake()
        {
            _root = dialogueUIDocument.rootVisualElement;
            _root.style.display = DisplayStyle.None;
            
            _npcDialogue = gameObject.GetComponent<NPCDialogue>();
            Assert.IsNotNull(_npcDialogue, $"NPCDialogue component not found on {gameObject.name}");
            
            dialogueUIAsset.CloneTree(_root);
        }

        public void EnableDialogue()
        {
            _root.style.display = DisplayStyle.Flex;
            
            _currentDialogue = _npcDialogue.GetCurrentDialogue();
            Assert.IsNotNull(_currentDialogue, $"Failed to get CurrentDialogue from {gameObject.name}. Please check the NPCDialog and assign a dialogue to the list.");
            
            var narrativeData = _currentDialogue.NodeLinks.First(); //Entrypoint node
            ProceedToNarrative(narrativeData.TargetNodeGUID);
        }

        public void DisableDialogue()
        {
            _root.style.display = DisplayStyle.None;
        }

        private void ProceedToNarrative(string narrativeDataGUID)
        {
            string text = _currentDialogue.DialogueNodeData.Find(x => x.GUID == narrativeDataGUID).DialogueName;
            _currentChoices = _currentDialogue.NodeLinks.Where(x => x.BaseNodeGUID == narrativeDataGUID);
            
            // Set Dialogue Text
            _dialogueText = _root.Q<Label>("dialogue-text");
            _currentDialogueText = ProcessProperties(text);
            
            // Clear all previous Dialogue Choices
            _currentDialogueChoicesContainer = _root.Q<VisualElement>("dialogue-choices-container");
            _currentDialogueChoicesContainer.Clear();

            if (TextAnimationEnabled)
            {
                _typingCoroutine = StartCoroutine(TypeDialogueText(_currentDialogueText));
            }
            else
            {
                _dialogueText.text = _currentDialogueText;
                ShowDialogueChoices();
            }
            
        }
        
        
        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in _currentDialogue.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }

        private void ShowDialogueChoices()
        {
            // Add all new Dialogue Choices
            foreach (var choice in _currentChoices)
            {
                VisualElement choiceElement = dialogueChoiceUIAsset.Instantiate();
                Button button = choiceElement.Q<Button>();
                button.focusable = true;
                button.text = ProcessProperties(choice.PortName);
                button.clicked += () => { ProceedToNarrative(choice.TargetNodeGUID); };
                _currentDialogueChoicesContainer.Add(choiceElement);
            }

            // Add an exit option if no more dialogue options are given.
            if (_currentDialogueChoicesContainer.childCount <= 0)
            {
                VisualElement choiceElement = dialogueChoiceUIAsset.Instantiate();
                Button button = choiceElement.Q<Button>();
                button.focusable = true;
                button.text = "Exit";
                button.clicked += () => { _npcDialogue.EndDialogue(); };
                _currentDialogueChoicesContainer.Add(choiceElement);
            }
            
            FocusOnFirstDialogueOption();
        }
        
        private void FocusOnFirstDialogueOption()
        {
            Button firstChoice = _currentDialogueChoicesContainer.Q<Button>();
            if (firstChoice != null)
            {
                firstChoice.Focus();
            }
        }

        private IEnumerator TypeDialogueText(string dialogue)
        {
            _isTyping = true;

            _dialogueText.text = "";
            
            string originalText = dialogue;
            int alphaIndex = 0;

            foreach (char c in dialogue.ToCharArray())
            {
                alphaIndex++;
                _dialogueText.text = dialogue;
                
                string displayedText = _dialogueText.text.Insert(alphaIndex, HTML_ALPHA);
                _dialogueText.text = displayedText;
                
                yield return new WaitForSeconds(MAX_TYPE_TIME/ TextAnimationSpeed);
            }
            
            ShowDialogueChoices();
            
            _isTyping = false;
        }

        public void FinishDialogueEarly(InputAction.CallbackContext context)
        {
            // this could be called after it has already finished,
            // so we want to stop it from doing that so it doesn't show duplicate dialogue choices.
            if (!_isTyping) return; 
            
            StopCoroutine(_typingCoroutine);

            _dialogueText.text = _currentDialogueText;
            
            ShowDialogueChoices();
            
            _isTyping = false;
        }
    }
}