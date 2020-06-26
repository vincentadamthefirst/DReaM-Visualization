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
            
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            var position = Parent.EvaluatePoint(S, m * T, ZOffset);

            GameObject initialized = null;
            var scaleBaseRadius = 1f;

            switch (RoadObjectType) {
                case RoadObjectType.Tree:
                    initialized = Instantiate(RoadDesign.tree, transform, true);
                    var scaleA = 2 * Radius / RoadDesign.treeBaseRadius;
                    var scaleB = Height / RoadDesign.treeBaseHeight;
                    
                    initialized.transform.SetGlobalScale(new Vector3(scaleA, scaleB, scaleA));
                    
                    break;
                case RoadObjectType.StreetLamp:
                    initialized = Instantiate(RoadDesign.streetLight, transform, true);
                    Height -= 1;

                    var middleLocalScale = initialized.transform.GetChild(1).localScale;
                    initialized.transform.GetChild(1).localScale =
                        new Vector3(middleLocalScale.x, Height, middleLocalScale.z);
                    initialized.transform.GetChild(2).localPosition += new Vector3(0, Height - 1, 0);
                    scaleBaseRadius = RoadDesign.streetLightBaseRadius;
                    
                    var scale = 2 * Radius / scaleBaseRadius;
                    transform.GetChild(0).SetGlobalScale(new Vector3(scale, 1, scale));

                    break;
                case RoadObjectType.Pole:
                    initialized = Instantiate(RoadDesign.pole, transform, true);

                    middleLocalScale = initialized.transform.GetChild(0).localScale;
                    initialized.transform.GetChild(0).localScale =
                        new Vector3(middleLocalScale.x, Height, middleLocalScale.z);
                    initialized.transform.GetChild(1).localPosition += new Vector3(0, Height - 1, 0);
                    scaleBaseRadius = RoadDesign.poleBaseRadius;
                    
                    scale = 2 * Radius / scaleBaseRadius;
                    transform.GetChild(0).SetGlobalScale(new Vector3(scale, 1, scale));

                    break;
                case RoadObjectType.CrossWalk:
                case RoadObjectType.ParkingSpace:
                case RoadObjectType.Building:
                case RoadObjectType.None:
                    break;
                default:
                    break;
            }

            if (initialized != null) {
                initialized.transform.position = position;
                
                
            }
            
            transform.parent = Parent.transform;
        }
    }
}