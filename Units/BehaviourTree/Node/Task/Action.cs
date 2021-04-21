using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class Action : TaskNode
    {
        //The constructor requires a delegate with the logic to work
        public Action(ActionDelegate action) : base(action) { }

        // Evaluates the node using the passed in delegate and reports the resulting state as appropriate
        //Returns SUCCESS if the action succeeds.
        //Returns FAILURE if the action fails.
        //Returns RUNNING if the action is running.
        public override NodeStates Update(BlackBoard bb)
        {
            switch (m_action(bb))
            {
                case NodeStates.SUCCESS:
                    return SetNodeState(NodeStates.SUCCESS);
                case NodeStates.FAILURE:
                    return SetNodeState(NodeStates.FAILURE);
                case NodeStates.RUNNING:
                    return SetNodeState(NodeStates.RUNNING);
                default:
                    return SetNodeState(NodeStates.SUCCESS);
            }
        }
    }
}
