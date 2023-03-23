using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scenery.RoadNetwork {
    
    /// <summary>
    /// Class representing a LaneSection from OpenDrive. Provides access methods for Lanes and parameters as well as
    /// function methods to communicate with its children Lanes or with its parent Road.
    /// </summary>
    public class LaneSection : VisualizationElement {
        
        /// <summary>
        /// Left (in road direction) Lane-Objects for this section
        /// </summary>
        public List<Lane> LeftLanes { get; private set; }
        
        /// <summary>
        /// CenterLane-object for this section
        /// </summary>
        public Lane CenterLane { get; private set; }
        
        /// <summary>
        /// Right (in road direction) Lane-objects for this section
        /// </summary>
        public List<Lane> RightLanes { get; private set; }

        /// <summary>
        /// All Lane-objects mapped to their id (string)
        /// </summary>
        public Dictionary<string, Lane> LaneIdMappings { get; }
        
        /// <summary>
        /// Starting distance on the road for this lane section
        /// </summary>
        public float S { get; set; }
        
        /// <summary>
        /// Length of this lane section
        /// </summary>
        public float Length { get; set; }
        
        /// <summary>
        /// Road-object this LaneSection-object belongs to
        /// </summary>
        public Road Parent { get; set; }

        /// <summary>
        /// If this section is completely within a line geometry, will be used to simplify mesh generation for children
        /// Lanes. Gets set in parent Road.
        /// </summary>
        public bool CompletelyOnLineSegment { get; set; }

        public LaneSection() {
            LeftLanes = new List<Lane>();
            RightLanes = new List<Lane>();
            LaneIdMappings = new Dictionary<string, Lane>();
        }

        /// <summary>
        /// Adds a left Lane to the list (in road direction). Will sort the list afterwards (based on the sign() of the
        /// id of the Lane.
        /// </summary>
        /// <param name="lane">The Lane to be added.</param>
        public void AddLeftLane(Lane lane) {
            LeftLanes.Add(lane);
            var ordered = LeftLanes.AsEnumerable().OrderBy(l => l.LaneIdInt);
            LeftLanes = ordered.ToList();

            LaneIdMappings[lane.Id] = lane;
        }
        
        /// <summary>
        /// Adds a right Lane to the list (in road direction). Will sort the list afterwards (based on the sign() of the
        /// id of the Lane.
        /// </summary>
        /// <param name="lane">The Lane to be added.</param>
        public void AddRightLane(Lane lane) {
            RightLanes.Add(lane);
            var ordered = RightLanes.AsEnumerable().OrderByDescending(l => l.LaneIdInt);
            RightLanes = ordered.ToList();
            
            LaneIdMappings[lane.Id] = lane;
        }

        /// <summary>
        /// Sets the center Lane.
        /// </summary>
        /// <param name="lane">The new center Lane.</param>
        public void SetCenterLane(Lane lane) {
            CenterLane = lane;
            LaneIdMappings[lane.Id] = lane;
        }

        /// <summary>
        /// Prepares this LaneSection and its children for mesh generation by evaluating neighbors of lanes.
        /// </summary>
        public void PrepareNeighbors() {
            for (var i = 0; i < LeftLanes.Count; i++) {
                LeftLanes[i].InnerNeighbor = i == 0 ? CenterLane : LeftLanes[i - 1];
                LeftLanes[i].OuterNeighbor = i == LeftLanes.Count - 1 ? null : LeftLanes[i + 1];
            }
            
            for (var i = 0; i < RightLanes.Count; i++) {
                RightLanes[i].InnerNeighbor = i == 0 ? CenterLane : RightLanes[i - 1];
                RightLanes[i].OuterNeighbor = i == RightLanes.Count - 1 ? null : RightLanes[i + 1];
            }

            CenterLane.OuterNeighbor = RightLanes.Count != 0 ? RightLanes[0] : null;
            CenterLane.InnerNeighbor = LeftLanes.Count != 0 ? LeftLanes[0] : null;
        }

        /// <summary>
        /// Starts the mesh generation for this LaneSections children Lanes.
        /// </summary>
        public void StartMeshGeneration() {
            CenterLane.GenerateAndSetMesh();
            CenterLane.RoadMark.GenerateMesh();
            
            foreach (var entry in LaneIdMappings) {
                entry.Value.GenerateAndSetMesh();
            }
        }

        /// <summary>
        /// Evaluates a point on the geometry of the parent Road. Transforms the local s into global S value.
        /// </summary>
        /// <param name="s">Lane-local s value</param>
        /// <param name="t">t value</param>
        /// <param name="h">height value (used for sidewalks)</param>
        /// <returns>evaluated point</returns>
        public Vector3 EvaluatePoint(float s, float t, float h = 0f) {
            return Parent.EvaluatePoint(S + s, t, h);
        }

        /// <summary>
        /// Evaluates the heading of the geometry of the parent road. Transforms the local s into global S value
        /// </summary>
        /// <param name="s">Lane-local s value</param>
        /// <returns>evaluated heading</returns>
        public float EvaluateHeading(float s) {
            return Parent.EvaluateHeading(S + s);
        }

        public override ElementOrigin ElementOrigin => ElementOrigin.OpenDrive;
    }
}