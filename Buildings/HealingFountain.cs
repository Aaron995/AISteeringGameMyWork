using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units.Buildings
{
    public class HealingFountain : MonoBehaviour
    {
        // The duration between each healing wave
        [SerializeField] private float healingInterval = 5f;        
        // The amount of healing that will happen per wave
        [SerializeField] private int healAmount = 5;

        // Timer used to track intervals
        private float healingTimer;

        [SerializeField] private List<GameObject> unitsInRange = new List<GameObject>();
        void Update()
        {
            healingTimer += Time.deltaTime;
            if (healingTimer >= healingInterval)
            {
                healingTimer = 0;
                foreach (GameObject unit in unitsInRange)
                {
                    unit.GetComponent<IHealable>().HealUnit(healAmount);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            IHealable healable = other.gameObject.GetComponent<IHealable>();
            if (healable != null)
            {
                unitsInRange.Add(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            unitsInRange.Remove(other.gameObject);
        }
    }
}
