using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Scenery.RoadNetwork.RoadObjects;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Scenery.RoadNetwork {
    
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
        public float sidewalkHeight;
        public float sidewalkCurbWidth;
        public float offsetHeight = 0.0001f;
        
        [Header("Main Materials")]
        public Material roadBase;
        public Material sidewalkCurb;
        public Material sidewalk;
        public Material roadBiking;
        public Material roadRestricted;
        public Material none;
        public Material terrain;

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

        private Random _random;

        public Material GetMaterialForLaneType(LaneType laneType) {
            switch (laneType) {
                case LaneType.None:
                case LaneType.Sidewalk:
                    // handled in Lane
                    return none;
                case LaneType.Driving:
                    return roadBase;
                case LaneType.Biking:
                    return roadBiking;
                case LaneType.Restricted:
                    return roadRestricted;
                default:
                    return none;
            }
        }

        public RoadObjectPrefab GetRoadObjectPrefab(RoadObjectType type, string subType = "") {
            if (_random == null) _random =  new Random(Environment.TickCount);
            
            if (subType == "random") {
                
                var found = roadObjectPrefabs.Where(rop => rop.type == type).ToArray();
                return found.Length == 0 ? null : found[_random.Next(0, found.Length)];
            }

            RoadObjectPrefab toReturnA, toReturnB;
            toReturnA = roadObjectPrefabs.FirstOrDefault(rop => rop.type == type && rop.subType == subType);
            toReturnB = roadObjectPrefabs.FirstOrDefault(rop => rop.type == type);

            return toReturnA ?? toReturnB;
        }

        public RoadObjectMaterial GetRoadObjectMaterial(RoadObjectType type, string subType = "") {
            if (_random == null) _random =  new Random(Environment.TickCount);
            
            if (subType == "random") {
                
                var found = roadObjectMaterials.Where(rop => rop.type == type).ToArray();
                return found.Length == 0 ? null : found[_random.Next(0, found.Length)];
            }

            RoadObjectMaterial toReturnA, toReturnB;
            toReturnA = roadObjectMaterials.FirstOrDefault(rop => rop.type == type && rop.subType == subType);
            toReturnB = roadObjectMaterials.FirstOrDefault(rop => rop.type == type);

            return toReturnA ?? toReturnB;
        }
    }

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

    [Serializable]
    public class RoadObjectMaterial {
        public Material material;
        public RoadObjectType type;
        public string subType;
    }
}