using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Units
{
    public enum UnitBehaviourEnum
    {
        aggressive,
        defensive,
        loyal,
        wanderer,
        guard_path_a,
        guard_path_b            
    }

    [Serializable]// Serializable to make it showable in unity inspector to help debugging
    public struct UnitStats 
    {
        public UnitStats(int _movementSpeed, int _attackSpeed, int _attackDamage, int _sightRange, int _defense, UnitBehaviourEnum _behaviour, int _playerID)
        {
            m_movementSpeed = _movementSpeed;
            m_attackSpeed = _attackSpeed;
            m_attackDamage = _attackDamage;
            m_sightRange = _sightRange;
            m_defense = _defense;
            m_behaviour = _behaviour;
            m_playerID = _playerID;
        }

        public int m_movementSpeed; // The current movement speed of a unit
        public int m_attackSpeed; // The current attack speed of a unit
        public int m_attackDamage; // The current attack damage of a unit
        public int m_sightRange; // The sight range of a unit
        public int m_defense; // The current defense of a unit  
        public UnitBehaviourEnum m_behaviour; // The behaviour of a unit
        public int m_playerID; // A number to represent on what team they are
    }
}
