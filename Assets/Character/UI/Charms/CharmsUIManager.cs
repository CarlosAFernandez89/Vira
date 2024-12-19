using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Character.Abilities.Charms;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Character.UI.Charms
{
    public class CharmsUIManager : MonoBehaviour, IPlayerSubMenu
    {
        [SerializeField] private int _maxEquippedSlots = 6;
        [SerializeField] public GameObject _player; // Reference to the player GameObject

        [SerializeField] private VisualTreeAsset _charmItemAsset; // Reference to CharmItem.uxml

        private VisualElement _root;
        private ListView _equippedCharmList;
        private ListView _allCharmsList;
        private Label _charmNameLabel;
        private VisualElement _charmImage;
        private Label _charmDescriptionLabel;

        private CharmManager _charmManager; // Reference to the CharmManager on the player
        private List<CharmAbilityBase> _allCharms = new List<CharmAbilityBase>();
        private int _notchesUsed = 0;
        public int _maxNotches;

        private void Awake()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _equippedCharmList = _root.Q<ListView>("EquippedCharmList");
            _allCharmsList = _root.Q<ListView>("AllCharmsList");
            _charmNameLabel = _root.Q<Label>("CharmName");
            _charmImage = _root.Q<VisualElement>("CharmImage");
            _charmDescriptionLabel = _root.Q<Label>("CharmDescription");

            _root.style.display = DisplayStyle.None;
        }

        private void Start()
        {
            _charmManager = _player.GetComponent<CharmManager>();
            _maxNotches = _charmManager.GetMaxCharmSlots();

            LoadAllCharms();

            InitializeEquippedCharmList();
            InitializeAllCharmsList();
        }

        public void Initialize()
        {
            RefreshUI();
        }

        public void Deinitialize()
        {
            // Any cleanup when the UI is hidden.
        }

        private void LoadAllCharms()
        {
            // Load all CharmAbilityBase ScriptableObjects from the Resources/Charms folder
            _allCharms = Resources.LoadAll<CharmAbilityBase>("Charms").ToList();
        }

        private void InitializeEquippedCharmList()
        {
            _equippedCharmList.fixedItemHeight = 64;
            _equippedCharmList.itemsSource = _charmManager.GetActiveCharmAbilities();
            _equippedCharmList.makeItem = MakeEquippedCharmItem;
            _equippedCharmList.bindItem = BindEquippedCharmItem;

            // Refresh the ListView
            _equippedCharmList.Rebuild();
            Debug.Log($"Equipped ItemsSource.Count:{_equippedCharmList.itemsSource.Count}");
        }

        private VisualElement MakeEquippedCharmItem()
        {
            // Instantiate a new CharmItem 
            VisualElement charmItem = _charmItemAsset.Instantiate();
            return charmItem;
        }

        private void BindEquippedCharmItem(VisualElement element, int i)
        {
            CharmAbilityBase charm = _charmManager.GetActiveCharmAbilities()[i];

            VisualElement imageSlot = element.Q<VisualElement>("ImageSlot");
            Button button = element.Q<Button>("Button");
            button.clicked += () => { };

            if (charm != null)
            {
                // Update the imageSlot with the charm's data
                imageSlot.RemoveFromClassList("empty-slot");
                imageSlot.AddToClassList("charm-image");
                imageSlot.style.backgroundImage = new StyleBackground(charm.CharmInfo.Icon);

                // Add hover behavior
                element.RegisterCallback<MouseEnterEvent>(evt => DisplayCharmInfo(charm));
                element.RegisterCallback<MouseLeaveEvent>(evt => ClearCharmInfo());
                
                button.clicked += () =>
                {
                    UnequipCharm(charm);
                    RefreshUI(); // Refresh the UI to update the lists
                };
            }
            else
            {
                // Clear data and set as empty slot
                imageSlot.RemoveFromClassList("charm-image");
                imageSlot.AddToClassList("empty-slot");
                imageSlot.style.backgroundImage = null;

                // Clear hover behavior
                element.UnregisterCallback<MouseEnterEvent>(evt => { });
                element.UnregisterCallback<MouseLeaveEvent>(evt => { });
            }
        }

        private void InitializeAllCharmsList()
        {
            // Set the item height for the ListView
            _allCharmsList.fixedItemHeight = 64;

            // Assign the methods to the ListView
            _allCharmsList.makeItem = MakeCharmItem;
            _allCharmsList.bindItem = BindCharmItem;

            // Set the list of items to display
            _allCharmsList.itemsSource = _charmManager.GetOwnedCharmAbilities();

            // Refresh the ListView
            _allCharmsList.Rebuild();
            Debug.Log($"All ItemsSource.Count:{_allCharmsList.itemsSource.Count}");

        }

        // Function to make a charm item for the All Charms list
        private VisualElement MakeCharmItem()
        {
            VisualElement charmItem = _charmItemAsset.Instantiate();
            charmItem.Q<VisualElement>("ImageSlot").AddToClassList("empty-slot");
            return charmItem;
        }

        // Function to bind data to a charm item in the All Charms list
        private void BindCharmItem(VisualElement e, int i)
        {
            VisualElement imageSlot = e.Q<VisualElement>("ImageSlot");
            Button button = e.Q<Button>("Button");
            button.clicked += () => { }; // Reset the clicked event

            CharmAbilityBase charm = _charmManager.GetOwnedCharmAbilities()[i];
            if (charm != null)
            {
                // Owned charm
                imageSlot.RemoveFromClassList("empty-slot");

                imageSlot.AddToClassList(charm._equipped ? "equipped-charm" : "charm-image");

                imageSlot.style.backgroundImage = new StyleBackground(charm.CharmInfo.Icon);

                button.clicked += () =>
                {
                    if (charm._equipped)
                    {
                        UnequipCharm(charm);
                    }
                    else
                    {
                        EquipCharm(charm);
                    }
                };

                // Add hover behavior
                e.RegisterCallback<MouseEnterEvent>(evt => DisplayCharmInfo(charm));
                e.RegisterCallback<MouseLeaveEvent>(evt => ClearCharmInfo());
            }
            else
            {
                // Empty slot (shouldn't normally happen)
                imageSlot.RemoveFromClassList("charm-image");
                imageSlot.RemoveFromClassList("equipped-charm");
                imageSlot.AddToClassList("empty-slot");

                imageSlot.style.backgroundImage = null;

                // Clear hover behavior
                e.UnregisterCallback<MouseEnterEvent>(evt => { });
                e.UnregisterCallback<MouseLeaveEvent>(evt => { });
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
            _charmManager.ActivateOwnedCharmAbility(charm.name);

            // 3. Refresh the UI
            RefreshUI();
        }

        private void UnequipCharm(CharmAbilityBase charm)
        {
            // 1. Remove the charm from the CharmManager's active charms list.
            _charmManager.DeactivateOwnedCharmAbility(charm.name);

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
            _equippedCharmList.Rebuild();
            _allCharmsList.Rebuild();
        }
    }
}