using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public class Wait : DecorationNode
    {
        private readonly float waitTime;
        private float timePassed = 0.0f;

        public Wait(Node node, float waitTime) : base(node) 
        {
            this.waitTime = waitTime;
        }

        public override void Start()
        {
            timePassed = 0.0f;
        }

        public override NodeStates Update(BlackBoard bb)
        {
            timePassed += bb.dt;
            if (timePassed < waitTime)
            {
                return SetNodeState(NodeStates.RUNNING);
            }

            return m_node.Update(bb);
        }
    }
}
