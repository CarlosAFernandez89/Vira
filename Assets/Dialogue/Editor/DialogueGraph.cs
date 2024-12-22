using System;
using System.IO;
using System.Linq;
using Dialogue.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor
{
    public class DialogueGraph : EditorWindow
    {
        private string _fileName = "NewFile";
        private string _newFileNameTemp = "NewFile";
        private const string _folderPath = "Assets/Resources/Dialogue";
        
        private DialogueGraphView _graphView;
        private DialogueContainer _dialogueContainer;

        [MenuItem("Graph/Narrative Graph")]
        public static void CreateGraphViewWindow()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Narrative Graph");
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogueGraphView(this)
            {
                name = "Narrative Graph",
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
        
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            // Get existing file names
            string[] fileNames = GetDialogueFileNames();

            // Create dropdown field
            var popupField = new PopupField<string>("Dialogue", fileNames.ToList(), -1);

            popupField.SetValueWithoutNotify("Select a File"); // Set to placeholder
            
            popupField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "Select a File")
                {
                    // Handle case where placeholder is selected (optional)
                    // You might want to reset _fileName to "New Narrative" or do nothing
                }
                else
                {
                    _fileName = Path.GetFileNameWithoutExtension(evt.newValue);
                    Debug.Log($"Dialogue file name: {_fileName}");
                }
            });

            toolbar.Add(popupField);
            
            // Create New File Button
            var newFileButton = new Button(() => ShowNewFileDialogue(popupField)) { text = "New" };
            toolbar.Add(newFileButton);

            toolbar.Add(new Button(() => RequestDataOperation(true)) {text = "Save Data"});

            toolbar.Add(new Button(() => RequestDataOperation(false)) {text = "Load Data"});
            
            // Add a label to display the currently selected file name
            var fileNameLabel = new Label("");
            toolbar.Add(fileNameLabel);
            
            rootVisualElement.Add(toolbar);
            
            UpdateFileNameLabel();

        }
        
        private void ShowNewFileDialogue(PopupField<string> popupField)
        {
            // Create a new window for entering the new file name
            var newFileNameWindow = GetWindow<NewFileNameWindow>(true, "New Dialogue File", true);
            newFileNameWindow.Initialize(_folderPath, (newFileName) =>
            {
                // Callback when the new file name is confirmed
                _newFileNameTemp = newFileName;

                GraphSaveUtility.GetInstance(_graphView).ClearGraph();

                // Save the new graph file immediately
                var saveUtility = GraphSaveUtility.GetInstance(_graphView);
                saveUtility.SaveGraph(_newFileNameTemp);

                // Update the current file name and reset temporary file name
                _fileName = _newFileNameTemp;
                _newFileNameTemp = string.Empty;

                // Refresh the dropdown to include the new file
                UpdatePopupField(popupField);

                Debug.Log($"Created and selected new dialogue file: {_fileName}");
            });
        }
        
        private void UpdatePopupField(PopupField<string> popupField)
        {
            // Get existing file names
            string[] fileNames = GetDialogueFileNames();

            // Update the choices in the dropdown
            popupField.choices = fileNames.ToList();

            // Set the dropdown value to the current file name if it exists
            if (!string.IsNullOrEmpty(_fileName) && fileNames.Contains(_fileName + ".asset"))
            {
                popupField.SetValueWithoutNotify(_fileName + ".asset");
            }
            else
            {
                // Reset selection if the current file does not exist
                popupField.SetValueWithoutNotify("Select a File");
                _fileName = string.Empty;
            }
        }
        
        private string[] GetDialogueFileNames()
        {
            // Ensure the directory exists
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }

            // Get all file names with the .asset extension in the save folder
            string[] guids = AssetDatabase.FindAssets("t:DialogueContainer", new[] { _folderPath });
            string[] filePaths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();

            // Filter and format the file names for the dropdown
            string[] formattedFileNames = filePaths.Select(path =>
            {
                string relativePath = path.Replace(_folderPath + "/", ""); // Remove folder path
                return Path.GetFileName(relativePath); // Return file name with extension
            }).ToArray();

            return formattedFileNames;
        }
        
        private void UpdateFileNameLabel()
        {
            // Find the label in the toolbar and update its text
            var toolbar = rootVisualElement.Q<Toolbar>();
            var fileNameLabel = toolbar.Q<Label>();
            if (fileNameLabel != null)
            {
                fileNameLabel.text = $"Current File: {_fileName}";
            }
        }

        private void RequestDataOperation(bool save)
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                _fileName = _fileName.Trim();
                var saveUtility = GraphSaveUtility.GetInstance(_graphView);
                if (save)
                    saveUtility.SaveGraph(_fileName);
                else
                    saveUtility.LoadNarrative(_fileName);
            }
            else if (!string.IsNullOrEmpty(_newFileNameTemp))
            {
                _newFileNameTemp = _newFileNameTemp.Trim();
                var saveUtility = GraphSaveUtility.GetInstance(_graphView);
                if (save)
                {
                    saveUtility.SaveGraph(_newFileNameTemp);
                    _fileName = _newFileNameTemp;
                    _newFileNameTemp = string.Empty;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid File name", "Please Enter a valid filename", "OK");
            }
            
            UpdateFileNameLabel();
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            GenerateMiniMap();
            GenerateBlackBoard();
        }

        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap {anchored = true};
            var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            _graphView.Add(miniMap);
        }

        private void GenerateBlackBoard()
        {
            var blackboard = new Blackboard(_graphView);
            blackboard.Add(new BlackboardSection {title = "Exposed Variables"});
            blackboard.addItemRequested = _blackboard =>
            {
                _graphView.AddPropertyToBlackBoard(ExposedProperty.CreateInstance(), false);
            };
            blackboard.editTextRequested = (_blackboard, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField) element).text;
                if (_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one.",
                        "OK");
                    return;
                }

                var targetIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
                _graphView.ExposedProperties[targetIndex].PropertyName = newValue;
                ((BlackboardField) element).text = newValue;
            };
            blackboard.SetPosition(new Rect(10,30,200,300));
            _graphView.Add(blackboard);
            _graphView.Blackboard = blackboard;
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
    
    // New window for entering new file name
    public class NewFileNameWindow : EditorWindow
    {
        private string _newFileName = "";
        private string _folderPath;
        private Action<string> _onConfirm;

        public void Initialize(string folderPath, Action<string> onConfirm)
        {
            _folderPath = folderPath;
            _onConfirm = onConfirm;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Enter New File Name:");
            _newFileName = EditorGUILayout.TextField(_newFileName);

            if (GUILayout.Button("Create"))
            {
                if (string.IsNullOrEmpty(_newFileName))
                {
                    EditorUtility.DisplayDialog("Error", "File name cannot be empty.", "OK");
                }
                else if (File.Exists(Path.Combine(_folderPath, _newFileName + ".asset")))
                {
                    EditorUtility.DisplayDialog("Error", "File with this name already exists.", "OK");
                }
                else
                {
                    _onConfirm?.Invoke(_newFileName);
                    Close();
                }
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }
    }
}