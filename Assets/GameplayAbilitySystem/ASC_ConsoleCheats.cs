using System;
using System.Collections.Generic;
using GameplayAbilitySystem.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayAbilitySystem
{
    public class ASC_ConsoleCheats : MonoBehaviour
    {
        public GameObject consoleUI;       // A UI panel to show/hide the console
        public InputField inputField;      // An input field to type commands
        public Text outputText;            // A text field to display console output

        private Dictionary<string, Action<string[]>> _commands;

        private void Awake()
        {
            consoleUI.SetActive(false);
        
            // Register available commands
            _commands = new Dictionary<string, Action<string[]>>();
            _commands.Add("print_attributes", PrintAttributes);
            _commands.Add("set_attribute", SetAttribute);
            _commands.Add("player_heal", HealPlayer);
            _commands.Add("player_damage", DamagePlayer);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote)) // Toggle console with ` key
            {
                consoleUI.SetActive(!consoleUI.activeSelf);
                if (consoleUI.activeSelf)
                    inputField.ActivateInputField();
            }
        
            if (consoleUI.activeSelf && Input.GetKeyDown(KeyCode.Return)) // Execute command with Enter key
            {
                string commandInput = inputField.text;
                ExecuteCommand(commandInput);
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }

        private void ExecuteCommand(string commandInput)
        {
            string[] splitCommand = commandInput.Split(' ');
            string commandName = splitCommand[0];
            string[] args = splitCommand.Length > 1 ? splitCommand[1..] : new string[0];

            if (_commands.ContainsKey(commandName))
            {
                _commands[commandName].Invoke(args);
            }
            else
            {
                outputText.text += $"\nUnknown command: {commandName}";
            }
        }

        // Command to print all player attributes
        private void PrintAttributes(string[] args)
        {
            outputText.text = "";
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                outputText.text += "\nPlayer not found!";
                return;
            }

            AbilitySystemComponent asc = player.GetComponent<AbilitySystemComponent>();
            if (asc == null)
            {
                outputText.text += "\nPlayer does not have AbilitySystemComponent!";
                return;
            }

            outputText.text += "\nPlayer Attributes:";
            foreach (var attribute in asc.attributesComponent.attributes)
            {
                outputText.text += $"\n{attribute.name}: {attribute.currentValue}";
            }
        }

        // Command to set an attribute value
        private void SetAttribute(string[] args)
        {
            outputText.text = "";
        
            if (args.Length < 2)
            {
                outputText.text += "\nUsage: set_attribute <attribute_name> <value>";
                return;
            }

            string attributeName = args[0];
            if (!float.TryParse(args[1], out float newValue))
            {
                outputText.text += "\nInvalid value!";
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                outputText.text += "\nPlayer not found!";
                return;
            }

            AbilitySystemComponent asc = player.GetComponent<AbilitySystemComponent>();
            if (asc == null)
            {
                outputText.text += "\nPlayer does not have AbilitySystemComponent!";
                return;
            }

            AttributeBase attribute = asc.attributesComponent.GetAttribute(attributeName);
            if (attribute != null)
            {
                attribute.ModifyCurrentValue(newValue - attribute.currentValue); // Adjust the attribute to new value
                outputText.text += $"\n{attributeName} set to {newValue}";
            }
            else
            {
                outputText.text += $"\nAttribute {attributeName} not found!";
            }
        }
    
        private void HealPlayer(string[] args)
        {
            outputText.text = "";
        
            if (args.Length < 2)
            {
                outputText.text += "\nUsage: player_heal <attribute_name> <value>";
                return;
            }

            string attributeName = args[0];
            if (!float.TryParse(args[1], out float healAmount ))
            {
                outputText.text += "\nInvalid value!";
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                outputText.text += "\nPlayer not found!";
                return;
            }

            AbilitySystemComponent asc = player.GetComponent<AbilitySystemComponent>();
            if (asc == null)
            {
                outputText.text += "\nPlayer does not have AbilitySystemComponent!";
                return;
            }

            AttributeBase attribute = asc.attributesComponent.GetAttribute(attributeName);
            if (attribute != null)
            {
                attribute.ModifyCurrentValue(healAmount); // Adjust the attribute to new value
                outputText.text += $"\n{attributeName} set to {attribute.CurrentValue}";
            }
            else
            {
                outputText.text += $"\nAttribute {attributeName} not found!";
            }
        }
    
        private void DamagePlayer(string[] args)
        {
            outputText.text = "";
        
            if (args.Length < 2)
            {
                outputText.text += "\nUsage: player_heal <attribute_name> <value>";
                return;
            }

            string attributeName = args[0];
            if (!float.TryParse(args[1], out float damageAmount ))
            {
                outputText.text += "\nInvalid value!";
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                outputText.text += "\nPlayer not found!";
                return;
            }

            AbilitySystemComponent asc = player.GetComponent<AbilitySystemComponent>();
            if (asc == null)
            {
                outputText.text += "\nPlayer does not have AbilitySystemComponent!";
                return;
            }

            AttributeBase attribute = asc.attributesComponent.GetAttribute(attributeName);
            if (attribute != null)
            {
                attribute.ModifyCurrentValue(-damageAmount); // Adjust the attribute to new value
                outputText.text += $"\n{attributeName} set to {attribute.CurrentValue}";
            }
            else
            {
                outputText.text += $"\nAttribute {attributeName} not found!";
            }
        }
    }
}
