using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using JetBrains.Annotations;
using Scenery.RoadNetwork;
using Scenery.RoadNetwork.RoadGeometries;
using UnityEngine;

namespace Importer.XMLHandlers {
    public class SceneryXmlHandler : XmlHandler {
        public NetworkHolder networkHolder;

        private int _roadIdCounter;

        public override string GetName() {
            return "SceneryXmlHandler";
        }

        public override void StartImport() {
            if (xmlDocument.Root == null) return; // TODO Error handling
            
            ImportRoads();

            networkHolder.CreateMeshes();
        }

        public override List<GameObject> GetInfoFields() {
            throw new System.NotImplementedException();
        }

        private void ImportRoads() {
            var roadElements = xmlDocument.Root?.Elements("road");

            if (roadElements != null)
                foreach (var road in roadElements) {
                    CreateRoad(road);
                }
            else {
                // TODO Error handling
            }
        }

        private void CreateRoad(XElement road) {
            var roadId = road.Attribute("id")?.Value ?? "-1";
            var roadName = road.Attribute("name")?.Value ?? "Road #" + _roadIdCounter;
            if (roadName == "" || roadName == " ") roadName = "Road #" + _roadIdCounter;
            _roadIdCounter++;

            
            var roadObject = networkHolder.CreateRoad(roadId);
            roadObject.name = roadName;
            roadObject.Length = float.Parse(road.Attribute("length")?.Value ??
                                            throw new ArgumentMissingException(
                                                "s-value for geometry of road " + roadObject.OpenDriveId +
                                                " missing."), CultureInfo.InvariantCulture.NumberFormat);
            roadObject.OnJunction = int.Parse(road.Attribute("junction")?.Value ?? "-1") != -1;
            roadObject.OpenDriveId = roadId + "";

            var successorId = road.Element("link")?.Element("successor")?.Attribute("elementId")?.Value ?? "-1";
            roadObject.SuccessorOdId = successorId;

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

                var laneSectionObject = networkHolder.CreateLaneSection(road);
                laneSectionObject.Parent = road;
                laneSectionObject.S = s;

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

            var laneObject = networkHolder.CreateLane(parentSection);
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

            laneObject.SuccessorId = lane.Element("link")?.Element("successor")?.Attribute("id")?.Value ?? "0";

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
    }
}