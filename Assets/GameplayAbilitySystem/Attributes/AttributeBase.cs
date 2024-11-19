using System;
using UnityEngine;

namespace GameplayAbilitySystem.Attributes
{
    public enum AttributeValue
    {
        Name,
        BaseValue,
        CurrentValue,
        MinValue,
        MaxValue
        
        // Add any other values as needed.
    }
    [Serializable]
    public class AttributeBase
    {
        public string name;  // Name of the attribute
        public float baseValue;  // Base value of the attribute
        public float currentValue;  // Current value of the attribute
        public float minValue; // Min value of the attribute
        public float maxValue; // Max value of the attribute
        
        public event Action<float> OnValueChanged;
        public event Action<float> OnMaxValueChanged;
        public event Action OnZeroReached;

        public float BaseValue
        {
            get => baseValue;
            set => baseValue = Mathf.Clamp(value, minValue, maxValue);
        }

        public float CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = Mathf.Clamp(value, MinValue, MaxValue); // Prevent going below zero
                OnValueChanged?.Invoke(currentValue);
                if (currentValue <= MinValue)
                {
                    OnZeroReached?.Invoke();
                }
            }
        }

        public float MinValue
        {
            get => minValue;
            set => minValue = Mathf.Clamp(value, value, maxValue);
        }

        public float MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = Mathf.Clamp(value, minValue, value);
                OnMaxValueChanged?.Invoke(maxValue);
            }
        }

        public AttributeBase(string name, float baseValue, float minValue, float maxValue)
        {
            this.name = name;
            this.baseValue = baseValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            currentValue = baseValue; // Initialize current value to base value
        }

        public AttributeBase()
        {
            this.name = string.Empty;
            this.baseValue = 0;
            this.minValue = 0;
            this.maxValue = 0;
            this.currentValue = this.baseValue;
        }


        // Method to modify the attribute by a certain amount
        public void ModifyBaseValue(float value)
        {
            baseValue += value;
        }
        public void ModifyCurrentValue(float amount)
        {
            CurrentValue += amount;
        }

        public void ModifyMaxValue(float amount)
        {
            MaxValue += amount;
        }

        public void ModifyMinValue(float amount)
        {
            MinValue += amount;
        }
        
        public void SetOnZeroReached(Action action)
        {
            OnZeroReached += action; // Allows external code to subscribe to this specific event
        }

        public void SetOnValueChanged(Action<float> action)
        {
            OnValueChanged += action;
        }
    }
}
