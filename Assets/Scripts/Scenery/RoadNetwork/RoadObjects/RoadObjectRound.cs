using System;
using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork.RoadObjects {
    
    /// <summary>
    /// Class representing a round RoadObject. Can be a generic cylinder or a predefined model. Currently supported
    /// predefined models:
    /// - Streetlamp
    /// - Pole
    /// </summary>
    public class RoadObjectRound : RoadObject {
        public float Radius { get; set; }

        protected override void Repeat() {
            if (RepeatParameters == null) return;

            var start = RepeatParameters.SStart;
            var end = RepeatParameters.Length;
            var dist = RepeatParameters.Distance;
            
            for (var s = 0f; s <= end; s += dist) {
                // Create a copy of the current RoadObject and change some values for it
                var newChild = Instantiate(this);
                newChild.RepeatParameters = null;
                newChild.Heading = Heading;
                newChild.T = RepeatParameters.GetT(s);
                newChild.ZOffset = RepeatParameters.GetZ(s);
                newChild.Height = RepeatParameters.GetHeight(s);
                newChild.S = s + start;
                newChild.Orientation = Orientation;
                newChild.Parent = Parent;
                newChild.RoadObjectType = RoadObjectType;
                newChild.SubType = SubType;
                newChild.RoadDesign = RoadDesign;
                newChild.name = name;
                newChild.Radius = Radius;
                newChild.Show();
            }

            markedForDelete = true;
        }

        public override void Show() {
            Repeat(); // repeat if the parameters are set
            if (markedForDelete) return;

            var rop = RoadDesign.GetRoadObjectPrefab(RoadObjectType, SubType);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (RoadObjectType) {
                case RoadObjectType.Tree:
                    ShowTree(rop);
                    break;
                case RoadObjectType.StreetLamp:
                    ShowStreetLamp(rop);
                    break;
                case RoadObjectType.Pole:
                    ShowPole(rop);
                    break;
                case RoadObjectType.Building:
                    ShowBuilding();
                    break;
                case RoadObjectType.CrossWalk:
                case RoadObjectType.ParkingSpace:
                case RoadObjectType.None: // these types are no supported round objects, destroy this object
                    markedForDelete = true;
                    break;
            }

            transform.parent = Parent.transform;
        }

        private void ShowStreetLamp(RoadObjectPrefab rop) {
            if (rop == null) return;
            
            var streetLamp = Instantiate(rop.prefab, transform, true);
            Height -= 1;

            var middleLocalScale = streetLamp.transform.GetChild(1).localScale;
            streetLamp.transform.GetChild(1).localScale =
                new Vector3(middleLocalScale.x, Height, middleLocalScale.z);
            streetLamp.transform.GetChild(2).localPosition += new Vector3(0, Height - 1, 0);

            var scale = 2 * Radius / rop.baseRadius;
            transform.GetChild(0).SetGlobalScale(new Vector3(scale, 1, scale));
            
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            streetLamp.transform.position = Parent.EvaluatePoint(S, m * T, ZOffset);
        }

        private void ShowPole(RoadObjectPrefab rop) {
            if (rop == null) return;
            
            var pole = Instantiate(rop.prefab, transform, true);

            var middleLocalScale = pole.transform.GetChild(0).localScale;
            pole.transform.GetChild(0).localScale =
                new Vector3(middleLocalScale.x, Height, middleLocalScale.z);
            pole.transform.GetChild(1).localPosition += new Vector3(0, Height - 1, 0);
            var scaleBaseRadius = rop.baseRadius;
                    
            var scale = 2 * Radius / scaleBaseRadius;
            transform.GetChild(0).SetGlobalScale(new Vector3(scale, 1, scale));
            
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            pole.transform.position = Parent.EvaluatePoint(S, m * T, ZOffset);
        }

        private void ShowTree(RoadObjectPrefab rop) {
            if (rop == null) return;
            
            var tree = Instantiate(rop.prefab, transform, true);
            var scaleA = 2 * Radius / rop.baseRadius;
            var scaleB = Height / rop.baseHeight;
                    
            tree.transform.SetGlobalScale(new Vector3(scaleA, scaleB, scaleA));
            
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            tree.transform.position = Parent.EvaluatePoint(S, m * T, ZOffset);
            
            var completeHdg = Parent.EvaluateHeading(S) + Heading;
            Debug.Log("Rotating to " + Parent.EvaluateHeading(S) + " + " + Heading);
            tree.transform.Rotate(Vector3.up, Mathf.Rad2Deg * completeHdg);
        }

        private void ShowBuilding() {
            var buildingBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            buildingBase.transform.SetGlobalScale(new Vector3(Radius * 2, Height / 2, Radius * 2));
            buildingBase.GetComponent<MeshRenderer>().material =
                RoadDesign.GetRoadObjectMaterial(RoadObjectType, SubType).material;
            
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            buildingBase.transform.position = Parent.EvaluatePoint(S, m * T, ZOffset + Height / 2f);
        }
    }
}