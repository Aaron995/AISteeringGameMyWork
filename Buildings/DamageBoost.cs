using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units.Buildings
{
    public class DamageBoost : MonoBehaviour
    {
        // Stats the units will get when they enter the range, only the first 5 numbers matter
        [SerializeField]
        private UnitStats upgradeStats = new UnitStats(
            0,5,5,0,0,UnitBehaviourEnum.defensive,42069
            );  

        private void OnTriggerEnter(Collider other)
        {
            // Check if the other collider is able to be upgraded
            IUpgradeable upgradeable = other.gameObject.GetComponent<IUpgradeable>();
            if (upgradeable != null)
            {
                if (!upgradeable.m_upgraded)
                {
                    upgradeable.UpgradeStats(upgradeStats);
                }
            }
        }


    }
}
