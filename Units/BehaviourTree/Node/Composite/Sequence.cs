using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class Sequence : CompositeNode
    {
        //The constructor requires a list of child nodes to work
        public Sequence(List<Node> nodes) : base(nodes) { }

        //Returns SUCCESS if non of the chil nodes are running (hence all of the child nodes succeed)
        //Returns FAILURE if any of the child nodes fails.
        //Returns RUNNING if any of the child nodes is running
        public override NodeStates Update(BlackBoard bb)
        {
            foreach (Node node in m_nodes)
            {
                switch (node.Update(bb))
                {
                    case NodeStates.SUCCESS:
                        continue;
                    case NodeStates.FAILURE:
                        return SetNodeState(NodeStates.FAILURE);
                    case NodeStates.RUNNING:
                        return SetNodeState(NodeStates.RUNNING);
                    default:
                        return SetNodeState(NodeStates.RUNNING);
                }
            }
            return SetNodeState(NodeStates.SUCCESS);
        }
    }
}
