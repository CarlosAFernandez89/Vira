using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UXML
{
    public class MainMenuController : MonoBehaviour
    {
        public VisualElement Root;
        
        public Button PlayButton;
        public Button OptionsButton;
        public Button AchievementsButton;
        public Button ExtrasButton;
        public Button QuitButton;

        [SerializeField] private string playGameScene = "SampleScene";
        
        private PlayerInput _playerInput;
        private void Awake()
        {
            Root = GetComponent<UIDocument>().rootVisualElement;
            _playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            PlayButton = Root.Q<Button>("PlayButton");
            PlayButton.clicked += OnPlayButtonClicked;
            
            OptionsButton = Root.Q<Button>("OptionsButton");
            OptionsButton.clicked += OnOptionsButtonClicked;
            
            AchievementsButton = Root.Q<Button>("AchievementsButton");
            AchievementsButton.clicked += OnAchievementsButtonClicked;
            
            ExtrasButton = Root.Q<Button>("ExtrasButton");
            ExtrasButton.clicked += OnExtrasButtonClicked;
            
            QuitButton = Root.Q<Button>("QuitButton");
            QuitButton.clicked += OnQuitButtonClicked;
        }

        private void Start()
        {
            if(_playerInput != null && _playerInput.currentActionMap.name != "UI")
                _playerInput.SwitchCurrentActionMap("UI");
            
            PlayButton.Focus();
        }

        private void OnDisable()
        {
            if(_playerInput != null)
                _playerInput.SwitchCurrentActionMap("Player");
        }

        private void OnPlayButtonClicked()
        {
            LoadingScreen.Instance.LoadScene(playGameScene);
        }

        private void OnOptionsButtonClicked()
        {
            Debug.Log("OnOptionsButtonClicked");
        }

        private void OnAchievementsButtonClicked()
        {
            Debug.Log("OnAchievementsButtonClicked");
        }

        private void OnExtrasButtonClicked()
        {
            Debug.Log("OnExtrasButtonClicked");
        }

        private void OnQuitButtonClicked()
        {
            Application.Quit();
            
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
        }
    }
}
