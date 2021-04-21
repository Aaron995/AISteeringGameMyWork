using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    /// <summary>
    /// Objects with this interface are able to take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// The current status of the interface owner.
        /// </summary>
        public UnitStatusEnum m_unitStatus { get; set; }
        /// <summary>
        /// The current stats the interface owner has.
        /// </summary>
        public UnitStats m_unitStats { get; }
        /// <summary>
        /// Used to deal damage to the interface owner.
        /// </summary>
        /// <param name="_damage">
        /// The amount of damage being dealt.
        /// </param>
        public void TakeDamage(int _damage);
    }
     
    /// <summary>
    /// Used for unit objectpooling to get the stats ready on new spawns.
    /// </summary>
    public interface IUnitPoolable: AaronScripts.ObjectPool.IPoolable
    {       
        /// <summary>
        /// Used to give the interface owner the stats.
        /// </summary>
        /// <param name="_stats">
        /// The stats the owner is getting.
        /// </param>
        void InitializeStats(UnitStats _stats);
    }

    /// <summary>
    /// Objects with this interface are able to be healed.
    /// </summary>
    public interface IHealable
    {
        /// <summary>
        /// Heal the interface owner.
        /// </summary>
        /// <param name="amount">
        /// The amount of healing to be done.
        /// </param>
        public void HealUnit(int amount);
    }

    /// <summary>
    /// Objects with this interface are able to be given extra stats.
    /// </summary>
    public interface IUpgradeable
    {
        /// <summary>
        /// To check if the object already got upgraded.
        /// </summary>
        public bool m_upgraded { get; }
        /// <summary>
        /// To upgrade the current stats of the interface owner.
        /// </summary>
        /// <param name="extraStats">
        /// Stats the interface owner will be adding to their stats.
        /// </param>
        public void UpgradeStats(UnitStats extraStats);
    }
}
