using System.Collections.Generic;
using System.Linq;
using Dialogue.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dialogue.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView _graphView;

        private DialogueContainer _dialogueContainer;

        private List<Edge> Edges => _graphView.edges.ToList();

        private List<Node> Nodes => _graphView.nodes.ToList();

        private List<Group> CommentBlocks =>
            _graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();

        public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
        {
            return new GraphSaveUtility
            {
                _graphView = graphView
            };
        }

        public void SaveGraph(string fileName)
        {
            // Create a new DialogueContainer object
            var dialogueContainerObject = ScriptableObject.CreateInstance<DialogueContainer>();

            // Save nodes and check if the graph is valid
            if (!SaveNodes(dialogueContainerObject))
            {
                EditorUtility.DisplayDialog("Invalid Dialogue Graph",
                    "Can't save an empty graph. Add at least one Node.", "OK");
                return;
            }

            // Save exposed properties and comment blocks
            SaveExposedProperties(dialogueContainerObject);
            SaveCommentBlocks(dialogueContainerObject);

            // Ensure the Dialogue folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogue"))
                AssetDatabase.CreateFolder("Assets/Resources", "Dialogue");

            // Load the asset if it already exists
            var loadedAsset =
                AssetDatabase.LoadAssetAtPath($"Assets/Resources/Dialogue/{fileName}.asset",
                    typeof(DialogueContainer));

            // Create or update the asset
            if (loadedAsset == null || !AssetDatabase.Contains(loadedAsset))
            {
                AssetDatabase.CreateAsset(dialogueContainerObject, $"Assets/Resources/Dialogue/{fileName}.asset");
            }
            else
            {
                // Update existing asset
                var container = loadedAsset as DialogueContainer;
                container.NodeLinks = dialogueContainerObject.NodeLinks;
                container.DialogueNodeData = dialogueContainerObject.DialogueNodeData;
                container.ConditionalNodeData = dialogueContainerObject.ConditionalNodeData; // Save ConditionalNodeData
                container.ExposedProperties = dialogueContainerObject.ExposedProperties;
                container.CommentBlockData = dialogueContainerObject.CommentBlockData;
                EditorUtility.SetDirty(container);
            }

            // Save all changes to assets
            AssetDatabase.SaveAssets();
        }

        private bool SaveNodes(DialogueContainer dialogueContainerObject)
        {
            // Check if there are any edges in the graph
            if (!Edges.Any()) return false;
            
            // Find the entry point node's GUID (if it exists in the current graph)
            string entryPointGuid = Edges.FirstOrDefault(x => x.output.portName == "Next")?.output.node.viewDataKey;

            // Save node links (connections)
            var connectedSockets = Edges.Where(x => x.input.node != null).ToArray();
            foreach (var connection in connectedSockets)
            {
                var outputNode = connection.output.node;
                var inputNode = connection.input.node;

                // Check if nodes are valid before accessing their properties
                if (outputNode != null && inputNode != null)
                {
                    dialogueContainerObject.NodeLinks.Add(new NodeLinkData
                    {
                        BaseNodeGUID = outputNode.viewDataKey,
                        PortName = connection.output.portName,
                        TargetNodeGUID = inputNode.viewDataKey
                    });
                }
            }

            // Save node data
            foreach (var node in Nodes)
            {
                // Skip the entry point node
                if (node.viewDataKey == entryPointGuid) continue;

                // Check the type of the node and save accordingly
                if (node is ConditionalNode conditionalNode)
                {
                    dialogueContainerObject.ConditionalNodeData.Add(new ConditionalNodeData
                    {
                        GUID = conditionalNode.viewDataKey,
                        DialogueName = conditionalNode.DialogueText, // Consider renaming this field in ConditionalNodeData
                        DialoguePosition = conditionalNode.GetPosition().position,
                        LeftPropertyName = conditionalNode.LeftPropertyName,
                        RightPropertyName = conditionalNode.RightPropertyName,
                        LeftPropertyType = conditionalNode.LeftPropertyType,
                        RightPropertyType = conditionalNode.RightPropertyType,
                        Operator = conditionalNode.Operator
                    });
                }
                else if (node is DialogueNode dialogueNode)
                {
                    dialogueContainerObject.DialogueNodeData.Add(new DialogueNodeData
                    {
                        GUID = dialogueNode.viewDataKey,
                        DialogueName = dialogueNode.DialogueText,
                        DialoguePosition = dialogueNode.GetPosition().position
                    });
                }
            }

            return true;
        }

        private void SaveExposedProperties(DialogueContainer dialogueContainer)
        {
            // Clear and update exposed properties
            dialogueContainer.ExposedProperties.Clear();
            dialogueContainer.ExposedProperties.AddRange(_graphView.ExposedProperties);
        }

        private void SaveCommentBlocks(DialogueContainer dialogueContainer)
        {
            // Save comment block data
            foreach (var block in CommentBlocks)
            {
                var nodes = block.containedElements.OfType<Node>().Select(x => x.viewDataKey).ToList();

                dialogueContainer.CommentBlockData.Add(new CommentBlockData
                {
                    ChildNodes = nodes,
                    Title = block.title,
                    Position = block.GetPosition().position
                });
            }
        }

        public void LoadNarrative(string fileName)
        {
            // Load the DialogueContainer from resources
            _dialogueContainer = Resources.Load<DialogueContainer>($"Dialogue/{fileName}");
            if (_dialogueContainer == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target Narrative Data does not exist!", "OK");
                return;
            }

            // Clear the graph and regenerate nodes and connections
            ClearGraph();
            GenerateDialogueNodes();
            GenerateConditionalNodes(); // Load Conditional Nodes
            ConnectDialogueNodes(); // Connect all nodes (Dialogue and Conditional)
            AddExposedProperties();
            GenerateCommentBlocks();
        }

        public void ClearGraph()
        {
            // Ensure _dialogueContainer is not null
            if (_dialogueContainer == null) return;

            // Reset the entry point GUID if it exists
            var entryPoint = Nodes.OfType<DialogueNode>().FirstOrDefault(x => x.EntryPoint);
            if (entryPoint != null)
            {
                entryPoint.viewDataKey = _dialogueContainer.NodeLinks.FirstOrDefault()?.BaseNodeGUID;
            }

            // Remove all nodes and edges except the entry point
            foreach (var node in Nodes)
            {
                if (node == entryPoint) continue;

                // Remove connected edges
                Edges.Where(edge => edge.input.node == node || edge.output.node == node).ToList()
                    .ForEach(edge => _graphView.RemoveElement(edge));

                // Remove the node
                _graphView.RemoveElement(node);
            }
        }

        private void GenerateDialogueNodes()
        {
            // Create and add dialogue nodes to the graph
            foreach (var perNode in _dialogueContainer.DialogueNodeData)
            {
                var tempNode = _graphView.CreateNode(perNode.DialogueName, Vector2.zero);
                tempNode.viewDataKey = perNode.GUID;
                _graphView.AddElement(tempNode);

                // Add choice ports to the node
                var nodePorts = _dialogueContainer.NodeLinks.Where(x => x.BaseNodeGUID == perNode.GUID).ToList();
                nodePorts.ForEach(x => _graphView.AddChoicePort(tempNode, x.PortName));
            }
        }

        private void GenerateConditionalNodes()
        {
            foreach (var perNode in _dialogueContainer.ConditionalNodeData)
            {
                // Create the ConditionalNode
                var tempNode = _graphView.CreateConditionalNode(perNode.DialogueName, Vector2.zero);
                tempNode.viewDataKey = perNode.GUID;
                tempNode.LeftPropertyName = perNode.LeftPropertyName;
                tempNode.RightPropertyName = perNode.RightPropertyName;
                tempNode.LeftPropertyType = perNode.LeftPropertyType;
                tempNode.RightPropertyType = perNode.RightPropertyType;
                tempNode.Operator = perNode.Operator;

                // Set the value of the left property dropdown
                var leftPropertyDropdown = tempNode.mainContainer.Q<VisualElement>("LeftPropertyField")?.Q<PopupField<string>>();
                if (leftPropertyDropdown != null)
                {
                    leftPropertyDropdown.SetValueWithoutNotify(tempNode.LeftPropertyName);
                }

                // Update the right property field after setting the left property
                var rightPropertyContainer = tempNode.mainContainer.Q<VisualElement>("rightPropertyContainer");
                if (rightPropertyContainer != null)
                {
                    _graphView.UpdateRightPropertyField(tempNode, rightPropertyContainer);
                }

                // Get existing output ports BEFORE adding the node to the graph
                var existingPorts = tempNode.outputContainer.Query<Port>().ToList();

                // Add output ports (True/False) to the node only if they don't exist
                var truePortExists = existingPorts.Any(port => port.portName == "True");
                var falsePortExists = existingPorts.Any(port => port.portName == "False");

                if (!truePortExists)
                {
                    _graphView.AddConditionalOutputPort(tempNode, "True");
                }

                if (!falsePortExists)
                {
                    _graphView.AddConditionalOutputPort(tempNode, "False");
                }

                // Add the node to the graph AFTER adding the ports
                _graphView.AddElement(tempNode);
            }
        }

        private void ConnectDialogueNodes()
        {
            // Connect nodes based on the loaded data
            var allNodes = Nodes.ToList(); // Include both DialogueNode and ConditionalNode

            for (var i = 0; i < allNodes.Count; i++)
            {
                var connections = _dialogueContainer.NodeLinks.Where(x => x.BaseNodeGUID == allNodes[i].viewDataKey)
                    .ToList();
                for (var j = 0; j < connections.Count(); j++)
                {
                    var targetNodeGuid = connections[j].TargetNodeGUID;
                    var targetNode = allNodes.FirstOrDefault(x => x.viewDataKey == targetNodeGuid);
                    if (targetNode != null)
                    {
                        Port outputPort = null;
                        if (allNodes[i] is DialogueNode dialogueNode)
                        {
                            outputPort = dialogueNode.outputContainer[j].Q<Port>();
                        }
                        else if (allNodes[i] is ConditionalNode conditionalNode)
                        {
                            outputPort = conditionalNode.outputContainer.Query<Port>().ToList()
                                .FirstOrDefault(port => port.portName == connections[j].PortName);
                        }

                        if (outputPort != null)
                        {
                            LinkNodesTogether(outputPort, (Port)targetNode.inputContainer[0]);

                            targetNode.SetPosition(new Rect(
                                GetNodePosition(targetNodeGuid),
                                _graphView.DefaultNodeSize));
                        }
                    }
                }
            }
        }

        // Helper function to get node position from either DialogueNodeData or ConditionalNodeData
        private Vector2 GetNodePosition(string nodeGuid)
        {
            var dialogueNodeData = _dialogueContainer.DialogueNodeData.FirstOrDefault(x => x.GUID == nodeGuid);
            if (dialogueNodeData != null)
            {
                return dialogueNodeData.DialoguePosition;
            }

            var conditionalNodeData = _dialogueContainer.ConditionalNodeData.FirstOrDefault(x => x.GUID == nodeGuid);
            if (conditionalNodeData != null)
            {
                return conditionalNodeData.DialoguePosition;
            }

            return Vector2.zero; // Default position if not found
        }

        private void LinkNodesTogether(Port outputSocket, Port inputSocket)
        {
            // Create and add an edge between two nodes
            var tempEdge = new Edge
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge.input?.Connect(tempEdge);
            tempEdge.output?.Connect(tempEdge);
            _graphView.Add(tempEdge);
        }

        private void AddExposedProperties()
        {
            // Add exposed properties to the blackboard
            _graphView.ClearBlackBoardAndExposedProperties();
            foreach (var exposedProperty in _dialogueContainer.ExposedProperties)
            {
                _graphView.AddPropertyToBlackBoard(exposedProperty);
            }
        }

        private void GenerateCommentBlocks()
        {
            // Create and add comment blocks to the graph
            foreach (var commentBlock in CommentBlocks)
            {
                _graphView.RemoveElement(commentBlock);
            }

            foreach (var commentBlockData in _dialogueContainer.CommentBlockData)
            {
                var block = _graphView.CreateCommentBlock(
                    new Rect(commentBlockData.Position, _graphView.DefaultCommentBlockSize),
                    commentBlockData);
                block.AddElements(Nodes.Where(x => commentBlockData.ChildNodes.Contains(x.viewDataKey)));
            }
        }
    }
}