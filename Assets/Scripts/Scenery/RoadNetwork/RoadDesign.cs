using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;
using Random = System.Random;

namespace Scenery.RoadNetwork {
    
    /// <summary>
    /// ScriptableObject holding all information on the look of the scene
    /// </summary>
    [CreateAssetMenu(menuName = "RoadDesign", order = 0)]
    [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
    public class RoadDesign : ScriptableObject {

        [Header("Prefabs")] 
        public GameObject roadPrefab;
        public GameObject lanePrefab;
        public GameObject laneSectionPrefab;
        public GameObject junctionPrefab;
        public GameObject roadMarkPrefab;

        [Header("Values")] 
        public int samplePrecision;
        public float sidewalkCurbWidth;
        public float offsetHeight = 0.0001f;
        
        [Header("Lane Materials")]
        public List<LaneMaterial> laneMaterials = new List<LaneMaterial>();

        [Header("RoadMark Options")] 
        public Material broken;
        public Material solid;
        public bool disableRoadMarkForCenterLaneOnJunction;
        public bool disableRoadMarkOnJunction;

        [Header("Toggles")]
        public bool closeMeshToNextMesh = true;
        public bool disableStraightSidewalkOnJunction = true;
        public bool disableSidewalkOnJunction;

        [Header("Prefabs")]
        public List<RoadObjectPrefab> roadObjectPrefabs = new List<RoadObjectPrefab>();
        
        [Header("Road Object Materials")]
        public List<RoadObjectMaterial> roadObjectMaterials = new List<RoadObjectMaterial>();

        // Random function that might be used
        private Random _random;

        /// <summary>
        /// Returns the LaneMaterial for a given type and subType
        /// </summary>
        /// <param name="laneType">The type of Lane</param>
        /// <param name="subType">The subType of the Lane</param>
        /// <returns>The Material catalog entry for this type</returns>
        public LaneMaterial GetLaneMaterial(LaneType laneType, string subType = "") {
            return laneMaterials.First(lm => lm.laneType == laneType && lm.subType == subType);
        }

        /// <summary>
        /// Returns the RoadObjectPrefab for a given type and subType.
        /// </summary>
        /// <param name="type">The type of object</param>
        /// <param name="subType">The subType of the object</param>
        /// <returns>The prefab catalog entry for this type</returns>
        public RoadObjectPrefab GetRoadObjectPrefab(RoadObjectType type, string subType = "") {
            if (subType == "random") {
                if (_random == null) _random =  new Random(Environment.TickCount);
                var found = roadObjectPrefabs.Where(rop => rop.type == type).ToArray();
                return found.Length == 0 ? null : found[_random.Next(0, found.Length)];
            }

            var toReturnA = roadObjectPrefabs.FirstOrDefault(rop => rop.type == type && rop.subType == subType);
            var toReturnB = roadObjectPrefabs.FirstOrDefault(rop => rop.type == type);

            return toReturnA ?? toReturnB;
        }

        /// <summary>
        /// Returns the RoadObjectMaterial for a given type and subType.
        /// </summary>
        /// <param name="type">The type of object</param>
        /// <param name="subType">The subType of the object</param>
        /// <returns>The Material catalog entry for this type</returns>
        public RoadObjectMaterial GetRoadObjectMaterial(RoadObjectType type, string subType = "") {
            if (subType == "random") {
                if (_random == null) _random =  new Random(Environment.TickCount);
                var found = roadObjectMaterials.Where(rop => rop.type == type).ToArray();
                return found.Length == 0 ? null : found[_random.Next(0, found.Length)];
            }

            var toReturnA = roadObjectMaterials.FirstOrDefault(rop => rop.type == type && rop.subType == subType);
            var toReturnB = roadObjectMaterials.FirstOrDefault(rop => rop.type == type);

            return toReturnA ?? toReturnB;
        }
    }

    /// <summary>
    /// Class for holding a prefab for an object along a road.
    /// </summary>
    [Serializable]
    public class RoadObjectPrefab {
        public GameObject prefab;
        public RoadObjectType type;
        public string subType;
        public float baseHeight;
        public float baseRadius;
        public float baseWidth = 1f;
        public float baseLength = 1f;
    }

    /// <summary>
    /// Material to be used for a given RoadObject.
    /// </summary>
    [Serializable]
    public class RoadObjectMaterial {
        public Material material;
        public RoadObjectType type;
        public string subType;
    }

    /// <summary>
    /// Material to be used for a given LaneType.
    /// </summary>
    [Serializable]
    public class LaneMaterial {
        public LaneType laneType;
        public string subType = "";
        public Color color;
        public Material material;
    }
}