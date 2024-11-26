using GameplayAbilitySystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace UXML
{
    public class HUDController : MonoBehaviour
    {
        private UIDocument _hudDocument;
        private VisualElement _healthCirclesContainer, _manaCirclesContainer;
        private VisualElement[] _healthCircles, _manaCircles;
        
        GameObject _player;
        
        private int _maxHealth;
        private int _currentHealth;
        private int _maxMana;
        private int _currentMana;

        private void Start()
        {
            _hudDocument = GetComponent<UIDocument>();
            _healthCirclesContainer = _hudDocument.rootVisualElement.Q<VisualElement>("health-circles");
            _manaCirclesContainer = _hudDocument.rootVisualElement.Q<VisualElement>("mana-circles");

            SetPlayerReferences();
            
            CreateHealthCircles();
            CreateManaCircles();
            
            UpdateHealthCircles();
            UpdateManaCircles();
        }
        
        private void CreateHealthCircles()
        {
            _healthCircles = new VisualElement[_maxHealth];
            _healthCirclesContainer.Clear();

            for (int i = 0; i < _maxHealth; i++)
            {
                var healthCircle = new VisualElement();
                healthCircle.AddToClassList("health-circle");
                _healthCirclesContainer.Add(healthCircle);
                _healthCircles[i] = healthCircle;
            }
        }
        
        private void CreateManaCircles()
        {
            _manaCircles = new VisualElement[_maxMana];
            _manaCirclesContainer.Clear();

            for (int i = 0; i < _maxMana; i++)
            {
                var manaCircle = new VisualElement();
                manaCircle.AddToClassList("mana-circle");
                _manaCirclesContainer.Add(manaCircle);
                _manaCircles[i] = manaCircle;
            }
        }

        private void SetPlayerReferences()
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_player != null)
            {
                _currentHealth = (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent
                    .GetAttribute("Health").CurrentValue;
                _player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health")
                    .OnValueChanged += OnHealthChanged;
                
                _maxHealth = (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health")
                    .MaxValue;
                _player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health")
                    .OnMaxValueChanged += OnMaxHealthChanged;
                
                _currentMana = (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent
                    .GetAttribute("Mana").CurrentValue;
                _player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Mana")
                    .OnValueChanged += OnManaChanged;
                
                _maxMana = (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Mana")
                    .MaxValue;
                _player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Mana")
                    .OnMaxValueChanged += OnMaxManaChanged;
            }
        }

        private void OnHealthChanged(float newHealth)
        {
            _currentHealth = (int)newHealth;
            UpdateHealthCircles();
        }

        private void OnMaxHealthChanged(float newMaxHealth)
        {
            _maxHealth = (int)newMaxHealth;
            CreateHealthCircles();
        }

        private void OnManaChanged(float newMana)
        {
            _currentMana = (int)newMana;
            UpdateManaCircles();
        }

        private void OnMaxManaChanged(float newMaxMana)
        {
            _maxMana = (int)newMaxMana;
            CreateManaCircles();
        }

        private void UpdateHealthCircles()
        {
            // Update the visibility and styling of the health circles
            for (int i = 0; i < _maxHealth; i++)
            {
                _healthCircles[i].style.backgroundColor = i < (_maxHealth - _currentHealth) ? Color.clear : Color.green;
            }
        }

        private void UpdateManaCircles()
        {
            // Update the visibility and styling of the mana circles
            for (int i = 0; i < _maxMana; i++)
            {
                _manaCircles[i].style.backgroundColor = i < _currentMana ? Color.blue : Color.clear;
            }
        }
        
    }
}
