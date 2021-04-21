using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class Selector : CompositeNode
    {
        //The constrtuctor requires a list of child nodes to work
        public Selector(List<Node> nodes) : base(nodes) { }

        //Returns SUCCESS once a child node reports success
        //Returns FAILURE if all child nodes fail.
        //Returns RUNNING if any of the child nodes is running.
        public override NodeStates Update(BlackBoard bb)
        {
            foreach (Node node in m_nodes)
            {
                switch (node.Update(bb))
                {
                    case NodeStates.SUCCESS:
                        return SetNodeState(NodeStates.SUCCESS);                        
                    case NodeStates.FAILURE:
                        continue;                        
                    case NodeStates.RUNNING:
                        return SetNodeState(NodeStates.RUNNING);                        
                    default:
                        continue;                        
                }
            }
            return SetNodeState(NodeStates.FAILURE);
        }
    }
}
