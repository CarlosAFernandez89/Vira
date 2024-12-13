using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace AI.BT.Actions.Math
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Integer Operation",
        story: "[Result] = [LHS] [Operator] [RHS]",
        category: "Action/Math")]
    public class IntegerOperation : Action
    {
        public enum OperatorType
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            IncrementAndRepeat
        }

        [SerializeReference]
        public BlackboardVariable<int> Result;

        [SerializeReference]
        public BlackboardVariable<int> LHS;

        [SerializeReference]
        public BlackboardVariable<OperatorType> Operator;

        [SerializeReference]
        public BlackboardVariable<int> RHS;

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

                case OperatorType.IncrementAndRepeat:
                    Result.Value = (int)Mathf.Repeat(LHS.Value + 1, RHS.Value);
                    break;
            };

            return Status.Success;
        }
    }
}