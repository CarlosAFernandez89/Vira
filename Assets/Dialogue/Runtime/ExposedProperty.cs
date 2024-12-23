namespace Dialogue.Runtime
{
    [System.Serializable]
    public class ExposedProperty
    {
        public static ExposedProperty CreateInstance()
        {
            return new ExposedProperty();
        }

        public string PropertyName = "New String";
        public string PropertyValue = "New Value";

        public ExposedPropertyType PropertyType = ExposedPropertyType.String;

        public enum ExposedPropertyType
        {
            String,
            Int,
            Float,
            Bool,
        }
    }
}