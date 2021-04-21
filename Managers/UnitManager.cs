using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStarPathfinding;

namespace Units.Manager
{
    /// <summary>
    /// Singleton Manager used for Units to gather information they all use.
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        // Instance reference
        public static UnitManager Instance;       

        // The settings for the grid used in pathfinding
        [SerializeField] private WorldGrid.GridSettings pathfindingGridSettings;

        // The transform of the weapon upgrade location in the world.
        [SerializeField] private Transform weaponUpgrade;
        /// <summary>
        /// Location of the weapon upgrade in the world space.
        /// </summary>
        public Vector3 m_weaponUpgrade { get { return weaponUpgrade.position; } }
        // The transform of the healing fountain.
        [SerializeField] private Transform healingFountain;
        /// <summary>
        /// Location of the healing fountain in the world space.
        /// </summary>
        public Vector3 m_healingFountain { get { return healingFountain.position; } }

        // Reference to the pathfinding class.
        private PathFinding pathFinding;
        // Array of the castle scripts in the scene.
        private Castle[] teamCastles;
        // Dictionary that tracks the currently spawned in units with their team ID.
        private Dictionary<int,List<GameObject>> spawnedUnits;
        // List of paths that are commonly used to reduce pathfinding calls.
        private List<PathData> commonPaths;

        void Awake()
        {
            // If there is no instance already make this the instance.
            if (instance == null)
            {
                instance = this;
            }
            // Destroy this if there is already another instance that isn't this.
            else if (instance != this)
            {
                Destroy(this);
            }

            // Initalize new pathfinding.
            pathFinding = new PathFinding(pathfindingGridSettings, this);

            // Find all castle scripts in the scene.
            teamCastles = FindObjectsOfType<Castle>();

            // Initalize the spawnedUnits dict.
            spawnedUnits = new Dictionary<int, List<GameObject>>();
            foreach (Castle castle in teamCastles)
            {
                spawnedUnits.Add(castle.m_teamID, new List<GameObject>());
            }

            // Generate the commonly used paths.
            GenerateCommonPaths();
        }

        /// <summary>
        /// Clears all known common paths and regenerates them.
        /// </summary>
        public void GenerateCommonPaths()
        {
            // Clear the list of common path datas
            commonPaths = new List<PathData>();

            // Loop through each castle
            foreach (Castle castle in teamCastles)
            {
                // Request a path from castle to each other castle
                foreach (Castle castle1 in teamCastles)
                {
                    // We dont need a path from the same castle to itself
                    if (castle.m_teamID == castle1.m_teamID)
                    {
                        continue;
                    }
                    pathFinding.m_requestManager.RequestPath(pathFinding.m_grid.GetGridObject(castle.m_castleAttackPoints[0].transform.position),
                    pathFinding.m_grid.GetGridObject(GetRandomPositionFromTransformArray(castle1.m_castleAttackPoints)),PathComplete);
                }

                // Request a path from castle to weapon upgrade and healing fountain
                pathFinding.m_requestManager.RequestPath(pathFinding.m_grid.GetGridObject(castle.m_castleAttackPoints[0].transform.position),
                    pathFinding.m_grid.GetGridObject(m_weaponUpgrade), PathComplete);
                pathFinding.m_requestManager.RequestPath(pathFinding.m_grid.GetGridObject(castle.m_castleAttackPoints[0].transform.position),
                   pathFinding.m_grid.GetGridObject(m_healingFountain), PathComplete);
            }
        }
        /// <summary>
        /// Method used for pathfinding to return after the pathfinding request has finished.
        /// </summary>
        public void PathComplete(PathNode[] path, bool success, PathRequest pathRequest)
        {
            if (success)
            {
                // Add the new path to our common path list
                commonPaths.Add(new PathData(pathRequest.nodeStart.GetMiddleWorldPosition(),
                    pathRequest.nodeEnd.GetMiddleWorldPosition(), path));
            }
            else
            {
                // If we cannot generate a path log error as this method should only be called with commonly used paths.
                Debug.LogError("Path from: "+ pathRequest.nodeStart + " to: " + pathRequest.nodeEnd + " FAILED!");
            }
        }

        /// <summary>
        /// Adds the unit to the spawned unit dict.
        /// </summary>
        /// <param name="teamID">
        /// The team ID of the unit.
        /// </param>
        /// <param name="unit">
        /// The GameObject of the unit spawned in.
        /// </param>
        public void SpawnedUnit(int teamID, GameObject unit)
        {
            spawnedUnits[teamID].Add(unit);
        }

        /// <summary>
        /// Remove the unit from the spawned unit dict.
        /// </summary>
        /// <param name="teamID">
        /// The team ID of the unit.
        /// </param>
        /// <param name="unit">
        /// The GameObject of the unit who died.
        /// </param>
        public void UnitDied(int teamID,GameObject unit)
        {
            spawnedUnits[teamID].Remove(unit);
        }

        /// <summary>
        /// Gets all enemy units in a given range of a position. This includes enemy castles. 
        /// </summary>
        /// <param name="range">
        /// The max range an enemy can be seen by.
        /// </param>
        /// <param name="teamID">
        /// The team ID of team the requester is from.
        /// </param>
        /// <param name="position">
        /// Position of where we look for enemies from.
        /// </param>
        /// <returns>
        /// GameObject array of all enemies in range.
        /// </returns>
        public GameObject[] GetEnemiesInRange(int range, int teamID, Vector3 position)
        {
            List<GameObject> enemiesInRange = new List<GameObject>();
            // Get all the team ids and remove the ID from the unit requesting this
            List<int> ids = new List<int>(spawnedUnits.Keys);
            ids.Remove(teamID);
            // Loop through each other enemy list
            foreach (int id in ids)
            {
                foreach (GameObject unit in spawnedUnits[id])
                {
                    if (Vector3.Distance(position, unit.transform.position) <= range)
                    {
                        enemiesInRange.Add(unit);
                    }
                }
            }

            // Check of each castle in-range as well.
            foreach (Castle castle in teamCastles)
            {
                if (castle.m_teamID == teamID)
                {
                    continue;
                }

                if (Vector3.Distance(position, castle.gameObject.transform.position) <= range)
                {
                    enemiesInRange.Add(castle.gameObject);
                }
            }
        
            return enemiesInRange.ToArray();
        }

        /// <summary>
        /// Gets all allied units in a given range of a position. This excludes friendly castles.
        /// </summary>
        /// <param name="range">
        /// The max range an ally can be seen by.
        /// </param>
        /// <param name="teamID">
        /// The team ID of team the requester is from.
        /// </param>
        /// <param name="rootUnit">
        /// The unit requesting allies in range.
        /// </param>
        /// <returns>
        /// Returns a GameObject with all Allied units within the given range.
        /// </returns>
        public GameObject[] GetAlliesInRange(int range, int teamID, GameObject rootUnit)
        {
            List<GameObject> alliesInRange = new List<GameObject>();
            // Loop through all spawned in allied units.
            foreach (GameObject unit in spawnedUnits[teamID])
            {
                // If one of the spawned units is itself skip it.
                if (unit == rootUnit)
                {
                    continue;
                }

                if (Vector3.Distance(rootUnit.transform.position, unit.transform.position) <= range)
                {
                    alliesInRange.Add(unit);
                }
            }
            return alliesInRange.ToArray();
        }

        /// <summary>
        /// Gets the world posistion of a random enemy castle.
        /// </summary>
        /// <param name="playerID">
        /// The team ID of team the requester is from.
        /// </param>
        /// <returns>
        /// World location of a random enemy castle.
        /// </returns>
        public Vector3 GetRandomEnemyCastleLocation(int playerID)
        {
            int random = Random.Range(0, teamCastles.Length);
            while (teamCastles[random].m_teamID == playerID)
            {
                random = Random.Range(0, teamCastles.Length);
            }

            return GetRandomPositionFromTransformArray(teamCastles[random].m_castleAttackPoints);
        }

        /// <summary>
        /// Checks to see if given location is an enemy castle.
        /// </summary>
        /// <param name="destination">
        /// The location where the unit is heading to.
        /// </param>
        /// <param name="playerID">
        /// The team ID of team the requester is from.
        /// </param>
        /// <returns>
        /// If the given destination is an enemy castle.
        /// </returns>
        public bool IsTargetEnemyCastle(Vector3 destination, int playerID)
        {
            foreach (Castle castle in teamCastles)
            {
                // Skip if the castle team ID is the same as the given ID.
                if (castle.m_teamID == playerID)
                {
                    continue;
                }

                foreach (Transform transform in castle.m_castleAttackPoints)
                {
                    // If the caslte attack point is the same as the given destination return true.
                    if (transform.position == destination)
                    {
                        return true;
                    }
                }
            }
            // If no matching location got found return false.
            return false;
        }

        /// <summary>
        /// Get the castle GameObject based off location.
        /// </summary>
        /// <param name="destination">
        /// World location of where the castle is.
        /// </param>
        /// <returns>
        /// The GameObject of the enemy castle, returns null if the destination isn't an enemy castle.
        /// </returns>
        public GameObject GetCastleObject(Vector3 destination)
        {
            foreach (Castle castle in teamCastles)
            {
                foreach (Transform transform in castle.m_castleAttackPoints)
                {
                    if (transform.position == destination)
                    {
                        return castle.gameObject;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get the location of the friendly castle of the requester.
        /// </summary>
        /// <param name="playerID">
        /// The team ID of team the requester is from.
        /// </param>
        /// <returns>
        /// World position of the unit friendly castle, returns Vector3.zero if there is no castle found for the given ID.
        /// </returns>
        public Vector3 GetFriendlyCastleLocation(int playerID)
        {
            Castle castle = null;
            foreach (Castle _castle in teamCastles)
            {
                if (_castle.m_teamID == playerID)
                {
                    castle = _castle;
                    break;
                }
            }

            if (castle == null)
            {
                Debug.LogError("No castle found for " + playerID + " ID!");
                return Vector3.zero;
            }

            return GetRandomPositionFromTransformArray(castle.m_castleAttackPoints);
        }

        /// <summary>
        /// Get the world locations of the guard paths based on team ID and unit behaviour.
        /// </summary>
        /// <param name="playerID">
        /// The team ID of team the requester is from.
        /// </param>
        /// <param name="behaviour">
        /// The behaviour of the requester.
        /// </param>
        /// <returns>
        /// An Vector3 Array with the world positions of the waypoints.
        /// Returns an empty array if the behaviour isn't a guard path or the team ID isn't found.
        /// </returns>
        public Vector3[] GetGuardPath(int playerID, UnitBehaviourEnum behaviour)
        {
            Castle castle = null;
            foreach (Castle _castle in teamCastles)
            {
                if (_castle.m_teamID == playerID)
                {
                    castle = _castle;
                    break;
                }
            }
            if (castle != null)
            {
                switch (behaviour)
                {
                    case UnitBehaviourEnum.guard_path_a:
                        return TransformArrayPositionsToVector3Array(castle.m_guardPathA);                    
                    case UnitBehaviourEnum.guard_path_b:
                        return TransformArrayPositionsToVector3Array(castle.m_guardPathB);                    
                    default:
                        Debug.LogError("Non guard path behaviour tried accessing a guard path!");
                        break;
                }
            }
            Debug.LogError("No castle found for " + playerID + " ID!");
            return new Vector3[0];
        }

        /// <summary>
        /// Request a path from a given position to a given destination.
        /// </summary>
        /// <param name="callBackDelegate">
        /// Delegate to return to after pathfinding has finished.
        /// </param>
        /// <param name="position">
        /// Start position of the pathfinding.
        /// </param>
        /// <param name="destinationPosition">
        /// End point of the pathfinding.
        /// </param>
        public void GetPath(CallBackDelegate callBackDelegate, Vector3 startPosition, Vector3 destinationPosition)
        {
            foreach (PathData pathData in commonPaths)
            {
                if (Vector3.Distance(pathData.endPosition, destinationPosition) <= pathfindingGridSettings.cellSize * 2
                    && Vector3.Distance(pathData.startPosition, startPosition) <= pathfindingGridSettings.cellSize * 2)
                {
                    callBackDelegate.Invoke(pathData.path, true, new PathRequest(
                        pathData.path[0], pathData.path[pathData.path.Length - 1], callBackDelegate));
                    return;
                }
            }

            pathFinding.m_requestManager.RequestPath(
                pathFinding.m_grid.GetGridObject(startPosition),
                pathFinding.m_grid.GetGridObject(destinationPosition),
                callBackDelegate);
        }


        private Vector3[] TransformArrayPositionsToVector3Array(Transform[] transforms)
        {
            List<Vector3> vectorList = new List<Vector3>();
            foreach (Transform transform in transforms)
            {
                vectorList.Add(transform.position);
            }
            return vectorList.ToArray();
        }    

        private Vector3 GetRandomPositionFromTransformArray(Transform[] transforms)
        {
            int random = Random.Range(0, transforms.Length);
            return transforms[random].position;
        }

    #if (UNITY_EDITOR)

        private void OnDrawGizmos()
        {
            if (pathFinding != null)
            {
                pathFinding.OnDrawGizmos();
            }
        }
    #endif

        struct PathData
        {
            public Vector3 startPosition;
            public Vector3 endPosition;
            public PathNode[] path;

            public PathData(Vector3 startPos, Vector3 EndPos, PathNode[] _path)
            {
                startPosition = startPos;
                endPosition = EndPos;
                path = _path;
            }
        }
    }
}
