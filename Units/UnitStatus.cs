using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AaronScripts.ObjectPool;
using UnityEngine.Events;
using Units.Manager;


namespace Units
{
    /// <summary>
    /// The current status of a unit.
    /// </summary>
    public enum UnitStatusEnum
    {
        patroling,
        fleeing,
        fighting,
        traveling,
        thinking,
        chasing,
        going_to_allies,
        looking_for_allies,
        healing,
        upgrading
    }  

    /// <summary>
    /// Class used to keep other unit scripts clean, implements all interfaces needed for units as well.
    /// </summary>
    [RequireComponent(typeof(UnitBrain))]
    public class UnitStatus : MonoBehaviour, IUnitPoolable, IDamageable, IHealable, IUpgradeable
    {
        // Current state of the unit
        public UnitStatusEnum m_unitStatus { get { return unitStatus; } set { unitStatus = value; } }
        [SerializeField] private UnitStatusEnum unitStatus;
        
        // Events that can be used for animations ect for when a unit spawns,dies, upgrades or heals
        public ReturnToPoolEvent onDeconstruct { get; } = new ReturnToPoolEvent(); 
        public UnityEvent onInitialization = new UnityEvent();
        public UnityEvent onUpgrade = new UnityEvent();
        public UnityEvent onHeal = new UnityEvent();
        public UnityEvent onAttack = new UnityEvent();
        
        // Public getter for the interface of the unit stats
        public UnitStats m_unitStats { get { return unitStats; } } 
        // Private variable used by this class to update the stats.
        [SerializeField] private UnitStats unitStats; 
        // Bool to keep track if the unit already got updated onces
        public bool m_upgraded { get; private set; } = false;

        public void Initialize(Vector3 pos, Quaternion rotation)
        {
            transform.SetPositionAndRotation(pos, rotation);
            m_unitStatus = UnitStatusEnum.thinking;
            m_upgraded = false;
            gameObject.SetActive(true);            
            onInitialization.Invoke();
        }

        public void InitializeStats(UnitStats _stats)
        {
            unitStats = _stats;            
        }

        public void Deconstruct()
        {
            UnitManager.Instance.UnitDied(unitStats.m_playerID, gameObject);            
            onDeconstruct.Invoke(gameObject);
        }

        public void TakeDamage(int _damage)
        {
            unitStats.m_defense -= _damage;
            if (m_unitStats.m_defense <= 0)
            {
                Deconstruct();
            }
        }

        public void HealUnit(int amount)
        {
            onHeal.Invoke();
            unitStats.m_defense += amount;
        }

        public void UpgradeStats(UnitStats extraStats)
        {
            onUpgrade.Invoke();
            unitStats.m_attackDamage += extraStats.m_attackDamage;
            unitStats.m_attackSpeed += extraStats.m_attackSpeed;
            unitStats.m_defense += extraStats.m_defense;
            unitStats.m_movementSpeed += extraStats.m_movementSpeed;
            unitStats.m_sightRange += extraStats.m_sightRange;
            m_upgraded = true;
        }
    }
}
