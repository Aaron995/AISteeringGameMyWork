using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class CompositeNode : Node
    {
        //The child ndoes for this selector
        protected List<Node> m_nodes;

        //Constructor to set child nodes
        public CompositeNode(List<Node> nodes)
        {
            m_nodes = nodes;
        }

        //Restarts all child nodes and this node.
        public override void RestartTree()
        {
            foreach (Node node in m_nodes)
            {
                node.Start();
            }
        }
    }
}
