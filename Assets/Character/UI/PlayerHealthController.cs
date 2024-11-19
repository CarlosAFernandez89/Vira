using GameplayAbilitySystem;
using GameplayAbilitySystem.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Character.UI
{
    public class PlayerHealthController : MonoBehaviour
    {
        GameObject _player;
        private GameObject[] _healthContainers;
        private Image[] _healthFills;

        public Transform healthParent;
        public GameObject healthPrefab;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            
            int maxHealth = 
                (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health").MaxValue;
            
            int currentHealth = 
                (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health").CurrentValue;
            
            _healthContainers = new GameObject[maxHealth];
            _healthFills = new Image[maxHealth];

            _player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health").OnValueChanged += UpdateHealthHUD;
            InstantiateHealthContainers();
            UpdateHealthHUD(currentHealth);
            
        }

        void SetHealthContainers()
        {
            for (int i = 0; i < _healthContainers.Length; i++)
            {
                if (i < (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health").MaxValue)
                {
                    _healthContainers[i].SetActive(true);
                }
                else
                {
                    _healthContainers[i].SetActive(false);
                }
            }
        }
        
        void SetHealthFills(float currentHealth)
        {
            for (int i = 0; i < _healthFills.Length; i++)
            {
                if (i < currentHealth)
                {
                    _healthFills[i].fillAmount = 1;
                }
                else
                {
                    _healthFills[i].fillAmount = 0;
                }
            }
        }

        void InstantiateHealthContainers()
        {
            for (int i = 0; i < (int)_player.GetComponent<AbilitySystemComponent>().attributesComponent.GetAttribute("Health").maxValue; i++)
            {
                GameObject temp = Instantiate(healthPrefab, healthParent, false);
                _healthContainers[i] = temp;
                _healthFills[i] = temp.transform.Find("HealthFill").GetComponent<Image>();
            }
        }

        void UpdateHealthHUD(float currentHealth)
        {
            SetHealthContainers();
            SetHealthFills(currentHealth);
        }
    }
}
