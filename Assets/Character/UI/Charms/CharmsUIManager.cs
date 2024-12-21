using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Character.Abilities.Charms;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Character.UI.Charms
{
    public class CharmsUIManager : MonoBehaviour, IPlayerSubMenu
    {
        [SerializeField] private int _maxEquippedSlots = 6;
        [SerializeField] public GameObject _player; // Reference to the player GameObject
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private VisualTreeAsset charmItemAsset; // Reference to CharmItemUI.uxml
        
        private VisualElement _root;
        
        private VisualElement _topHalf;
        private VisualElement _equippedCharmsContainer;
        
        private VisualElement _bottomHalf;
        private VisualElement _allCharmsContainer;
        
        private Label _charmNameLabel;
        private VisualElement _charmImage;
        private Label _charmDescriptionLabel;

        private CharmManager _charmManager; // Reference to the CharmManager on the player
        private List<CharmAbilityBase> _allCharms = new List<CharmAbilityBase>();
        private int _notchesUsed = 0;
        public int _maxNotches;
        
        private InputAction _navigateAction;
        private InputAction _submitAction;

        private void Awake()
        {
            _root = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("content-viewport");
            _navigateAction = inputActions.FindActionMap("UI").FindAction("Navigate");
            _submitAction = inputActions.FindActionMap("UI").FindAction("Submit");
        }

        private void Start()
        {
            _charmManager = _player.GetComponent<CharmManager>();
            _maxNotches = _charmManager.GetMaxCharmSlots();
        }

        public void InitializeSubMenu()
        {
            _topHalf = _root.Q<VisualElement>("EquippedCharms");
            _equippedCharmsContainer = _root.Q<VisualElement>("EquippedCharmsContainer");
            
            _bottomHalf = _root.Q<VisualElement>("BottomHalf");
            _allCharmsContainer = _root.Q<VisualElement>("AllCharmsContainer");
            
            _charmNameLabel = _root.Q<Label>("CharmName");
            _charmImage = _root.Q<VisualElement>("CharmImage");
            _charmDescriptionLabel = _root.Q<Label>("CharmDescription");
            
            BindInputActions();

            LoadAllCharms();

            InitializeEquippedCharmList();
            InitializeAllCharmList();
            
            ClearCharmInfo();
            
            FocusOnFirstEquippedCharm();
            
            _root.MarkDirtyRepaint();
        }

        public void DeinitializeSubMenu()
        {
            // Any cleanup when the UI is hidden.
            
            UnbindInputActions();
        }

        private void BindInputActions()
        {
            _submitAction.performed += OnSelect;
        }

        private void UnbindInputActions()
        {
            _submitAction.performed -= OnSelect;
        }

        private void LoadAllCharms()
        {
            // Load all CharmAbilityBase ScriptableObjects from the Resources/Charms folder
            _allCharms = Resources.LoadAll<CharmAbilityBase>("Charms").ToList();
        }

        private VisualElement CreateCharmItem(CharmAbilityBase charm, bool isEquipped = true)
        {
            if (charmItemAsset == null) return null;
            
            VisualElement charmItem = charmItemAsset.Instantiate();
            VisualElement cItem = charmItem.Q<VisualElement>("CharmItem");
            VisualElement imageSlot = cItem.Q<VisualElement>("ImageSlot");
            Button button = cItem.Q<Button>("Button");

            if (charm != null && imageSlot != null && button != null)
            {
                // Set charm image
                imageSlot.AddToClassList("charm-image");
                imageSlot.style.backgroundImage = new StyleBackground(charm.CharmInfo.Icon);

                // Add hover behavior
                charmItem.RegisterCallback<MouseEnterEvent>(evt => DisplayCharmInfo(charm));
                charmItem.RegisterCallback<MouseLeaveEvent>(evt => ClearCharmInfo());
                
                button.name = charm.CharmInfo.DisplayName;
                button.focusable = true;
                
                button.RegisterCallback<FocusEvent>(evt =>
                {
                    DisplayCharmInfo(charm);
                    
                });
                button.RegisterCallback<BlurEvent>(evt =>
                {
                    ClearCharmInfo();
                });
                
                // Add unequip button behavior
                button.clicked += () =>
                {
                    if (isEquipped)
                    {
                        UnequipCharm(charm);
                    }
                    else
                    {
                        EquipCharm(charm);
                    }
                    
                    RefreshUI(); // Refresh the UI to update the lists
                };
                
            }
            else
            {
                // Empty slot
                if (imageSlot != null)
                {
                    imageSlot.AddToClassList("empty-slot");
                }

                // Clear hover behavior
                charmItem.UnregisterCallback<MouseEnterEvent>(evt => { });
                charmItem.UnregisterCallback<MouseLeaveEvent>(evt => { });
                
                charmItem.UnregisterCallback<FocusEvent>(evt => { });
                charmItem.UnregisterCallback<BlurEvent>(evt => { });

                // Clear button behavior
                if (button != null) button.clicked += () => { };
            }

            return charmItem;
        }
        

        private void InitializeEquippedCharmList()
        {
            Label label = _topHalf.Q<Label>("TopLabel");
            label.text = "Equipped Charms";
            
            // Clear existing children
            _equippedCharmsContainer.Clear();

            // Get equipped charms from CharmManager
            List<CharmAbilityBase> equippedCharms = _charmManager.GetActiveCharmAbilities();

            // Add equipped charms
            for (int i = 0; i < _maxEquippedSlots; i++)
            {
                CharmAbilityBase charm = i < equippedCharms.Count ? equippedCharms[i] : null;
                _equippedCharmsContainer.Add(CreateCharmItem(charm));
            }
        }

        private void InitializeAllCharmList()
        {
            Label label = _bottomHalf.Q<Label>("BottomLabel");
            label.text = "Owned Charms";
            
            _allCharmsContainer.Clear();
            
            List<CharmAbilityBase> allCharms = _charmManager.GetOwnedCharmAbilities();

            for (int i = 0; i < _allCharms.Count; i++)
            {
                CharmAbilityBase charm = i < allCharms.Count ? allCharms[i] : null;
                bool equipped = _charmManager.GetActiveCharmAbilities().Contains(allCharms[i]);
                _allCharmsContainer.Add(CreateCharmItem(charm, equipped));
            }
        }

        private void EquipCharm(CharmAbilityBase charm)
        {
            // 1. Check if the player has enough notches.
            if (_notchesUsed + charm.CharmInfo.EquipCost > _maxNotches)
            {
                Debug.Log("Not enough notches to equip this charm!");
                return;
            }

            // 2. Add the charm to the CharmManager's active charms list.
            _charmManager.ActivateOwnedCharmAbility(charm.CharmInfo.DisplayName);

            // 3. Refresh the UI
            RefreshUI();
        }

        private void UnequipCharm(CharmAbilityBase charm)
        {
            // 1. Remove the charm from the CharmManager's active charms list.
            _charmManager.DeactivateOwnedCharmAbility(charm.CharmInfo.DisplayName);

            // 2. Refresh the UI
            RefreshUI();
        }

        private void DisplayCharmInfo(CharmAbilityBase charm)
        {
            // Update the charm name
            _charmNameLabel.text = charm.CharmInfo.DisplayName;

            // Update the charm image
            if (charm.CharmInfo.Icon != null)
            {
                _charmImage.style.backgroundImage = new StyleBackground(charm.CharmInfo.Icon);
            }

            // Update the charm description
            _charmDescriptionLabel.text = charm.CharmInfo.Description;
        }

        private void ClearCharmInfo()
        {
            _charmNameLabel.text = "";
            _charmImage.style.backgroundImage = null;
            _charmDescriptionLabel.text = "";
        }

        // Method to refresh the UI (call this if you change equipped/owned charms outside of this UI)
        public void RefreshUI()
        {
            _notchesUsed = _charmManager.GetActiveCharmAbilities().Sum(c => c.CharmInfo.EquipCost);
            InitializeEquippedCharmList();
            InitializeAllCharmList();
            FocusOnFirstEquippedCharm();
        }
        
        private void FocusOnFirstEquippedCharm()
        {
            if (_equippedCharmsContainer.childCount > 0)
            {
                _root.schedule.Execute(() =>
                {
                    foreach (VisualElement charmItem in _equippedCharmsContainer.Children())
                    {
                        Button buttonInCharmItem = charmItem.Q<Button>();
                        if (buttonInCharmItem != null)
                        {
                            buttonInCharmItem.focusable = true;
                            buttonInCharmItem.Focus();
                            Debug.Log($"Focused Element: {_root.focusController.focusedElement}");
                            return;
                        }
                    }
                });
            }
        }
        
        private void OnSelect(InputAction.CallbackContext context)
        {
            // Get the currently focused element
            Focusable focusedElement = _root.focusController.focusedElement;

            if (focusedElement is Button button)
            {
                // Trigger the click event directly on the focused button
                button.SendEvent(new ClickEvent());
                button.SendEvent(new BlurEvent());
            }
            else if (focusedElement is VisualElement visualElement)
            {
                // Try to find a button within the focused VisualElement
                Button buttonInElement = visualElement.Q<Button>();
                if (buttonInElement != null)
                {
                    // Trigger the click event on the found button
                    buttonInElement.SendEvent(new ClickEvent());
                    buttonInElement.SendEvent(new BlurEvent());
                }
            }
            
            ClearCharmInfo();
        }
    }
}