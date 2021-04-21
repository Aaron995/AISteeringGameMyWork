using UnityEngine;

namespace ObjectPooling
{
    [CreateAssetMenu(fileName = "Object Pool settings", menuName = "Object Pool/Object Pool settings")]
    public class ObjectPoolSettings : ScriptableObject
    {
        /// <summary>
        /// The Gameobject used in the object pool.
        /// </summary>
        public GameObject poolObject;
        /// <summary>
        /// The amount of items that starts in the object pool.
        /// </summary>
        public int startAmount;
        /// <summary>
        /// Allows the pool to grow when more objects are needed.
        /// </summary>
        public bool allowPoolGrowth;
        /// <summary>
        /// The amount of items being added when the pools growths during runtime.
        /// </summary>
        public int increasePerGrowth;
        /// <summary>
        /// The maximum amount the pool is allowed to grow to.
        /// </summary>
        public int maxGrowthSize;
    }
}