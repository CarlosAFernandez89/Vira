using System.Collections.Generic;
using UnityEngine;

namespace GameplayAbilitySystem.Attributes
{
    [System.Serializable]
    public struct StatRange
    {
        public int baseValue;
        public int minValue;
        public int maxValue;
    }
    
    public class AttributesComponent : MonoBehaviour
    {
        [Header("Default Attributes")]
        [SerializeField] private StatRange defaultHealth = new StatRange { baseValue = 5, minValue = 0, maxValue = 5 };
        [SerializeField] private StatRange defaultMana = new StatRange { baseValue = 10, minValue = 0, maxValue = 10 };

        //TODO: Add a way to assign functions to SetOnZeroReached and SetOnValueChanged for additional attributes.

        [Header("Additional Attributes")]
        [SerializeField] public List<AttributeBase> attributes = new List<AttributeBase>();
        
        public void InitializeOnGameStart()
        {
            //AbilitySystemLogger.Log("Initializing attributes on the Game Start");
        }

        private void OnEnable()
        {
            // Initialize default attributes if not already set
            InitializeDefaultAttributes();
        }
        
        private void OnDisable()
        {
            // Probably do some saving logic here.
        }
        
        private void InitializeDefaultAttributes()
        {
            // These attributes can be added here or in the editor.
            if (GetAttribute("Health") == null)
            {
                AttributeBase health = new AttributeBase("Health", defaultHealth.baseValue, defaultHealth.minValue, defaultHealth.maxValue);
                health.SetOnZeroReached(OnHealthZero);
                health.SetOnValueChanged(OnHealthChanged);
                attributes.Add(health);
            }

            if (GetAttribute("Mana") == null)
            {
                AttributeBase mana = new AttributeBase("Mana", defaultMana.baseValue, defaultMana.minValue, defaultMana.maxValue);
                mana.SetOnZeroReached(OnManaZero);
                mana.SetOnValueChanged(OnManaChanged);
                attributes.Add(mana);
            }
            
            // Add more attributes as needed
            
            
            // Add the editor defined attributes
            foreach (AttributeBase attribute in attributes)
            {
                if (GetAttribute(attribute.name) == null)
                {
                    attributes.Add(attribute);
                }
            }
        }

        // Method to get an attribute by name
        public AttributeBase GetAttribute(string attributeName)
        {
            return attributes.Find(attr => attr.name == attributeName);
        }
        
        public float GetAttributeValue(string attributeName)
        {
            AttributeBase attribute = GetAttribute(attributeName);
            return attribute?.CurrentValue ?? 0f;
        }

        // Method to modify an attribute
        public void ModifyAttribute(string attributeName, float amount)
        {
            AttributeBase attribute = GetAttribute(attributeName);
            if (attribute != null)
            {
                attribute.ModifyCurrentValue(amount);
            }
        }
        
        private void OnHealthZero()
        {
            //AbilitySystemLogger.Log("Health has reached zero!");
            // Handle character death or game over logic
        }

        private void OnHealthChanged(float health)
        {
            //AbilitySystemLogger.Log("Health has changed to " + health + "!");
        }

        private void OnManaZero()
        {
            //AbilitySystemLogger.Log("Mana has reached zero!");
            // Handle mana depletion logic (e.g., disabling abilities that require mana)
        }

        private void OnManaChanged(float mana)
        {
            //AbilitySystemLogger.Log("Mana has changed to " + mana + "!");
        }
    }
}
