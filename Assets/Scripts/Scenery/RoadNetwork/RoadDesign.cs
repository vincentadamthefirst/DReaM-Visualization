using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Scenery.RoadNetwork {
    
    [CreateAssetMenu(menuName = "RoadDesign", order = 0)]
    public class RoadDesign : ScriptableObject {

        [Header("Prefabs")] 
        public GameObject roadPrefab;
        public GameObject lanePrefab;
        public GameObject laneSectionPrefab;
        public GameObject roadMarkPrefab;

        [Header("Values")] 
        public int samplePrecision;
        public float sidewalkHeight;
        public float sidewalkCurbWidth;
        
        [Header("Main Materials")]
        public Material roadBase;
        public Material sidewalkCurb;
        public Material sidewalkEndCap;
        public Material sidewalk;
        public Material bikingMaterial;
        public Material restrictedMaterial;
        public Material none;

        [Header("RoadMark Materials")] 
        public Material broken;
        public Material solid;
    }

    public enum LaneMaterialSpecification {
        SidewalkCurb, SidewalkMain, SidewalkEndCap,
        LaneMarkLeftSolid, LaneMarkRightSolid, LaneMarkBothSolid,
        LaneMarkLeftBroken, LaneMarkRightBroken, LaneMarkBothBroken,
    }
}