using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;

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

        [Header("RoadMark Options")] 
        public Material broken;
        public Material solid;
        public bool disableRoadMarkForCenterLaneOnJunction;
        public bool disableRoadMarkOnJunction;

        [Header("Toggles")]
        public bool closeMeshToNextMesh = true;
        public bool disableStraightSidewalkOnJunction = true;
        public bool disableSidewalkOnJunction;

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
    }
}