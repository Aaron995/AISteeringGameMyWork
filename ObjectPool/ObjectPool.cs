using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace AaronScripts.ObjectPool
{
    /// <summary>
    /// Holds Unity Gameobjects to pool together and reuse objects instead of destroying and instantiating.
    /// </summary>
    public class ObjectPool
    {
        private GameObject pooledObjectPrefab; // The prefab of the item we are pooling
        private Stack<GameObject> pooledObjects = new Stack<GameObject>(); // The stack of pooled objects
        private bool allowGrowningPool; // Allows pool to grow if pool is empty but objects are needed
        private int itemIncrease; // How much items to instantiate when pool is empty and objects are needed
        private int maxPoolSize; // The max size the pool can get

        // Overloading to accept the scriptableobject
        public ObjectPool(ObjectPoolSettings settings)  
        {
            int startAmount = settings.startAmount;
            // If the start amount is 0 or lower we need to adjust to make sure the pool works
            if (settings.startAmount <= 0)
            {
                // If there is no growth allowed the pool doesn't do anything with 0 object so we return early with an error
                if (!settings.allowPoolGrowth)
                {
                    Debug.LogError("Start amount has to be at least 1 or growth needs to be allowed!");
                    return;
                }
                else
                {
                    // If growth is allowed we need to make sure the start amount isn't in the negative
                    if (settings.startAmount < 0)
                    {
                        startAmount = 0;
                    }
                }
            }

           
            
            GameObject tempObj = Object.Instantiate(settings.poolObject);
            IPoolable tempPool = tempObj.GetComponent<IPoolable>();
            if (tempPool == null) // Checks if item has poolable interface to indicate item is able to be pooled
            {
                Debug.LogError("Not a poolable object!" + tempObj.name);
                Object.Destroy(tempObj);
                return;
            }
            else // If item is poolable add it to the stack and reduce start amount.
            {
                tempPool.onDeconstruct.AddListener(AddObjectBackToPool);
                tempObj.SetActive(false);
                pooledObjects.Push(tempObj);
                startAmount--;
            }
            pooledObjectPrefab = settings.poolObject;

            for (int i = 0; i < startAmount; i++)
            {
                AddObject(settings.poolObject);
            }
            allowGrowningPool = settings.allowPoolGrowth;
            itemIncrease = settings.increasePerGrowth;

            if (settings.maxGrowthSize <= 0 && settings.allowPoolGrowth)
            {
                // If no max param gets given allow only 10 grows to it making the pool too big
                maxPoolSize = settings.maxGrowthSize + (settings.increasePerGrowth * 10); 
            }
            else if (settings.maxGrowthSize < settings.startAmount)
            {
                maxPoolSize = settings.startAmount;
            }
            else
            {
                maxPoolSize = settings.maxGrowthSize;
            }
        }

        // For non scriptable object overload
        public ObjectPool(GameObject _obj, int startAmount, bool allowGrowth = false, int increasePerGrowth = 1, int maxGrowthSize = 0) 
        {
            // If the start amount is 0 or lower we need to adjust to make sure the pool works
            if (startAmount <= 0)
            {
                // If there is no growth allowed the pool doesn't do anything with 0 object so we return early with an error
                if (allowGrowth)
                {
                    Debug.LogError("Start amount has to be at least 1 or growth needs to be allowed!");
                    return;
                }
                else
                {
                    // If growth is allowed we need to make sure the start amount isn't in the negative
                    if (startAmount < 0)
                    {
                        startAmount = 0;
                    }
                }
            }

            GameObject tempObj = Object.Instantiate(_obj);
            IPoolable tempPool = tempObj.GetComponent<IPoolable>();
            if (tempPool == null) // Checks if item has poolable interface to indicate item is able to be pooled
            {
                Debug.LogError("Not a poolable object!" + tempObj.name);
                Object.Destroy(tempObj);
                return;
            }
            else // If item is poolable add it to the stack and reduce start amount
            {
                tempPool.onDeconstruct.AddListener(AddObjectBackToPool);
                tempObj.SetActive(false);
                pooledObjects.Push(tempObj);
                startAmount--;
            }
            pooledObjectPrefab = _obj;

            for (int i = 0; i < startAmount; i++)
            {
                AddObject(_obj);
            }
            allowGrowningPool = allowGrowth;
            itemIncrease = increasePerGrowth;

            if (maxGrowthSize <= 0 && allowGrowth)
            {
                // If no max param gets given allow only 10 grows to it making the pool too big
                maxPoolSize = maxGrowthSize + (increasePerGrowth * 10); 
            }
            else if (maxGrowthSize < startAmount)
            {
                maxPoolSize = startAmount;
            }
            else
            {
                maxPoolSize = maxGrowthSize;
            }
        }       

        public GameObject GetObject()
        {
            if (pooledObjects.Count <= 0) // Check if pool is empty
            {
                if (allowGrowningPool && pooledObjects.Count < maxPoolSize)// Check if allowed to grow
                {
                    if (itemIncrease > maxPoolSize - pooledObjects.Count) // Incase last increase is going over the max
                    {
                        itemIncrease = maxPoolSize - pooledObjects.Count;
                    }

                    for (int i = 0; i < itemIncrease; i++) // Add new items
                    {
                        AddObject(pooledObjectPrefab);
                    }

                    return pooledObjects.Pop(); // Pop item and return it;
                }
                return null; // If pool is empty and not allowed to grow return null
            }
            else // If pool has object(s) in it
            {
                return pooledObjects.Pop(); // Pop item and return it;
            }

        }

        private void AddObject(GameObject _obj)
        {
            GameObject obj = Object.Instantiate(_obj); // Instantiate the object
            obj.GetComponent<IPoolable>().onDeconstruct.AddListener(AddObjectBackToPool); // Sub to event to get back in into the pool
            obj.SetActive(false); // Disable the object
            pooledObjects.Push(obj); // Push it to the stack
        }

        public void AddObjectBackToPool(GameObject _obj)
        {
            if (_obj.activeSelf) // Making sure that item is deactivated.
            {
                _obj.SetActive(false);
            }

            pooledObjects.Push(_obj);
        }
    }

    /// <summary>
    /// Base interface for objects that are going to be in the object pool.
    /// </summary>
    public interface IPoolable 
    {
        /// <summary>
        /// The gameobject that is the owner of the interface.
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// Event that is used to get the object back to the object pool.
        /// Should be called in the Deconstruct method.
        /// </summary>
        ReturnToPoolEvent onDeconstruct { get; } 
        /// <summary>
        /// Method to position and rotate the unit used to setup a unit before reusing it from the pool.
        /// </summary>
        /// <param name="pos">
        /// Posistion the unit needs to be placed on.        
        /// </param>
        /// <param name="rotation">
        /// The rotation the unit will be rotated to.
        /// </param>
        void Initialize(Vector3 pos, Quaternion rotation); 
        /// <summary>
        /// Method to deactivate to object instead of calling destroy on it.
        /// </summary>
        void Deconstruct(); 
    }

    /// <summary>
    /// An Unity event with a GameObject Paramater.
    /// </summary>
    public class ReturnToPoolEvent : UnityEvent<GameObject> 
    {
    }

}
