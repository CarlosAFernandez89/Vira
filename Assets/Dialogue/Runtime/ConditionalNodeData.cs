using System;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;

namespace Dialogue.Runtime
{
    [Serializable]
    public class ConditionalNodeData
    {
        public string GUID;
        public string DialogueName;
        public Vector2 DialoguePosition;
        public string LeftPropertyName;
        public string RightPropertyName;
        public ExposedProperty.ExposedPropertyType LeftPropertyType;
        public ExposedProperty.ExposedPropertyType RightPropertyType;
        public ComparisonOperator Operator; // Use the Runtime ComparisonOperator
    }
}