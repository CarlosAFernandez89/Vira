using System;
using GameplayAbilitySystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace Character.UI
{
    public class PlayerManaController : MonoBehaviour
    {
        GameObject _player;
        
        private float _currentMana;
        private float _maxMana;
        public float manaPercentage;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            _currentMana = (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Mana").CurrentValue;
            _player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Mana").OnValueChanged += OnManaChanged;
            
            _maxMana = (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Mana").MaxValue;
            _player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Mana").OnMaxValueChanged += OnMaxManaChanged;
            
            manaPercentage = _currentMana / _maxMana;
        }
        
        private void OnManaChanged(float newMana)
        {
            _currentMana = newMana;
            manaPercentage = _currentMana / _maxMana;
        }

        private void OnMaxManaChanged(float newMaxMana)
        {
            _maxMana = newMaxMana;
            manaPercentage = _currentMana / _maxMana;
        }

        private void OnEnable()
        {
            VisualElement root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            root.Q<PlayerMana>().dataSource = this;
        }
    }
}
