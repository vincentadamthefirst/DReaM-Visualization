using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using JetBrains.Annotations;
using Packages.Rider.Editor;
using Scenery.RoadNetwork;
using Scenery.RoadNetwork.RoadGeometries;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;
using ContactPoint = Scenery.RoadNetwork.ContactPoint;

namespace Importer.XMLHandlers {
    [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
    public class SceneryXmlHandler : XmlHandler {
        public RoadNetworkHolder roadNetworkHolder;

        private int _roadIdCounter;

        public override string GetName() {
            return "SceneryXmlHandler";
        }

        public override void StartImport() {
            if (xmlDocument.Root == null) return; // TODO Error handling
            
            ImportRoads();

            roadNetworkHolder.CreateMeshes();
        }

        public override List<GameObject> GetInfoFields() {
            throw new System.NotImplementedException();
        }

        private void ImportRoads() {
            var roadElements = xmlDocument.Root?.Elements("road");
            var junctionElements = xmlDocument.Root?.Elements("junction");

            if (junctionElements != null) {
                foreach (var junction in junctionElements) {
                    CreateJunction(junction);
                }
            } else {
                // TODO error handling
            }

            if (roadElements != null)
                foreach (var road in roadElements) {
                    CreateRoad(road);
                }
            else {
                // TODO Error handling
            }
        }

        private void CreateJunction(XElement junction) {
            var junctionId = junction.Attribute("id")?.Value ??
                             throw new ArgumentMissingException("Junction has no id!");

            var junctionName = junction.Attribute("name")?.Value ?? "junction";

            var junctionObject = roadNetworkHolder.CreateJunction(junctionId);
            junctionObject.OpenDriveId = junctionId;
            junctionObject.name = junctionName;
        }

        private void CreateRoad(XElement road) {
            var roadId = road.Attribute("id")?.Value ?? "-1";
            var roadName = road.Attribute("name")?.Value ?? "Road #" + _roadIdCounter;
            if (roadName == "" || roadName == " ") roadName = "Road #" + _roadIdCounter;
            _roadIdCounter++;

            var junctionId = road.Attribute("junction")?.Value ?? "-1";

            var roadObject = roadNetworkHolder.CreateRoad(roadId, junctionId);
            roadObject.name = roadName;
            roadObject.Length = float.Parse(road.Attribute("length")?.Value ??
                                            throw new ArgumentMissingException(
                                                "s-value for geometry of road " + roadObject.OpenDriveId +
                                                " missing."), CultureInfo.InvariantCulture.NumberFormat);
            roadObject.OnJunction = int.Parse(road.Attribute("junction")?.Value ?? "-1") != -1;
            roadObject.OpenDriveId = roadId + "";

            var successorId = road.Element("link")?.Element("successor")?.Attribute("elementId")?.Value ?? "x";
            roadObject.SuccessorOdId = successorId;

            var contactPoint = road.Element("link")?.Element("successor")?.Attribute("contactPoint")?.Value ?? "start";
            roadObject.SuccessorContactPoint = contactPoint.ToLower() == "end" ? ContactPoint.End : ContactPoint.Start;
            
            CreateRoadObjects(road.Element("objects")?.Elements("object"), roadObject);

            try {
                ParseGeometries(road, roadObject);
            } catch (ArgumentUnknownException) {
                if (roadObject.RoadGeometries.Count == 0) {
                    throw new ArgumentMissingException("No supported geometry values given for road " +
                                                       roadObject.OpenDriveId);
                }
            }

            CreateLaneSections(
                road.Element("lanes") ??
                throw new ArgumentMissingException("No lanes given for road " + roadObject.OpenDriveId), roadObject);
        }

        private void CreateRoadObjects(IEnumerable<XElement> roadObjects, Road road) {
            if (roadObjects == null) return;
            foreach (var roadObject in roadObjects) {
                var type = roadObject.Attribute("type")?.Value ?? "None";
                if (type == "None") continue;
                switch (type.ToLower()) {
                    case "streetlamp":
                    case "tree":
                    case "pole":
                        CreateRoundRoadObject(roadObject, road, type.ToLower());
                        break;
                    case "building":
                        CreateBuildingRoadObject(roadObject, road, type);
                        break;
                    case "crosswalk":
                    case "parkingspace":
                        CreateSquareRoadObject(roadObject, road, type);
                        break;
                }
            }
        }

        private void CreateBuildingRoadObject(XElement obj, Road road, string type) {
            var rad = obj.Attribute("radius");
            var len = obj.Attribute("length");
            var wid = obj.Attribute("width");
            if (rad != null) {
                CreateRoundRoadObject(obj, road, type);
            } else if (len != null && wid != null) {
                CreateSquareRoadObject(obj, road, type);
            }
        }

        private void CreateRoundRoadObject(XElement obj, Road road, string type) {
            var newRoadObj = roadNetworkHolder.CreateRoadObjectRound(road);
            GetBasicRoadObjectInfo(newRoadObj, obj);
            newRoadObj.Radius = float.Parse(obj.Attribute("radius")?.Value ?? "1",
                CultureInfo.InvariantCulture.NumberFormat);

            switch (type) {
                case "streetlamp":
                    newRoadObj.RoadObjectType = RoadObjectType.StreetLamp;
                    break;
                case "pole":
                    newRoadObj.RoadObjectType = RoadObjectType.Pole;
                    break;
                case "tree":
                    newRoadObj.RoadObjectType = RoadObjectType.Tree;
                    break;
                case "building":
                    newRoadObj.RoadObjectType = RoadObjectType.Building;
                    break;
                default:
                    newRoadObj.RoadObjectType = RoadObjectType.None;
                    break;
            }
        }
        
        private void CreateSquareRoadObject(XElement obj, Road road, string type) {
            var newRoadObj = roadNetworkHolder.CreateRoadObjectSquare(road);
            GetBasicRoadObjectInfo(newRoadObj, obj);
            newRoadObj.Width = float.Parse(obj.Attribute("width")?.Value ?? "1",
                CultureInfo.InvariantCulture.NumberFormat);
            newRoadObj.Length = float.Parse(obj.Attribute("length")?.Value ?? "1",
                CultureInfo.InvariantCulture.NumberFormat);

            switch (type) {
                case "building":
                    newRoadObj.RoadObjectType = RoadObjectType.Building;
                    break;
                case "crosswalk":
                    newRoadObj.RoadObjectType = RoadObjectType.CrossWalk;
                    break;
                case "parkingspace":
                    newRoadObj.RoadObjectType = RoadObjectType.ParkingSpace;
                    break;
                default:
                    newRoadObj.RoadObjectType = RoadObjectType.None;
                    break;
            }
        }

        private void GetBasicRoadObjectInfo(RoadObject roadObject, XElement obj) {
            var orientation = obj.Attribute("orientation")?.Value ?? "none";
            roadObject.name = obj.Attribute("name")?.Value ?? "roadObject";
            roadObject.S = float.Parse(obj.Attribute("s")?.Value ?? "0",
                CultureInfo.InvariantCulture.NumberFormat);
            roadObject.T = float.Parse(obj.Attribute("t")?.Value ?? "0",
                CultureInfo.InvariantCulture.NumberFormat);
            roadObject.ZOffset = float.Parse(obj.Attribute("zOffset")?.Value ?? "0",
                CultureInfo.InvariantCulture.NumberFormat);
            roadObject.Heading = float.Parse(obj.Attribute("hdg")?.Value ?? "0",
                CultureInfo.InvariantCulture.NumberFormat);
            roadObject.Height = float.Parse(obj.Attribute("height")?.Value ?? "1",
                CultureInfo.InvariantCulture.NumberFormat);
            roadObject.SubType = obj.Attribute("subtype")?.Value ?? "random";
            
            switch (orientation) {
                case "+":
                    roadObject.Orientation = RoadObjectOrientation.Positive;
                    break;
                case "-":
                    roadObject.Orientation = RoadObjectOrientation.Negative;
                    break;
                default:
                    roadObject.Orientation = RoadObjectOrientation.None;
                    break;
            }
            
            GetRoadObjectRepeatInfo(roadObject, obj);
        }

        private static void GetRoadObjectRepeatInfo(RoadObject roadObject, XElement obj) {
            var repeat = obj.Element("repeat");
            if (repeat == null) return;

            roadObject.RepeatParameters = new RepeatParameters {
                SStart = float.Parse(repeat.Attribute("s")?.Value ?? "0",
                    CultureInfo.InvariantCulture.NumberFormat),
                Length = float.Parse(repeat.Attribute("length")?.Value ?? "1",
                    CultureInfo.InvariantCulture.NumberFormat),
                Distance = float.Parse(repeat.Attribute("distance")?.Value ?? "1",
                    CultureInfo.InvariantCulture.NumberFormat),
                TStart = float.Parse(repeat.Attribute("tStart")?.Value ?? "0",
                    CultureInfo.InvariantCulture.NumberFormat),
                TEnd = float.Parse(repeat.Attribute("tEnd")?.Value ?? "0",
                    CultureInfo.InvariantCulture.NumberFormat),
                HeightStart = float.Parse(repeat.Attribute("heightStart")?.Value ?? "1",
                    CultureInfo.InvariantCulture.NumberFormat),
                HeightEnd = float.Parse(repeat.Attribute("heightEnd")?.Value ?? "1",
                    CultureInfo.InvariantCulture.NumberFormat),
                ZOffsetStart = float.Parse(repeat.Attribute("zOffsetEnd")?.Value ?? "0",
                    CultureInfo.InvariantCulture.NumberFormat),
                ZOffsetEnd = float.Parse(repeat.Attribute("zOffsetStart")?.Value ?? "0",
                    CultureInfo.InvariantCulture.NumberFormat)
            };
        }

        private static void ParseGeometries(XContainer road, Road roadObject) {
            var geometries = road.Element("planView")?.Elements("geometry");

            if (geometries == null)
                throw new ArgumentMissingException("No geometry given for road " + roadObject.OpenDriveId);
            
            foreach (var geometry in geometries) {
                var s = float.Parse(
                    geometry.Attribute("s")?.Value ??
                    throw new ArgumentMissingException("s-value for geometry of road " + roadObject.OpenDriveId +
                                                       " missing."), CultureInfo.InvariantCulture.NumberFormat);
                var x = float.Parse(
                    geometry.Attribute("x")?.Value ??
                    throw new ArgumentMissingException("x-value for geometry of road " + roadObject.OpenDriveId +
                                                       " missing."), CultureInfo.InvariantCulture.NumberFormat);
                var y = float.Parse(
                    geometry.Attribute("y")?.Value ??
                    throw new ArgumentMissingException("y-value for geometry of road " + roadObject.OpenDriveId +
                                                       " missing."), CultureInfo.InvariantCulture.NumberFormat);
                var hdg = float.Parse(
                    geometry.Attribute("hdg")?.Value ??
                    throw new ArgumentMissingException("hdg-value for geometry of road " + roadObject.OpenDriveId +
                                                       " missing."), CultureInfo.InvariantCulture.NumberFormat);
                var length =
                    float.Parse(
                        geometry.Attribute("length")?.Value ??
                        throw new ArgumentMissingException("length-value for geometry of road " +
                                                           roadObject.OpenDriveId + " missing."),
                        CultureInfo.InvariantCulture.NumberFormat);

                if (geometry.Element("line") != null) {
                    roadObject.AddRoadGeometry(new LineGeometry(s, x, y, hdg, length));
                } else if (geometry.Element("arc") != null) {
                    var arcElement = geometry.Element("arc");
                    roadObject.AddRoadGeometry(new ArcGeometry(s, x, y, hdg, length, float.Parse(
                        arcElement?.Attribute("curvature")?.Value ?? throw new ArgumentMissingException(
                            "curvature-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                        CultureInfo.InvariantCulture.NumberFormat)));
                } else if (geometry.Element("spiral") != null) {
                    var spiralElement = geometry.Element("spiral");
                    roadObject.AddRoadGeometry(new SpiralGeometry(s, x, y, hdg, length, float.Parse(
                        spiralElement?.Attribute("curvStart")?.Value ?? throw new ArgumentMissingException(
                            "curvature-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                        CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                        spiralElement?.Attribute("curvEnd")?.Value ?? throw new ArgumentMissingException(
                            "curvature-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                        CultureInfo.InvariantCulture.NumberFormat)));
                } else if (geometry.Element("poly3") != null) {
                    // TODO implement
                } else if (geometry.Element("paramPoly3") != null) {
                    var pp3Element = geometry.Element("paramPoly3");
                    roadObject.AddRoadGeometry(new ParamPoly3Geometry(s, x, y, hdg, length, float.Parse(
                            pp3Element?.Attribute("aV")?.Value ??
                            throw new ArgumentMissingException(
                                "aV-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element?.Attribute("aU")?.Value ??
                            throw new ArgumentMissingException(
                                "aU-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element?.Attribute("bV")?.Value ??
                            throw new ArgumentMissingException(
                                "bV-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element?.Attribute("bU")?.Value ??
                            throw new ArgumentMissingException(
                                "bU-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element?.Attribute("cV")?.Value ??
                            throw new ArgumentMissingException(
                                "cV-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element?.Attribute("cU")?.Value ??
                            throw new ArgumentMissingException(
                                "cU-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element?.Attribute("dV")?.Value ??
                            throw new ArgumentMissingException(
                                "dV-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element?.Attribute("dU")?.Value ??
                            throw new ArgumentMissingException(
                                "dU-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat)
                    ));
                } else {
                    throw new ArgumentUnknownException("<geometry>-tag for road " + roadObject.OpenDriveId +
                                                       " does not contain a valid geometry Element.");
                }
            }
        }

        private void CreateLaneSections([NotNull] XContainer laneSectionsParent, Road road) {
            foreach (var laneSection in laneSectionsParent.Elements("laneSection")) {
                var s = float.Parse(
                    laneSection.Attribute("s")?.Value ??
                    throw new ArgumentMissingException("s-value for lane section of road " + road.OpenDriveId +
                                                       " missing."), CultureInfo.InvariantCulture.NumberFormat);

                var laneSectionObject = roadNetworkHolder.CreateLaneSection(road);
                laneSectionObject.Parent = road;
                laneSectionObject.S = s;
                laneSectionObject.name = "LaneSection";

                var centerLane = laneSection.Element("center")?.Element("lane");
                if (centerLane != null) CreateLane(centerLane, laneSectionObject, LaneDirection.Center);

                var leftLanes = laneSection.Element("left")?.Elements("lane");
                var rightLanes = laneSection.Element("right")?.Elements("lane");

                if (leftLanes != null)
                    foreach (var lane in leftLanes) {
                        CreateLane(lane, laneSectionObject, LaneDirection.Left);
                    }

                if (rightLanes != null)
                    foreach (var lane in rightLanes) {
                        CreateLane(lane, laneSectionObject, LaneDirection.Right);
                    }

                road.AddLaneSection(laneSectionObject);
            }
        }

        private void CreateLane([NotNull] XElement lane, LaneSection parentSection, LaneDirection laneDirection) {
            if (!int.TryParse(lane.Attribute("id")?.Value, out var id))
                throw new ArgumentMissingException("A lane of road " + parentSection.Parent.OpenDriveId +
                                                   " has no id!");

            var laneObject = roadNetworkHolder.CreateLane(parentSection);
            laneObject.LaneId = id;
            laneObject.OpenDriveId = id + "";
            laneObject.Parent = parentSection;
            laneObject.LaneDirection = laneDirection;

            switch (lane.Attribute("type")?.Value ?? "none") {
                case "driving":
                    laneObject.LaneType = LaneType.Driving; 
                    break;
                case "sidewalk":
                    laneObject.LaneType = LaneType.Sidewalk;
                    break;
                case "none":
                    laneObject.LaneType = LaneType.None;
                    break;
            }

            laneObject.name = RoadEnumStrings.laneDirectionToString[(int) laneDirection] + " Lane [" +
                              RoadEnumStrings.laneTypeToString[(int) laneObject.LaneType] + "]";

            laneObject.SuccessorId = lane.Element("link")?.Element("successor")?.Attribute("id")?.Value ?? "x";
            
            CreateRoadMark(lane.Element("roadMark"), laneObject);

            switch (laneDirection) {
                case LaneDirection.Center:
                    parentSection.CenterLane = laneObject;
                    break;
                case LaneDirection.Left:
                    parentSection.AddLeftLane(laneObject);
                    break;
                case LaneDirection.Right:
                    parentSection.AddRightLane(laneObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(laneDirection), laneDirection, null);
            }

            if (laneDirection == LaneDirection.Center) return;

            var width = lane.Element("width");
            if (width == null) {
                throw new ArgumentMissingException("width missing for lane of road " + parentSection.Parent.OpenDriveId);
            }

            laneObject.SetWidthParameters(
                // ReSharper disable once PossibleNullReferenceException
                float.Parse(width.Attribute("sOffset")?.Value ??
                            throw new ArgumentMissingException(
                                "sOffset-value missing for width of lane of road " + parentSection.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("a")?.Value ??
                            throw new ArgumentMissingException(
                                "a-value missing for width of lane of road " + parentSection.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("b")?.Value ??
                            throw new ArgumentMissingException(
                                "b-value missing for width of lane of road " + parentSection.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("c")?.Value ??
                            throw new ArgumentMissingException(
                                "c-value missing for width of lane of road " + parentSection.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("d")?.Value ??
                            throw new ArgumentMissingException(
                                "d-value missing for width of lane of road " + parentSection.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat)
            );
        }

        private void CreateRoadMark(XElement roadMark, Lane parent) {
            var color = roadMark?.Attribute("color")?.Value ?? "standard";
            var type = roadMark?.Attribute("type")?.Value ?? "none";
            var width = roadMark?.Attribute("width")?.Value ?? "0.25";
            var widthFloat = float.Parse(width, CultureInfo.InvariantCulture.NumberFormat);

            RoadMarkType t;
            switch (type) {
                case "broken":
                    t = RoadMarkType.Broken;
                    break;
                case "solid":
                    t = RoadMarkType.Solid;
                    break;
                case "solid solid":
                    t = RoadMarkType.SolidSolid;
                    break;
                case "solid broken":
                    t = RoadMarkType.SolidBroken;
                    break;
                case "broken solid":
                    t = RoadMarkType.BrokenSolid;
                    break;
                case "broken broken":
                    t = RoadMarkType.BrokenBroken;
                    break;
                default:
                    t = RoadMarkType.None;
                    break;
            }

            Color c;
            switch (color) {
                case "white":
                case "standard":
                    c = new Color(1f, 1f, 1f, 0.1f);
                    break;
                case "blue":
                    c = new Color(0, 0, 153);
                    break;
                case "green":
                    c = new Color(0, 153, 51);
                    break;
                case "red":
                    c = new Color(204, 0, 0);
                    break;
                case "yellow":
                    c = new Color(230, 230, 0);
                    break;
                case "orange":
                    c = new Color(255, 153, 0);
                    break;
                default:
                    c = Color.white;
                    break;
            }

            var roadMarkObject = roadNetworkHolder.CreateRoadMark(parent);
            roadMarkObject.Width = widthFloat;
            roadMarkObject.RoadMarkType = t;
            roadMarkObject.RoadMarkColor = c;
            roadMarkObject.name = "RoadMark";
        }
    }
}