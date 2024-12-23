using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;


namespace Dialogue.Editor
{
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public readonly Vector2 DefaultCommentBlockSize = new Vector2(300, 200);
        public DialogueNode EntryPointNode;
        public Blackboard Blackboard = new Blackboard();
        public List<ExposedProperty> ExposedProperties { get; private set; } = new List<ExposedProperty>();
        private NodeSearchWindow _searchWindow;

        public DialogueGraphView(DialogueGraph editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GetEntryPointNodeInstance());

            AddSearchWindow(editorWindow);
        }


        private void AddSearchWindow(DialogueGraph editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }


        public void ClearBlackBoardAndExposedProperties()
        {
            ExposedProperties.Clear();
            Blackboard.Clear();
        }

        public Group CreateCommentBlock(Rect rect, CommentBlockData commentBlockData = null)
        {
            if(commentBlockData==null)
                commentBlockData = new CommentBlockData();
            var group = new Group
            {
                autoUpdateGeometry = true,
                title = commentBlockData.Title
            };
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }

        public void AddPropertyToBlackBoard(ExposedProperty property, bool loadMode = false)
        {
            var localPropertyName = property.PropertyName;
            var localPropertyValue = property.PropertyValue;
            if (!loadMode)
            {
                while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
                    localPropertyName = $"{localPropertyName}(1)";
            }
            
            var item = ExposedProperty.CreateInstance();
            item.PropertyName = localPropertyName;
            item.PropertyValue = localPropertyValue;
            item.PropertyType = property.PropertyType; // Set the type
            ExposedProperties.Add(item);
            
            var container = new VisualElement();
            
            // Use a popup field for type selection
            var typePopup = new PopupField<string>(
                System.Enum.GetNames(typeof(ExposedProperty.ExposedPropertyType)).ToList(),
                (int)item.PropertyType
            );
            
            // Create a container for the value field HERE
            var valueContainer = new VisualElement();
            
            typePopup.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.PropertyName == item.PropertyName);
                ExposedProperties[index].PropertyType = (ExposedProperty.ExposedPropertyType)System.Enum.Parse(typeof(ExposedProperty.ExposedPropertyType), evt.newValue);
            
                // Update value field based on the type
                UpdatePropertyValueField(ExposedProperties[index], valueContainer);
            });
            
            var field = new BlackboardField { text = localPropertyName, typeText = typePopup.value };
            container.Add(field);
            
            // Update the value field initially
            UpdatePropertyValueField(item, valueContainer);
            
            var sa = new BlackboardRow(field, valueContainer);
            container.Add(sa);
            container.Add(typePopup);
            Blackboard.Add(container);
            
            // Add delete option to blackboard variables
            field.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Delete", (DropdownMenuAction a) =>
                {
                    // Remove from ExposedProperties list
                    ExposedProperties.Remove(item);
            
                    // Remove from Blackboard
                    Blackboard.Remove(container);
                });
            }));
        }
        
        private void UpdatePropertyValueField(ExposedProperty property, VisualElement valueContainer)
        {
            valueContainer.Clear(); // Clear existing field

            switch (property.PropertyType)
            {
                case ExposedProperty.ExposedPropertyType.String:
                    var stringField = new TextField("Value:") { value = property.PropertyValue };
                    stringField.RegisterValueChangedCallback(evt => property.PropertyValue = evt.newValue);
                    valueContainer.Add(stringField);
                    break;

                case ExposedProperty.ExposedPropertyType.Int:
                    // Parse initial value, use 0 if parsing fails
                    if (!int.TryParse(property.PropertyValue, out int intValue))
                    {
                        intValue = 0;
                    }
                    var intField = new IntegerField("Value:") { value = intValue };
                    intField.RegisterValueChangedCallback(evt => property.PropertyValue = evt.newValue.ToString()); // Convert to string
                    valueContainer.Add(intField);
                    break;

                case ExposedProperty.ExposedPropertyType.Float:
                    // Parse initial value, use 0 if parsing fails
                    if (!float.TryParse(property.PropertyValue, out float floatValue))
                    {
                        floatValue = 0.0f; 
                    }
                    var floatField = new FloatField("Value:") { value = floatValue };
                    floatField.RegisterValueChangedCallback(evt => property.PropertyValue = evt.newValue.ToString()); // Convert to string
                    valueContainer.Add(floatField);
                    break;

                case ExposedProperty.ExposedPropertyType.Bool:
                    // Parse initial value, use false if parsing fails
                    if (!bool.TryParse(property.PropertyValue, out bool boolValue))
                    {
                        boolValue = false;
                    }
                    var boolField = new Toggle("Value:") { value = boolValue };
                    boolField.RegisterValueChangedCallback(evt => property.PropertyValue = evt.newValue.ToString()); // Convert to string
                    valueContainer.Add(boolField);
                    break;
                default: // Handle the default case to make the warning go away.
                    var defaultStringField = new TextField("Value:") { value = property.PropertyValue };
                    defaultStringField.RegisterValueChangedCallback(evt => property.PropertyValue = evt.newValue);
                    valueContainer.Add(defaultStringField);
                    break;
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var startPortView = startPort;

            ports.ForEach((port) =>
            {
                var portView = port;
                if (startPortView != portView && startPortView.node != portView.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void CreateNewDialogueNode(string nodeName, Vector2 position)
        {
            AddElement(CreateNode(nodeName, position));
        }

        public DialogueNode CreateNode(string nodeName, Vector2 position)
        {
            var tempDialogueNode = new DialogueNode()
            {
                title = nodeName,
                DialogueText = nodeName,
                GUID = Guid.NewGuid().ToString()
            };
            tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("DialogueNode"));
            var inputPort = GetPortInstance(tempDialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            tempDialogueNode.inputContainer.Add(inputPort);
            tempDialogueNode.RefreshExpandedState();
            tempDialogueNode.RefreshPorts();
            tempDialogueNode.SetPosition(new Rect(position,
                DefaultNodeSize)); //To-Do: implement screen center instantiation positioning

            var textField = new TextField("");
            textField.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.DialogueText = evt.newValue;
                tempDialogueNode.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(tempDialogueNode.title);
            tempDialogueNode.mainContainer.Add(textField);

            var button = new Button(() => { AddChoicePort(tempDialogueNode); })
            {
                text = "Add Choice"
            };
            tempDialogueNode.titleButtonContainer.Add(button);
            return tempDialogueNode;
        }


        public void AddChoicePort(DialogueNode nodeCache, string overriddenPortName = "")
        {
            var generatedPort = GetPortInstance(nodeCache, Direction.Output);
            var portLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(portLabel);

            var outputPortCount = nodeCache.outputContainer.Query("connector").ToList().Count();
            var outputPortName = string.IsNullOrEmpty(overriddenPortName)
                ? $"Option {outputPortCount + 1}"
                : overriddenPortName;


            var textField = new TextField()
            {
                name = string.Empty,
                value = outputPortName
            };
            textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(nodeCache, generatedPort))
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = outputPortName;
            nodeCache.outputContainer.Add(generatedPort);
            nodeCache.RefreshPorts();
            nodeCache.RefreshExpandedState();
        }

        private void RemovePort(Node node, Port socket)
        {
            var targetEdge = edges.ToList()
                .Where(x => x.output.portName == socket.portName && x.output.node == socket.node);
            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            node.outputContainer.Remove(socket);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        private Port GetPortInstance(DialogueNode node, Direction nodeDirection,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }

        private DialogueNode GetEntryPointNodeInstance()
        {
            var nodeCache = new DialogueNode()
            {
                title = "START",
                GUID = Guid.NewGuid().ToString(),
                DialogueText = "ENTRYPOINT",
                EntryPoint = true
            };

            var generatedPort = GetPortInstance(nodeCache, Direction.Output);
            generatedPort.portName = "Next";
            nodeCache.outputContainer.Add(generatedPort);

            nodeCache.capabilities &= ~Capabilities.Movable;
            nodeCache.capabilities &= ~Capabilities.Deletable;

            nodeCache.RefreshExpandedState();
            nodeCache.RefreshPorts();
            nodeCache.SetPosition(new Rect(100, 200, 100, 150));
            return nodeCache;
        }
        
        public ConditionalNode CreateConditionalNode(string nodeName, Vector2 position)
        {
            var tempConditionalNode = new ConditionalNode()
            {
                title = nodeName,
                DialogueText = nodeName, // You might want to customize this
                GUID = Guid.NewGuid().ToString()
            };
            tempConditionalNode.styleSheets.Add(Resources.Load<StyleSheet>("DialogueNode"));

            // Input Port
            var inputPort = GetPortInstance(tempConditionalNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            tempConditionalNode.inputContainer.Add(inputPort);

            // Add output ports for each possible branch (e.g., True and False)
            AddConditionalOutputPort(tempConditionalNode, "True");
            AddConditionalOutputPort(tempConditionalNode, "False");

            // Property Selection
            var leftPropertyField = CreatePropertySelectionField(tempConditionalNode, true);
            tempConditionalNode.mainContainer.Add(leftPropertyField);

            // Right Property Input Field (Dynamically Updated)
            var rightPropertyContainer = new VisualElement();
            tempConditionalNode.mainContainer.Add(rightPropertyContainer);

            // Update Right Property Field when Left Property Changes
            var leftPropertyDropdown = leftPropertyField.Q<PopupField<string>>();
            leftPropertyDropdown.RegisterValueChangedCallback(evt =>
            {
                UpdateRightPropertyField(tempConditionalNode, rightPropertyContainer);
            });

            // Initialize Right Property Field based on initial Left Property
            EditorApplication.delayCall += () =>
            {
                UpdateRightPropertyField(tempConditionalNode, rightPropertyContainer);
            };

            // Operator Selection
            var operatorField = new PopupField<string>(
                System.Enum.GetNames(typeof(Runtime.ComparisonOperator)).ToList(),
                (int)tempConditionalNode.Operator
            );
            operatorField.RegisterValueChangedCallback(evt =>
            {
                tempConditionalNode.Operator = (Runtime.ComparisonOperator)System.Enum.Parse(typeof(Runtime.ComparisonOperator), evt.newValue);
            });
            tempConditionalNode.mainContainer.Add(new Label("Operator:"));
            tempConditionalNode.mainContainer.Add(operatorField);

            tempConditionalNode.RefreshExpandedState();
            tempConditionalNode.RefreshPorts();
            tempConditionalNode.SetPosition(new Rect(position, DefaultNodeSize));

            return tempConditionalNode;
        } 
        
        public void UpdateRightPropertyField(ConditionalNode node, VisualElement rightPropertyContainer) 
        {
            rightPropertyContainer.Clear(); // Clear existing field
            
            // Find the selected Left Property
            ExposedProperty leftProperty = null;
            if (!string.IsNullOrEmpty(node.LeftPropertyName))
            {
                leftProperty = ExposedProperties.FirstOrDefault(p => p.PropertyName == node.LeftPropertyName);
            }
            
            // Create the appropriate input field based on the Left Property type
            if (leftProperty != null)
            {
                switch (leftProperty.PropertyType)
                {
                    case ExposedProperty.ExposedPropertyType.String:
                        var stringField = new TextField("Value:") { value = node.RightPropertyName }; // Use RightPropertyName to store the value
                        stringField.RegisterValueChangedCallback(evt => node.RightPropertyName = evt.newValue);
                        rightPropertyContainer.Add(stringField);
                        break;
            
                    case ExposedProperty.ExposedPropertyType.Int:
                        // Try to parse the existing value, or default to 0
                        if (!int.TryParse(node.RightPropertyName, out int intValue))
                        {
                            intValue = 0;
                        }
                        var intField = new IntegerField("Value:") { value = intValue };
                        intField.RegisterValueChangedCallback(evt => node.RightPropertyName = evt.newValue.ToString());
                        rightPropertyContainer.Add(intField);
                        break;
            
                    case ExposedProperty.ExposedPropertyType.Float:
                        // Try to parse the existing value, or default to 0
                        if (!float.TryParse(node.RightPropertyName, out float floatValue))
                        {
                            floatValue = 0f;
                        }
                        var floatField = new FloatField("Value:") { value = floatValue };
                        floatField.RegisterValueChangedCallback(evt => node.RightPropertyName = evt.newValue.ToString());
                        rightPropertyContainer.Add(floatField);
                        break;
            
                    case ExposedProperty.ExposedPropertyType.Bool:
                        // Try to parse the existing value, or default to false
                        if (!bool.TryParse(node.RightPropertyName, out bool boolValue))
                        {
                            boolValue = false;
                        }
                        var boolField = new Toggle("Value:") { value = boolValue };
                        boolField.RegisterValueChangedCallback(evt => node.RightPropertyName = evt.newValue.ToString());
                        rightPropertyContainer.Add(boolField);
                        break;
                }
            }
            else
            {
                // If no Left Property is selected, you might want to display a message or disable the field
                rightPropertyContainer.Add(new Label("Select a Left Property"));
            }
        }
        
        private VisualElement CreatePropertySelectionField(ConditionalNode node, bool isLeft)
        {
            var container = new VisualElement { name = isLeft ? "LeftPropertyField" : "RightPropertyField" };
        
            // Get the list of exposed property names
            var exposedPropertyNames = ExposedProperties.Select(p => p.PropertyName).ToList();
        
            // Determine the current property name
            string currentPropertyName = isLeft ? node.LeftPropertyName : node.RightPropertyName;
        
            // Add a label to display the property type
            var typeLabel = new Label(isLeft ? node.LeftPropertyType.ToString() : node.RightPropertyType.ToString());
        
            // Create the PopupField with special handling for empty choices
            PopupField<string> propertyDropdown;
            if (exposedPropertyNames.Count > 0)
            {
                // If there are choices, use the standard PopupField constructor
                string defaultValue = exposedPropertyNames.Contains(currentPropertyName) ? currentPropertyName : exposedPropertyNames[0];
                propertyDropdown = new PopupField<string>(exposedPropertyNames, defaultValue);
            }
            else
            {
                // If choices are empty, use a constructor that doesn't require a default value
                propertyDropdown = new PopupField<string>(exposedPropertyNames, 0, null, null); // Use index 0 as default (will be ignored)
                propertyDropdown.SetEnabled(false); // Optionally disable the dropdown
            }
        
            // Register value changed callback to update the node's properties
            propertyDropdown.RegisterValueChangedCallback(evt =>
            {
                if (isLeft)
                {
                    node.LeftPropertyName = evt.newValue;
                    node.LeftPropertyType = ExposedProperties.FirstOrDefault(p => p.PropertyName == evt.newValue)?.PropertyType ?? ExposedProperty.ExposedPropertyType.String;
                }
                else
                {
                    node.RightPropertyName = evt.newValue;
                    node.RightPropertyType = ExposedProperties.FirstOrDefault(p => p.PropertyName == evt.newValue)?.PropertyType ?? ExposedProperty.ExposedPropertyType.String;
                }
                // Update the type label based on the selected property
                typeLabel.text = isLeft ? node.LeftPropertyType.ToString() : node.RightPropertyType.ToString();
            });
        
            container.Add(new Label(isLeft ? "Left Property:" : "Right Property:"));
            container.Add(propertyDropdown);
            container.Add(typeLabel);
        
            return container;
        }

        public void AddConditionalOutputPort(ConditionalNode node, string portName)
        {
            var generatedPort = GetPortInstance(node, Direction.Output);
            generatedPort.portName = portName;
            node.outputContainer.Add(generatedPort);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }
    }
}