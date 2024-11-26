using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Character.Input
{
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance;
        private PlayerInput _playerInput;

        public bool IsGamePaused { get; private set; }

        private bool _pauseToggleLock = false;

        [SerializeField] private UIDocument uiDocument;
        private VisualElement _pauseMenuRoot;

        public Button ResumeButton;
        public Button MainMenuButton;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            
            _playerInput = GetComponent<PlayerInput>();
            
            if (uiDocument != null)
            {
                VisualElement pauseMenuRoot = uiDocument.rootVisualElement.Q<VisualElement>("PauseMenu");
                _pauseMenuRoot = pauseMenuRoot;
                if (_pauseMenuRoot != null)
                {
                    _pauseMenuRoot.style.display = DisplayStyle.None;
                }
            }
        }

        private void OnEnable()
        {
            if (_pauseMenuRoot != null)
            {
                ResumeButton = _pauseMenuRoot.Q<Button>("ResumeButton");
                ResumeButton.clicked += OnResumeButtonClicked;
                
                MainMenuButton = _pauseMenuRoot.Q<Button>("QuitButton");
                MainMenuButton.clicked += OnQuitButtonClicked;
            }
        }

        private void OnQuitButtonClicked()
        {
            NoContextResumeGame();
            LoadingScreen.Instance.LoadScene("MainMenu");
        }

        private void OnResumeButtonClicked()
        {
            NoContextResumeGame();
        }

        private void Start()
        {
            
            if (_playerInput.currentActionMap.name == "Player")
            {
                _playerInput.actions["PauseGame"].started += PauseGame;
            }
            else if (_playerInput.currentActionMap.name == "UI")
            {
                _playerInput.actions["PauseGame"].started += ResumeGame;
            }

        }

        private void PauseGame(InputAction.CallbackContext context)
        {
            if (_pauseToggleLock) return;

            IsGamePaused = true;
            Time.timeScale = 0;
            _pauseToggleLock = true;
            
            // Show Pause Menu
            if (_pauseMenuRoot != null)
            {
                _pauseMenuRoot.style.display = DisplayStyle.Flex;
                ResumeButton.Focus();
            }

            _playerInput.actions["PauseGame"].started -= PauseGame;
            _playerInput.SwitchCurrentActionMap("UI");
            _playerInput.actions["PauseGame"].started += ResumeGame;

            StartCoroutine(ResetPauseToggleLock());

        }

        private void ResumeGame(InputAction.CallbackContext context)
        {
            NoContextResumeGame();
        }

        private void NoContextResumeGame()
        {
            if (_pauseToggleLock) return;

            IsGamePaused = false;
            Time.timeScale = 1;
            _pauseToggleLock = true;
            
            // Hide Pause Menu
            if (_pauseMenuRoot != null)
            {
                _pauseMenuRoot.style.display = DisplayStyle.None;
            }

            _playerInput.actions["PauseGame"].started -= ResumeGame;
            _playerInput.SwitchCurrentActionMap("Player");
            _playerInput.actions["PauseGame"].started += PauseGame;

            StartCoroutine(ResetPauseToggleLock());
        }
        
        private IEnumerator ResetPauseToggleLock()
        {
            yield return new WaitForSecondsRealtime(0.1f); // Time-independent delay
            _pauseToggleLock = false;
        }
    }
}
