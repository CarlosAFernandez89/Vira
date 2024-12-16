using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Character.UI
{
    public class PlayerMenu : MonoBehaviour
    {
        public UIDocument uiDocument; // Reference to the UI Document component
        public InputActionAsset inputActions;
        
        private VisualElement _root;
        private VisualElement _tabContainer;
        private Button[] _tabButtons;
        private VisualElement[] _tabPanels;
        
        private Button _previousTabButton;
        private Label _currentTabNameLabel;
        private Button _nextTabButton;
        private ScrollView _tabContentScrollView;
        
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
        
        private void Awake()
        {
            // Get references from the Input Actions asset
            _navigateAction = inputActions.FindActionMap("UI").FindAction("Navigate");
            _submitAction = inputActions.FindActionMap("UI").FindAction("Submit");
            _nextTabAction = inputActions.FindActionMap("UI").FindAction("NextTab");
            _previousTabAction = inputActions.FindActionMap("UI").FindAction("PreviousTab");
            _closePlayerMenuAction = inputActions.FindActionMap("UI").FindAction("ClosePlayerMenu");
            
            // Initialize UI but keep it hidden initially
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // Set up UI elements
            _root = uiDocument.rootVisualElement;
            _tabContainer = _root.Q<VisualElement>("tab-container");
        
            _previousTabButton = _root.Q<Button>("previous-tab-button");
            _currentTabNameLabel = _root.Q<Label>("current-tab-name");
            _nextTabButton = _root.Q<Button>("next-tab-button");
            _tabContentScrollView = _root.Q<ScrollView>("tab-content");
        
            // Add some sample tabs and content (replace with your actual content loading)
            AddTab("Charms", new Label("Content of Charms"));
        
            Button tab2Button = new Button
            {
                text = "Button in Map"
            };
        
            AddTab("Map", tab2Button);
        
            AddTab("Lore", new Label("Content of Lore"));
            
            // Initial tab selection
            SelectTab(_lastSelectedTabIndex);
            
            _root.style.display = DisplayStyle.None;
        }
        
        private void OnDisable()
        {
            // Disable input actions when this script is disabled.
            inputActions.FindActionMap("UI").Disable();
        }
        
        public void TogglePlayerMenu()
        {
            if (_root == null)
            {
                InitializeUI();
            }
            
            
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
            // Pause the game
            _defaultTimeScale = Time.timeScale;
            Time.timeScale = 0;
            
            // Show the UI
            _root.style.display = DisplayStyle.Flex;
        
            // Enable the Action Map
            inputActions.FindActionMap("Player").Disable();
            inputActions.FindActionMap("UI").Enable();
            BindInputActions();
        
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
        }
        
        private void SelectTab(int index)
        {
            _selectedTabIndex = index;
            _lastSelectedTabIndex = _selectedTabIndex;
        
            // Update tab names in the header
            _currentTabNameLabel.text = _tabNames[index];
            _previousTabButton.text = _tabNames[(index - 1 + _tabNames.Count) % _tabNames.Count];
            _nextTabButton.text = _tabNames[(index + 1) % _tabNames.Count];
        
            // Clear existing content and add new content
            _tabContentScrollView.Clear();
            _tabContentScrollView.Add(_tabContent[index]);
        
            // Move focus to the next tab button
            _nextTabButton.Focus();
        }
        
        // Helper method to add tabs
        public void AddTab(string tabName, VisualElement content)
        {
            _tabNames.Add(tabName);
            _tabContent.Add(content);
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
