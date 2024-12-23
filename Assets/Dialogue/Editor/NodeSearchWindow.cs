using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow _window;
        private DialogueGraphView _graphView;

        private Texture2D _indentationIcon;
        
        // Declare dummy instances (but don't create them here)
        private DialogueNode _dummyDialogueNode;
        private ConditionalNode _dummyConditionalNode;
        private Group _dummyGroup;
        
        public void Configure(EditorWindow window,DialogueGraphView graphView)
        {
            _window = window;
            _graphView = graphView;
            
            //Transparent 1px indentation icon as a hack
            _indentationIcon = new Texture2D(1,1);
            _indentationIcon.SetPixel(0,0,new Color(0,0,0,0));
            _indentationIcon.Apply();
        }
        
        private void OnEnable()
        {
            // Create dummy instances in OnEnable
            _dummyDialogueNode = new DialogueNode();
            _dummyConditionalNode = new ConditionalNode();
            _dummyGroup = new Group();
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
                new SearchTreeEntry(new GUIContent("Dialogue Node", _indentationIcon))
                {
                    level = 2, userData = _dummyDialogueNode // Use the dummy instance
                },
                new SearchTreeEntry(new GUIContent("Conditional Node", _indentationIcon))
                {
                    level = 2, userData = _dummyConditionalNode // Use the dummy instance
                },
                new SearchTreeEntry(new GUIContent("Comment Block",_indentationIcon))
                {
                    level = 1,
                    userData = _dummyGroup // Use the dummy instance
                }
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            //Editor window-based mouse position
            var mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);

            switch (SearchTreeEntry.userData)
            {
                // Check for ConditionalNode before DialogueNode
                case ConditionalNode conditionalNode:
                    // Create a new conditional node at the mouse position
                    _graphView.AddElement(_graphView.CreateConditionalNode("Conditional", graphMousePosition));
                    return true;
                case DialogueNode dialogueNode:
                    // Create a new dialogue node at the mouse position
                    _graphView.CreateNewDialogueNode("Dialogue Node", graphMousePosition);
                    return true;
                case Group group:
                    // Create a new comment block at the mouse position
                    var rect = new Rect(graphMousePosition, _graphView.DefaultCommentBlockSize);
                    _graphView.CreateCommentBlock(rect);
                    return true;
                default:
                    // Default case to handle any unexpected types
                    return false;
            }
        } 
    }
}