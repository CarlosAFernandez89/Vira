using System.Collections.Generic;
using Character.UI.Charms;
using Character.UI.Map;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Character.UI
{
    public interface IPlayerSubMenu
    {
        void InitializeSubMenu();
        void DeinitializeSubMenu();
    }

    public class PlayerMenu : MonoBehaviour
    {
        private bool _isInitialized = false;
        
        public UIDocument uiDocument; // Reference to the UI Document component
        public InputActionAsset inputActions;
        public MapUIManager mapManager;
        public VisualTreeAsset mapUIAsset;

        public CharmsUIManager charmsUIManager;
        public VisualTreeAsset charmsUIAsset;

        private VisualElement _root;
        private VisualElement _contentViewport;
        private Button[] _tabButtons;
        private VisualElement[] _tabPanels;

        private Button _previousTabButton;
        private Label _currentTabNameLabel;
        private Button _nextTabButton;

        // Store tab names
        private List<string> _tabNames = new List<string>();

        // Dictionary to track if a tab's content has been initialized
        private Dictionary<int, bool> _tabInitialized = new Dictionary<int, bool>();

        // Dictionary to hold VisualTreeAsset references for each tab
        private Dictionary<int, VisualTreeAsset> _tabUIAssets = new Dictionary<int, VisualTreeAsset>();

        private Dictionary<int, IPlayerSubMenu> _subUIs = new Dictionary<int, IPlayerSubMenu>(); // Dictionary to hold sub-UI instances

        private int _selectedTabIndex = 0;
        private int _lastSelectedTabIndex = 0;

        private InputAction _navigateAction;
        private InputAction _submitAction;
        private InputAction _nextTabAction;
        private InputAction _previousTabAction;
        private InputAction _closePlayerMenuAction;

        private float _defaultTimeScale;

        private void Awake()
        {
            // Get references from the Input Actions asset
            _navigateAction = inputActions.FindActionMap("UI").FindAction("Navigate");
            _submitAction = inputActions.FindActionMap("UI").FindAction("Submit");
            _nextTabAction = inputActions.FindActionMap("UI").FindAction("NextTab");
            _previousTabAction = inputActions.FindActionMap("UI").FindAction("PreviousTab");
            _closePlayerMenuAction = inputActions.FindActionMap("UI").FindAction("ClosePlayerMenu");

            // Set up UI elements
            _root = uiDocument.rootVisualElement;
            _contentViewport = _root.Q<VisualElement>("content-viewport");

            _previousTabButton = _root.Q<Button>("previous-tab-button");
            _currentTabNameLabel = _root.Q<Label>("current-tab-name");
            _nextTabButton = _root.Q<Button>("next-tab-button");

            _root.style.display = DisplayStyle.None;

            // Initialize tab data (but don't create content yet)
            InitializeTabData();
        }

        private void InitializeTabData()
        {
            // Charms Tab
            int charmsTabIndex = AddTab("Charms");
            _tabUIAssets.Add(charmsTabIndex, charmsUIAsset); // Store the asset
            _subUIs.Add(charmsTabIndex, charmsUIManager);

            // Map Tab
            int mapTabIndex = AddTab("Map");
            _tabUIAssets.Add(mapTabIndex, mapUIAsset); // Store the asset
            _subUIs.Add(mapTabIndex, mapManager);
            
        }

        private void OnDisable()
        {
            // Disable input actions when this script is disabled.
            inputActions.FindActionMap("UI").Disable();
        }

        public void TogglePlayerMenu()
        {
            if (_root != null && _root.style.display == DisplayStyle.None)
            {
                EnableMenu();
            }
            else
            {
                DisableMenu();
            }
        }

        private void BindInputActions()
        {
            // Register callbacks
            _previousTabButton.clicked += ClickSelectPreviousTab;
            _previousTabAction.performed += SelectPreviousTab;

            _nextTabButton.clicked += ClickSelectNextTab;
            _nextTabAction.performed += SelectNextTab;

            _closePlayerMenuAction.performed += DisableMenuAction;
        }

        private void UnbindInputActions()
        {
            _previousTabButton.clicked -= ClickSelectPreviousTab;
            _previousTabAction.performed -= SelectPreviousTab;

            _nextTabButton.clicked -= ClickSelectNextTab;
            _nextTabAction.performed -= SelectNextTab;

            _closePlayerMenuAction.performed -= DisableMenuAction;
        }

        private void EnableMenu()
        {
            // Pause the game
            _defaultTimeScale = Time.timeScale;
            Time.timeScale = 0;

            // Show the UI
            _root.style.display = DisplayStyle.Flex;

            // Enable the Action Map
            inputActions.FindActionMap("Player").Disable();
            inputActions.FindActionMap("UI").Enable();
            BindInputActions();

            // Select the initial tab (or previously selected tab)
            SelectTab(_lastSelectedTabIndex);

            // Set initial focus (optional, depending on your desired behavior)
            _nextTabButton.Focus();
        }

        private void DisableMenuAction(InputAction.CallbackContext context)
        {
            DisableMenu();
        }

        private void DisableMenu()
        {
            Time.timeScale = _defaultTimeScale;

            // Hide the UI
            _root.style.display = DisplayStyle.None;

            UnbindInputActions();
            // Disable the Action Map
            inputActions.FindActionMap("UI").Disable();
            inputActions.FindActionMap("Player").Enable();

            // Deinitialize the currently selected tab (if any)
            DeinitializeCurrentTab();
        }

        private void SelectTab(int index)
        {
            // Deinitialize the previously selected tab (if any)
            DeinitializeCurrentTab();

            _selectedTabIndex = index;
            _lastSelectedTabIndex = _selectedTabIndex;

            // Update tab names in the header
            _currentTabNameLabel.text = _tabNames[index];
            _previousTabButton.text = _tabNames[(index - 1 + _tabNames.Count) % _tabNames.Count];
            _nextTabButton.text = _tabNames[(index + 1) % _tabNames.Count];

            // Initialize the newly selected tab (if not already initialized)
            InitializeTabContent(index);

            // Move focus to the next tab button
            _nextTabButton.Focus();
        }

        private void DeinitializeCurrentTab()
        {
            if (_subUIs.ContainsKey(_selectedTabIndex) && _tabInitialized[_selectedTabIndex])
            {
                _subUIs[_selectedTabIndex]?.DeinitializeSubMenu();
                _contentViewport.Clear(); // Clear the content from the viewport
                _tabInitialized[_selectedTabIndex] = false;
            }
        }

        private void InitializeTabContent(int index)
        {
            if (!_tabInitialized[index])
            {
                // 1. Get the correct VisualTreeAsset for the tab
                VisualTreeAsset uiAsset = _tabUIAssets[index];

                // 2. Instantiate the content
                VisualElement content = null;
                if (index == 0) // Assuming 0 is the Charms tab
                {
                    content = uiAsset.Instantiate().Q<VisualElement>("CharmUI");
                    
                    // Check if content was found
                    if (content == null)
                    {
                        Debug.LogError($"Could not find root element 'CharmUI' in {uiAsset.name}. " +
                                       $"Check the name of the root element in your UXML file.");
                        return;
                    }
                }
                else if (index == 1) // Assuming 1 is the Map tab
                {
                    content = uiAsset.Instantiate().Q<VisualElement>("map-container");
                }
                
                _contentViewport.Add(content);

                // 3. Initialize the sub-UI (with a delay for Charms)
                if (index == 0 ) // Charms tab
                {
                    if (_subUIs.ContainsKey(index))
                    {
                        _subUIs[index].InitializeSubMenu();
                    }
                }
                else if (index == 1)
                {
                    mapManager.InitializeMap(content); // Pass the root element of the Map UI
                }

                // 4. Mark the tab as initialized
                _tabInitialized[index] = true;
            }
        }

        // Helper method to add tabs (only sets up the name)
        public int AddTab(string tabName)
        {
            int newTabIndex = _tabNames.Count;
            _tabNames.Add(tabName);
            _tabInitialized[newTabIndex] = false; // Initially not initialized
            return newTabIndex;
        }

        private void ClickSelectNextTab()
        {
            SelectTab((_selectedTabIndex + 1) % _tabNames.Count);
        }

        private void SelectNextTab(InputAction.CallbackContext context)
        {
            SelectTab((_selectedTabIndex + 1) % _tabNames.Count);
        }

        private void ClickSelectPreviousTab()
        {
            SelectTab((_selectedTabIndex - 1 + _tabNames.Count) % _tabNames.Count); // Handle negative wrapping
        }

        private void SelectPreviousTab(InputAction.CallbackContext context)
        {
            SelectTab((_selectedTabIndex - 1 + _tabNames.Count) % _tabNames.Count); // Handle negative wrapping
        }
    }
}