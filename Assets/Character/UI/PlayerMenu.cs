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
        void Initialize();
        void Deinitialize();
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
        
        // Store tab names and content
        private List<string> _tabNames = new List<string>();
        private List<VisualElement> _tabContent = new List<VisualElement>();
        
        private int _selectedTabIndex = 0;
        private int _lastSelectedTabIndex = 0;
        
        private InputAction _navigateAction;
        private InputAction _submitAction;
        private InputAction _nextTabAction;
        private InputAction _previousTabAction;
        private InputAction _closePlayerMenuAction;

        private float _defaultTimeScale;
        
        private Dictionary<int, IPlayerSubMenu> _subUIs = new Dictionary<int, IPlayerSubMenu>(); // Dictionary to hold sub-UI instances
        
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
        }
        
        private void InitializeUI()
        {
           
            if (charmsUIAsset != null)
            {

                VisualElement charmUIRoot = charmsUIAsset.Instantiate().Q<VisualElement>("CharmUI");
                
                int index = AddTab("Charms", charmUIRoot);
                
                charmsUIManager.Initialize();
                
                _subUIs.Add(index, charmsUIManager);
                
            }
            else
            {
                Debug.LogError("CharmUIAsset not assigned in the Inspector!");
            }
            
            // Add the map tab
            if (mapUIAsset != null)
            {
                VisualElement mapUIRoot = mapUIAsset.Instantiate().Q<VisualElement>("map-container");
                
                // Add the "Map" tab
                int mapTabIndex = AddTab("Map", mapUIRoot); // Capture the tab index

                // Pass the root element of the Map UI to the MapUIManager
                mapManager.InitializeMap(mapUIRoot);

                // Add MapUIManager to the dictionary of sub-UIs using the tab index as the key
                _subUIs.Add(mapTabIndex, mapManager);
            }
            else
            {
                Debug.LogError("MapUIAsset not assigned in the Inspector!");
            }
            
            
            // Initial tab selection
            SelectTab(_lastSelectedTabIndex);
            
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
        }
        
        private void EnableMenu()
        {
            InitializeUI();
            
            // Pause the game
            _defaultTimeScale = Time.timeScale;
            Time.timeScale = 0;
            
            // Show the UI
            _root.style.display = DisplayStyle.Flex;
        
            // Enable the Action Map
            inputActions.FindActionMap("Player").Disable();
            inputActions.FindActionMap("UI").Enable();
            BindInputActions();
            
            // Initialize the currently selected tab's sub-UI
            if (_subUIs.ContainsKey(_selectedTabIndex))
            {
                _subUIs[_selectedTabIndex]?.Initialize();
            }
        
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
            
            // Deinitialize the currently selected tab's sub-UI
            if (_subUIs.ContainsKey(_selectedTabIndex))
            {
                _subUIs[_selectedTabIndex]?.Deinitialize();
            }
        }
        
        private void SelectTab(int index)
        {
            // Deinitialize the previously selected tab's sub-UI (if any)
            if (_subUIs.ContainsKey(_selectedTabIndex))
            {
                _subUIs[_selectedTabIndex]?.Deinitialize();
            }

            _selectedTabIndex = index;
            _lastSelectedTabIndex = _selectedTabIndex;

            // Update tab names in the header
            _currentTabNameLabel.text = _tabNames[index];
            _previousTabButton.text = _tabNames[(index - 1 + _tabNames.Count) % _tabNames.Count];
            _nextTabButton.text = _tabNames[(index + 1) % _tabNames.Count];

            // Ensure no duplicate content is added
            if (!_contentViewport.Contains(_tabContent[index]))
            {
                _contentViewport.Clear();
                _contentViewport.Add(_tabContent[index]);
            }

            // Initialize the newly selected tab's sub-UI (if any)
            if (_subUIs.ContainsKey(_selectedTabIndex))
            {
                _subUIs[_selectedTabIndex]?.Initialize();
            }

            // Move focus to the next tab button
            _nextTabButton.Focus();
        }
        
        // Helper method to add tabs
        public int AddTab(string tabName, VisualElement content)
        {
            int newTabIndex = _tabNames.Count;
            _tabNames.Add(tabName);
            _tabContent.Add(content);
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
