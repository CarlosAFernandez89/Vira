using System;
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

        private NPCDialogue _npcDialogue;
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
            IEnumerable<NodeLinkData> choices = _currentDialogue.NodeLinks.Where(x => x.BaseNodeGUID == narrativeDataGUID);

            // Set Dialogue Text
            Label dialogueText = _root.Q<Label>("dialogue-text");
            dialogueText.text = ProcessProperties(text);

            // Clear all previous Dialogue Choices
            VisualElement dialogueChoicesContainer = _root.Q<VisualElement>("dialogue-choices-container");
            dialogueChoicesContainer.Clear();

            // Add all new Dialogue Choices
            foreach (var choice in choices)
            {
                VisualElement choiceElement = dialogueChoiceUIAsset.Instantiate();
                Button button = choiceElement.Q<Button>();
                button.focusable = true;
                button.text = ProcessProperties(choice.PortName);
                button.clicked += () => { ProceedToNarrative(choice.TargetNodeGUID); };
                dialogueChoicesContainer.Add(choiceElement);
            }

            // Add an exit option if no more dialogue options are given.
            if (dialogueChoicesContainer.childCount <= 0)
            {
                VisualElement choiceElement = dialogueChoiceUIAsset.Instantiate();
                Button button = choiceElement.Q<Button>();
                button.focusable = true;
                button.text = "Exit";
                button.clicked += () => { _npcDialogue.EndDialogue(); };
                dialogueChoicesContainer.Add(choiceElement);
            }
            
            FocusOnFirstDialogueOption(dialogueChoicesContainer);

        }
        
        
        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in _currentDialogue.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }

        private void FocusOnFirstDialogueOption(VisualElement choicesContainer)
        {
            Button firstChoice = choicesContainer.Q<Button>();
            if (firstChoice != null)
            {
                firstChoice.Focus();
            }
        }
    }
}