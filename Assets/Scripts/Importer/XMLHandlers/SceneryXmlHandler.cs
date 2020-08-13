using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Linq;
using JetBrains.Annotations;
using Scenery.RoadNetwork;
using Scenery.RoadNetwork.RoadGeometries;
using Scenery.RoadNetwork.RoadObjects;
using UnityEngine;
using ContactPoint = Scenery.RoadNetwork.ContactPoint;
using Version = Utils.VersionSystem.Version;

namespace Importer.XMLHandlers {
    [SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
    public class SceneryXmlHandler : XmlHandler {
        public RoadNetworkHolder roadNetworkHolder;

        private int _roadIdCounter;

        public override string GetName() {
            return "Scenery";
        }

        public virtual void StartImport() {
            if (xmlDocument.Root == null) return; // TODO Error handling
            
            ImportRoads();

            roadNetworkHolder.CreateMeshes();
        }

        public override string GetDetails() {
            if (xmlDocument.Root?.Element("header") == null) return "<color=\"red\"><b>XML Error</b>";

            var supported = new Version("1.4");

            var header = xmlDocument.Root.Element("header");
            var revMajor = header?.Attribute("revMajor")?.Value;
            var revMinor = header?.Attribute("revMinor")?.Value;
            var versionString = "<color=\"orange\">Version unknown";

            if (revMajor != null && revMinor != null) {
                var fileVersion = new Version(revMajor + "." + revMinor);
                versionString = fileVersion.CompareTo(supported) >= 0
                    ? "<color=\"green\">Version " + revMajor + "." + revMinor
                    : "<color=\"red\">Version " + revMajor + "." + revMinor;
            }

            return versionString + "<color=\"white\"> <b>|</b> " + "Date: " + (header?.Attribute("date")?.Value ?? "") +
                   " <b>|</b> " + "Name: " +  (header?.Attribute("name")?.Value ?? "");
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
            junctionName += " [" + junctionId + "]";

            var junctionObject = roadNetworkHolder.CreateJunction(junctionId);
            junctionObject.OpenDriveId = junctionId;
            junctionObject.name = junctionName;

            foreach (var connection in junction.Elements("connection")) {
                var incomingRoadAttribute = connection.Attribute("incomingRoad");
                var connectingRoadAttribute = connection.Attribute("connectingRoad");
                var contactPoint = (connection.Attribute("contactPoint")?.Value ?? "start") == "start"
                    ? ContactPoint.Start
                    : ContactPoint.End;

                if (incomingRoadAttribute == null || connectingRoadAttribute == null) continue;
                var laneLinks = new List<LaneLink>();
                foreach (var laneLink in connection.Elements("laneLink")) {
                    var fromAttribute = laneLink.Attribute("from");
                    var toAttribute = laneLink.Attribute("to");

                    if (fromAttribute != null && toAttribute != null) {
                        laneLinks.Add(new LaneLink() {From = fromAttribute.Value, To = toAttribute.Value});
                    }
                }

                if (laneLinks.Count == 0) continue;
                var newConnection = new Connection {
                    IncomingRoadOdId = incomingRoadAttribute.Value,
                    ConnectingRoadOdId = connectingRoadAttribute.Value,
                    ContactPoint = contactPoint,
                    LaneLinks = laneLinks
                };
                junctionObject.Connections.Add(newConnection);
            }
        }

        private void CreateRoad(XElement road) {
            var roadId = road.Attribute("id")?.Value ?? "-1";
            var roadName = road.Attribute("name")?.Value ?? "Road #" + _roadIdCounter;
            if (roadName == "" || roadName == " ") roadName = "Road #" + _roadIdCounter;
            _roadIdCounter++;

            var junctionId = road.Attribute("junction")?.Value ?? "-1";

            // creating road and setting base parameters
            var roadObject = roadNetworkHolder.CreateRoad(roadId, junctionId);
            roadObject.name = roadName + " [" + roadId + "]";
            roadObject.OpenDriveId = roadId.Replace(" ", "") + "";
            roadObject.Length = float.Parse(road.Attribute("length")?.Value ??
                                            throw new ArgumentMissingException(
                                                "s-value for geometry of road " + roadObject.OpenDriveId +
                                                " missing."), CultureInfo.InvariantCulture.NumberFormat);
            roadObject.OnJunction = int.Parse(road.Attribute("junction")?.Value ?? "-1") != -1;

            // finding successor id
            var successorId = road.Element("link")?.Element("successor")?.Attribute("elementId")?.Value ?? "x";
            roadObject.SuccessorOdId = successorId;

            // finding successor type
            var successorType = road.Element("link")?.Element("successor")?.Attribute("elementType")?.Value ?? "road";
            switch (successorType.ToLower()) {
                case "junction":
                    roadObject.SuccessorElementType = ElementType.Junction;
                    break;
                case "road":
                    roadObject.SuccessorElementType = ElementType.Road;
                    break;
                default:
                    roadObject.SuccessorElementType = ElementType.None;
                    break;
            }

            // finding contact point
            var contactPoint = road.Element("link")?.Element("successor")?.Attribute("contactPoint")?.Value ?? "start";
            roadObject.SuccessorContactPoint = contactPoint.ToLower() == "end" ? ContactPoint.End : ContactPoint.Start;
            
            // creating objects along the road
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

            switch (type.ToLower()) {
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

            switch (type.ToLower()) {
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
                TStart = float.Parse(repeat.Attribute("tStart")?.Value ?? roadObject.T + "",
                    CultureInfo.InvariantCulture.NumberFormat),
                TEnd = float.Parse(repeat.Attribute("tEnd")?.Value ?? roadObject.T + "",
                    CultureInfo.InvariantCulture.NumberFormat),
                HeightStart = float.Parse(repeat.Attribute("heightStart")?.Value ?? roadObject.Height + "",
                    CultureInfo.InvariantCulture.NumberFormat),
                HeightEnd = float.Parse(repeat.Attribute("heightEnd")?.Value ?? roadObject.Height + "",
                    CultureInfo.InvariantCulture.NumberFormat),
                ZOffsetStart = float.Parse(repeat.Attribute("zOffsetEnd")?.Value ?? roadObject.ZOffset + "",
                    CultureInfo.InvariantCulture.NumberFormat),
                ZOffsetEnd = float.Parse(repeat.Attribute("zOffsetStart")?.Value ?? roadObject.ZOffset + "",
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
                        spiralElement.Attribute("curvEnd")?.Value ?? throw new ArgumentMissingException(
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
                            pp3Element.Attribute("aU")?.Value ??
                            throw new ArgumentMissingException(
                                "aU-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element.Attribute("bV")?.Value ??
                            throw new ArgumentMissingException(
                                "bV-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element.Attribute("bU")?.Value ??
                            throw new ArgumentMissingException(
                                "bU-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element.Attribute("cV")?.Value ??
                            throw new ArgumentMissingException(
                                "cV-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element.Attribute("cU")?.Value ??
                            throw new ArgumentMissingException(
                                "cU-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element.Attribute("dV")?.Value ??
                            throw new ArgumentMissingException(
                                "dV-value for geometry of road " + roadObject.OpenDriveId + " missing."),
                            CultureInfo.InvariantCulture.NumberFormat), float.Parse(
                            pp3Element.Attribute("dU")?.Value ??
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
            var id = lane.Attribute("id")?.Value ?? "x";
            
            if (id == "x")
                throw new ArgumentMissingException("A lane of road " + parentSection.Parent.OpenDriveId +
                                                   " has no id!");

            if (!int.TryParse(id, out var idInt))
                throw new ArgumentMissingException("The lane with id " + id + " has no integer id!");

            LaneType laneType;

            switch ((lane.Attribute("type")?.Value ?? "none").ToLower()) {
                case "driving":
                    laneType = LaneType.Driving;
                    break;
                case "border":
                    laneType = LaneType.Border;
                    break;
                case "sidewalk":
                    laneType = LaneType.Sidewalk;
                    break;
                case "biking":
                    laneType = LaneType.Biking;
                    break;
                case "restricted":
                    laneType = LaneType.Restricted;
                    break;
                case "none":
                    laneType = LaneType.None;
                    break;
                case "shoulder":
                    laneType = LaneType.Shoulder;
                    break;
                default:
                    laneType = LaneType.None;
                    break;
            }
            
            var laneObject = roadNetworkHolder.CreateLane(parentSection);
            laneObject.LaneId = id;
            laneObject.LaneIdInt = idInt;
            laneObject.OpenDriveId = id + "";
            laneObject.Parent = parentSection;
            laneObject.LaneDirection = laneDirection;
            laneObject.LaneType = laneType;

            laneObject.name = Enum.GetName(typeof(LaneDirection), laneDirection) + " Lane [" +
                              Enum.GetName(typeof(LaneType), laneType) + "]";

            laneObject.SuccessorId = lane.Element("link")?.Element("successor")?.Attribute("id")?.Value ?? "x";

            laneObject.InnerHeight = float.Parse(lane.Element("height")?.Attribute("inner")?.Value ?? "0",
                CultureInfo.InvariantCulture.NumberFormat);
            laneObject.OuterHeight = float.Parse(lane.Element("height")?.Attribute("outer")?.Value ?? "0",
                CultureInfo.InvariantCulture.NumberFormat);
            
            CreateRoadMark(lane.Element("roadMark"), laneObject);

            switch (laneDirection) {
                case LaneDirection.Center:
                    parentSection.SetCenterLane(laneObject);
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

            var widths = lane.Elements("width");
            foreach (var width in widths) {
                ParseLaneWidth(width, laneObject);
            }
        }

        private static void ParseLaneWidth(XElement width, Lane laneObject) {
            laneObject.AddWidthEntry(
                float.Parse(width.Attribute("sOffset")?.Value ?? "0", CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("a")?.Value ??
                            throw new ArgumentMissingException("a-value missing for width of lane of road " + 
                                                               laneObject.Parent.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("b")?.Value ??
                            throw new ArgumentMissingException("b-value missing for width of lane of road " + 
                                                               laneObject.Parent.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("c")?.Value ??
                            throw new ArgumentMissingException("c-value missing for width of lane of road " + 
                                                               laneObject.Parent.Parent.OpenDriveId),
                    CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(width.Attribute("d")?.Value ??
                            throw new ArgumentMissingException("d-value missing for width of lane of road " + 
                                                               laneObject.Parent.Parent.OpenDriveId),
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