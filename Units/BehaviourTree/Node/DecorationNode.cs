using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class DecorationNode : Node
    {
        //Child node that this decorator node wraps
        private Node node;
        public Node m_node { get { return node; } }

        //Constructor requires the child node this decorator node wraps
        public DecorationNode(Node node)
        {
            this.node = node;
        }

        //Restart this node and all child nodes
        public override void RestartTree()
        {
            node.Start();
        }


    }
}
