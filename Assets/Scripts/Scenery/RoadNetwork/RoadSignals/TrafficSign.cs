using System;
using Scenery.RoadNetwork.RoadObjects;
using TMPro;
using UnityEngine;

namespace Scenery.RoadNetwork.RoadSignals {
    public class TrafficSign : RoadSignal {
        
        /// <summary>
        /// The signs type
        /// </summary>
        public TrafficSignType Type { get; set; }
        
        /// <summary>
        /// The signs subType
        /// </summary>
        public string SubType { get; set; }
        
        /// <summary>
        /// The orientation of the sign (left or right side of the road)
        /// </summary>
        public RoadObjectOrientation Orientation { get; set; }
        
        /// <summary>
        /// The s coordinate of the sign
        /// </summary>
        public float S { get; set; }
        
        /// <summary>
        /// The t coordinate of the sign
        /// </summary>
        public float T { get; set; }
        
        /// <summary>
        /// The z Offset of the sign (from z = 0)
        /// </summary>
        public float ZOffset { get; set; }
        
        /// <summary>
        /// The heading of the sign (based on the heading of the road at the given s value of the sign)
        /// </summary>
        public float HOffset { get; set; }
        
        /// <summary>
        /// The overall RoadDesign for the visualization of the road network
        /// </summary>
        public RoadDesign RoadDesign { get; set; }

        /// <summary>
        /// The Road this sign belongs to
        /// </summary>
        public Road Parent { get; set; }

        public override void Show() {
            transform.parent = Parent.transform;

            GameObject trafficSignPrefab = null;
            if (Type == TrafficSignType.RightOfWayBegin || Type == TrafficSignType.RightOfWayNextIntersection) {
                trafficSignPrefab = Resources.Load<GameObject>($"Prefabs/Objects/RoadNetwork/TrafficSigns/RightOfWayNextIntersection");
            } else {
                trafficSignPrefab = Resources.Load<GameObject>($"Prefabs/Objects/RoadNetwork/TrafficSigns/{Type.ToString()}");
            }
            if (trafficSignPrefab == null) return;

            var trafficSign = Instantiate(trafficSignPrefab, transform, true);
            trafficSign.layer = 19;
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            trafficSign.transform.position = Parent.EvaluatePoint(S, m * T, ZOffset - 3.8835f); 
            // FIXME quick hack for german signs: height of 3.8835 is deducted every time
            
            var completeHdg = Mathf.PI + Parent.EvaluateHeading(S) + HOffset;
            trafficSign.transform.Rotate(Vector3.up, Mathf.Rad2Deg * completeHdg);
            
            trafficSign.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            
            switch (Type) {
                case TrafficSignType.Stop:
                    break;
                case TrafficSignType.GiveWay:
                    // trafficSign.transform.Rotate(Vector3.up, -90);
                    break;
                case TrafficSignType.RightOfWayNextIntersection:
                    break;
                case TrafficSignType.MaximumSpeedLimit:
                    trafficSign.transform.Find("Text").GetComponent<TextMeshPro>().text = SubType;
                    break;
                case TrafficSignType.Undefined:
                    break;
                case TrafficSignType.MinimumSpeedLimit:
                    break;
                case TrafficSignType.EndOfMaximumSpeedLimit:
                    break;
                case TrafficSignType.EndOfMinimumSpeedLimit:
                    break;
                case TrafficSignType.EndOffAllSpeedLimitsAndOvertakingRestrictions:
                    break;
                case TrafficSignType.TownBegin:
                    break;
                case TrafficSignType.TownEnd:
                    break;
                case TrafficSignType.Zone30Begin:
                    break;
                case TrafficSignType.Zone30End:
                    break;
                case TrafficSignType.TrafficCalmedDistrictBegin:
                    break;
                case TrafficSignType.TrafficCalmedDistrictEnd:
                    break;
                case TrafficSignType.EnvironmentalZoneBegin:
                    break;
                case TrafficSignType.EnvironmentalZoneEnd:
                    break;
                case TrafficSignType.OvertakingBanBegin:
                    break;
                case TrafficSignType.OvertakingBanEnd:
                    break;
                case TrafficSignType.OvertakingBanForTrucksBegin:
                    break;
                case TrafficSignType.OvertakingBanForTrucksEnd:
                    break;
                case TrafficSignType.RightOfWayBegin:
                    break;
                case TrafficSignType.RightOfWayEnd:
                    break;
                case TrafficSignType.DoNotEnter:
                    break;
                case TrafficSignType.HighWayBegin:
                    break;
                case TrafficSignType.HighWayEnd:
                    break;
                case TrafficSignType.HighWayExit:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override ElementOrigin ElementOrigin => ElementOrigin.OpenDrive;

        public override void OcclusionStart() {
            throw new NotImplementedException();
        }

        public override void OcclusionEnd() {
            throw new NotImplementedException();
        }

        public override void SetupOccludedMaterials() {
            throw new NotImplementedException();
        }
    }
}