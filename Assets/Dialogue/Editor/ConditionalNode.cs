using Dialogue.Runtime;

namespace Dialogue.Editor
{
    public class ConditionalNode : DialogueNode
    {
        public string LeftPropertyName;
        public string RightPropertyName;
        public ExposedProperty.ExposedPropertyType LeftPropertyType;
        public ExposedProperty.ExposedPropertyType RightPropertyType;
        public Runtime.ComparisonOperator Operator; // Use the Runtime version
    }
}