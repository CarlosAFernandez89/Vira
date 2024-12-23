using System;
using System.Linq;
using UnityEngine;

namespace Dialogue.Runtime
{
    public class DialogueRunner : MonoBehaviour
    {
        public DialogueContainer dialogueContainer; // Assign your DialogueContainer here
    
        // Function to evaluate a ConditionalNode
        public bool EvaluateConditionalNode(ConditionalNodeData conditionalNodeData)
        {
            // 1. Get the ExposedProperty values:
            ExposedProperty leftProperty = GetExposedProperty(conditionalNodeData.LeftPropertyName);

            // 2. Handle null properties (optional):
            if (leftProperty == null)
            {
                Debug.LogError($"Conditional node evaluation failed: Left property is null (Left: {conditionalNodeData.LeftPropertyName}).");
                return false; // Or handle it in a way that makes sense for your game
            }
        
            // Check if the properties have the expected types
            if (leftProperty.PropertyType != conditionalNodeData.LeftPropertyType)
            {
                Debug.LogError("Property type mismatch in conditional node evaluation.");
                return false; // Or handle it differently
            }

            // 3. Perform the comparison based on property type and operator:
            return CompareProperties(leftProperty, conditionalNodeData.RightPropertyName, conditionalNodeData.Operator, conditionalNodeData);
        }
    
        // Helper function to compare properties (add more types as needed)
        // Helper function to compare properties (add more types as needed)
        private bool CompareProperties(ExposedProperty left, string rightPropertyValue, ComparisonOperator op, ConditionalNodeData conditionalNodeData)
        {
            // Perform comparison based on the property type
            switch (left.PropertyType)
            {
                case ExposedProperty.ExposedPropertyType.String:
                    return CompareValues(left.PropertyValue, rightPropertyValue, op);

                case ExposedProperty.ExposedPropertyType.Int:
                    if (int.TryParse(left.PropertyValue, out int leftInt) &&
                        int.TryParse(rightPropertyValue, out int rightInt))
                    {
                        return CompareValues(leftInt, rightInt, op);
                    }
                    break;

                case ExposedProperty.ExposedPropertyType.Float:
                    if (float.TryParse(left.PropertyValue, out float leftFloat) &&
                        float.TryParse(rightPropertyValue, out float rightFloat))
                    {
                        return CompareValues(leftFloat, rightFloat, op);
                    }
                    break;

                case ExposedProperty.ExposedPropertyType.Bool:
                    if (bool.TryParse(left.PropertyValue, out bool leftBool) &&
                        bool.TryParse(rightPropertyValue, out bool rightBool))
                    {
                        return CompareValues(leftBool, rightBool, op);
                    }
                    break;
            }

            // Log an error if the type is not supported
            Debug.LogError($"Unsupported property type for comparison: {left.PropertyType}");
            return false;
        }
    
        // Overloads for CompareValues to handle different data types
        private bool CompareValues<T>(T leftValue, T rightValue, ComparisonOperator op) where T : IComparable<T>
        {
            switch (op)
            {
                case ComparisonOperator.Equal:
                    return leftValue.CompareTo(rightValue) == 0;
                case ComparisonOperator.NotEqual:
                    return leftValue.CompareTo(rightValue) != 0;
                case ComparisonOperator.GreaterThan:
                    return leftValue.CompareTo(rightValue) > 0;
                case ComparisonOperator.LessThan:
                    return leftValue.CompareTo(rightValue) < 0;
                case ComparisonOperator.GreaterThanOrEqual:
                    return leftValue.CompareTo(rightValue) >= 0;
                case ComparisonOperator.LessThanOrEqual:
                    return leftValue.CompareTo(rightValue) <= 0;
                default:
                    Debug.LogError($"Unsupported operator: {op}");
                    return false;
            }
        }
    
        // Helper function to get an ExposedProperty from the DialogueContainer
        private ExposedProperty GetExposedProperty(string propertyName)
        {
            return dialogueContainer.ExposedProperties.FirstOrDefault(p => p.PropertyName == propertyName);
        }
        
        public void TraverseDialogue(string startNodeGuid)
        {
            string currentNodeGuid = startNodeGuid;

            while (!string.IsNullOrEmpty(currentNodeGuid))
            {
                // 1. Find the current node data (either DialogueNodeData or ConditionalNodeData)
                DialogueNodeData dialogueNodeData = dialogueContainer.DialogueNodeData.FirstOrDefault(n => n.GUID == currentNodeGuid);
                ConditionalNodeData conditionalNodeData = dialogueContainer.ConditionalNodeData.FirstOrDefault(n => n.GUID == currentNodeGuid);

                // 2. Handle DialogueNode (simple case - just pick the first output)
                if (dialogueNodeData != null)
                {
                    // Display dialogue text, handle choices, etc.

                    // Get the next node's GUID (simple example - just pick the first connection)
                    NodeLinkData nextLink = dialogueContainer.NodeLinks.FirstOrDefault(link => link.BaseNodeGUID == currentNodeGuid);
                    currentNodeGuid = nextLink?.TargetNodeGUID;
                }
                // 3. Handle ConditionalNode
                else if (conditionalNodeData != null)
                {
                    // Evaluate the condition
                    bool conditionResult = EvaluateConditionalNode(conditionalNodeData);

                    // Get the next node's GUID based on the condition result
                    NodeLinkData nextLink = dialogueContainer.NodeLinks.FirstOrDefault(link =>
                        link.BaseNodeGUID == currentNodeGuid &&
                        ((conditionResult && link.PortName == "True") || (!conditionResult && link.PortName == "False")));

                    currentNodeGuid = nextLink?.TargetNodeGUID;
                }
                else
                {
                    // Node not found (shouldn't normally happen)
                    Debug.LogError($"Node not found: {currentNodeGuid}");
                    currentNodeGuid = null; // Stop traversal
                }
            }
        }
    }
}