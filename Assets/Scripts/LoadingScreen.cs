using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;
using System.IO;
using System.Linq;

public class LoadingScreen : MonoBehaviour
{
    // Singleton instance
    public static LoadingScreen Instance { get; private set; }

    [Header("Loading Screen Settings")]
    public UIDocument loadingScreenUI;
    
    [Header("Background Image")]
    public Texture2D customBackgroundImage;
    public string randomImageFolderPath = "Assets/UXML/LoadingScreenBackgrounds";
    
    
    private VisualElement _rootElement;
    private VisualElement _loadingContainer;
    private Label _progressLabel;
    private Label _loadingLabel;

    [Header("Loading Customization")]
    [Range(0f, 3f)] 
    public float minLoadTime = 1f; // Minimum time to show loading screen

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup UI elements
        if (loadingScreenUI != null)
        {
            _rootElement = loadingScreenUI.rootVisualElement;
            _loadingContainer = _rootElement.Q<VisualElement>("loading-container");
            _progressLabel = _rootElement.Q<Label>("ProgressPercentage");
            _loadingLabel = _rootElement.Q<Label>("LoadingText");

            // Set background image
            SetBackgroundImage();

            // Hide the entire loading screen
            _rootElement.AddToClassList("hidden");
        }
    }
    
    private void SetBackgroundImage()
    {
        Debug.Log("Attempting to set background image");

        if (customBackgroundImage != null)
        {
            Debug.Log("Using custom background image");
            SetBackgroundFromTexture(customBackgroundImage);
        }
        else
        {
            Debug.Log("Trying to find random image");
            Texture2D randomImage = GetRandomImageFromFolder();
            if (randomImage != null)
            {
                Debug.Log("Found random image");
                SetBackgroundFromTexture(randomImage);
            }
            else
            {
                Debug.LogWarning("No background image found, using black background");
                _loadingContainer.style.backgroundColor = new StyleColor(Color.black);
            }
        }
    }
    
    private void SetBackgroundFromTexture(Texture2D texture)
    {
        if (_loadingContainer != null)
        {
            // Create a sprite from the texture
            var sprite = Sprite.Create(
                texture, 
                new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f)
            );

            // Apply background image using background-image style
            _loadingContainer.style.backgroundImage = new StyleBackground(sprite.texture);
        
            // Ensure the background covers the entire container
            _loadingContainer.style.backgroundSize = new StyleBackgroundSize((StyleKeyword)BackgroundSizeType.Cover);
            _loadingContainer.style.backgroundPositionX = new StyleBackgroundPosition();
            _loadingContainer.style.backgroundPositionY = new StyleBackgroundPosition();
        }
    }
    
    private Texture2D GetRandomImageFromFolder()
    {
        try
        {
            // Get all image files in the specified directory
            string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
            string[] imagePaths = Directory.GetFiles(randomImageFolderPath)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();

            if (imagePaths.Length > 0)
            {
                // Select a random image
                string randomImagePath = imagePaths[Random.Range(0, imagePaths.Length)];
                
                // Load texture
                byte[] fileData = File.ReadAllBytes(randomImagePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                return texture;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading random background image: {e.Message}");
        }

        return null;
    }

    private void Start()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Load a new scene with a loading screen
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    /// <summary>
    /// Restart the current scene with a loading screen
    /// </summary>
    public void RestartScene()
    {
        StartCoroutine(LoadSceneAsync(SceneManager.GetActiveScene().name));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Show loading screen
        if (_rootElement != null)
        {
            _rootElement.RemoveFromClassList("hidden");
        }

        // Start async operation
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Prevent scene activation until loading is complete
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = false;

            float elapsedTime = 0f;

            // Load progress tracking
            while (!asyncLoad.isDone)
            {
                // Calculate progress
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                // Update progress label if exists
                if (_progressLabel != null)
                    _progressLabel.text = $"{Mathf.RoundToInt(progress * 100)}%";

                // Update loading label if exists
                if (_loadingLabel != null)
                    _loadingLabel.text = "Loading...";

                // Track elapsed time
                elapsedTime += Time.deltaTime;

                // Allow scene activation once progress is at 90% and minimum load time is reached
                if (asyncLoad.progress >= 0.9f && elapsedTime >= minLoadTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        // Hide loading screen
        if (_rootElement != null)
        {
            _rootElement.AddToClassList("hidden");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Optional: Additional setup or logging when a scene is loaded
        Debug.Log($"Scene loaded: {scene.name}");
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}