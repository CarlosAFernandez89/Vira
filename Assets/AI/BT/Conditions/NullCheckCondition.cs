using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace AI.BT.Conditions
{
    [Serializable, GeneratePropertyBag]
    [Condition(
        name: "Null Check",
        story: "[Variable] is null",
        category: "Variable Conditions",
        description: "Null check a reference type. Return false if a value type is provided.")]
    public partial class NullCheckCondition : Condition
    {
        [SerializeReference] public BlackboardVariable Variable;

        public override bool IsTrue()
        {
            if (Variable.Type.IsValueType)
            {
                return false;
            }

            return ReferenceEquals(Variable.ObjectValue, null);
        }
    }
}
