using UnityEngine;
using UnityEngine.UIElements;

namespace UXML
{
    public class LoadingScreenUI : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _rootElement;
        private Label _progressLabel;
        private Label _loadingLabel;

        private void Awake()
        {
            // Get the UI Document component
            _uiDocument = GetComponent<UIDocument>();
        
            // Get the root visual element
            _rootElement = _uiDocument.rootVisualElement;

            // Find the progress and loading labels
            _progressLabel = _rootElement.Q<Label>("ProgressPercentage");
            _loadingLabel = _rootElement.Q<Label>("LoadingText");
        }

        public void UpdateProgress(float progress)
        {
            // Update progress percentage
            if (_progressLabel != null)
            {
                _progressLabel.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }
    }
}