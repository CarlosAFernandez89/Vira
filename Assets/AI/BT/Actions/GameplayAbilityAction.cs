using System;
using BandoWare.GameplayTags;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace AI.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name:"Gameplay Ability",
        description:"Tries to activate a Gameplay Ability with given tag.",
        category:"Action/GameplayAbilitySystem",
        story:"[Agent] trying to use Gameplay Ability with tag [AbilityTag]")]
    public class GameplayAbilityAction : Action
    {
        [SerializeReference] 
        public BlackboardVariable<GameObject> Agent;
        
        [SerializeReference]
        public BlackboardVariable<GameplayTag> AbilityTag = new BlackboardVariable<GameplayTag>();


        protected override Status OnStart()
        {


            return Status.Running;
        }

        protected override Status OnUpdate()
        {

            return Status.Running;
        }
    }
}