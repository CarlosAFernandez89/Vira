using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace AI.BT.Actions.Math
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Float Operation",
        story: "[Result] = [LHS] [Operator] [RHS]",
        category: "Action/Math")]
    public partial class FloatOperation : Action
    {
        public enum OperatorType
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        [SerializeReference]
        public BlackboardVariable<float> Result;

        [SerializeReference]
        public BlackboardVariable<float> LHS;

        [SerializeReference]
        public BlackboardVariable<OperatorType> Operator;

        [SerializeReference]
        public BlackboardVariable<float> RHS;

        protected override Status OnStart()
        {
            switch (Operator.Value)
            {
                case OperatorType.Add:
                    Result.Value = LHS.Value + RHS.Value;
                    break;

                case OperatorType.Subtract:
                    Result.Value = LHS.Value - RHS.Value;
                    break;

                case OperatorType.Multiply:
                    Result.Value = LHS.Value * RHS.Value;
                    break;

                case OperatorType.Divide:
                    Result.Value = LHS.Value / RHS.Value;
                    break;
            };

            return Status.Success;
        }
    }
}