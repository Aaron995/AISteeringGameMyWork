using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class TaskNode : Node
    {
        //Method signature for this action
        public delegate NodeStates ActionDelegate(BlackBoard bb);

        //The delegate that is called to evaluate this node
        protected ActionDelegate m_action;

        //Because this node contains no logic itself, the logic must be passed in the form of a delegate.
        //As the signature states, the action needs to return a nodestate enum
        public TaskNode(ActionDelegate action)
        {
            m_action = action;
        }

        //Restart this node and all child nodes
        public override void RestartTree() { }        
    }
}
