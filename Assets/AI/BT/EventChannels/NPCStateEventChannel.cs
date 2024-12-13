using System;
using AI.BT.Enums;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace AI.BT.EventChannels
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/NPC State Event Channel")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(
        name: "NPC State Event Channel",
        message: "NPC state has changed to [value]",
        category: "Events/Demo")]
    public partial class NPCStateEventChannel : EventChannelBase
    {
        public delegate void GuardStateEventChannelEventHandler(NPCState value);

        public event GuardStateEventChannelEventHandler Event;

        public void SendEventMessage(NPCState value)
        {
            Event?.Invoke(value);
        }

        public override void SendEventMessage(BlackboardVariable[] messageData)
        {
            var value = messageData[0] is BlackboardVariable<NPCState> valueBlackboardVariable ? valueBlackboardVariable.Value : default(NPCState);

            Event?.Invoke(value);
        }

        public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
        {
            GuardStateEventChannelEventHandler del = (value) =>
            {
                if (vars[0] is BlackboardVariable<NPCState> var0)
                    var0.Value = value;

                callback();
            };
            return del;
        }

        public override void RegisterListener(Delegate del)
        {
            Event += del as GuardStateEventChannelEventHandler;
        }

        public override void UnregisterListener(Delegate del)
        {
            Event -= del as GuardStateEventChannelEventHandler;
        }
    }
}