using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class RootNode : Node
    {
        //Child node of the root node
        private readonly Node node;
        
        //Constructor requires child node
        public RootNode(Node node)
        {
            this.node = node;
        }

        //Restart this node and child nodes
        public override void RestartTree()
        {
            node.RestartTree();
        }

        // Returns the status of the child node
        public override NodeStates Update(BlackBoard bb)
        {
            return node.Update(bb);
        }
    }
}
