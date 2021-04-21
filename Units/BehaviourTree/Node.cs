using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class Node
    {
        //Current state of the node
        private NodeStates nodeState;
        public NodeStates m_nodeState { get { return nodeState; } }

        public Node() { }
        //Restart all child nodes and this node
        public abstract void RestartTree();

        //Prepare the node for first usage
        public virtual void Start() { }

        // Implementing classes use this method to valuate the desired set of conditions
        public abstract NodeStates Update(BlackBoard bb);

        //Set new node state and return the new value
        protected NodeStates SetNodeState(NodeStates newNodeState)
        {
            nodeState = newNodeState;
            return m_nodeState;
        }
    }
}
