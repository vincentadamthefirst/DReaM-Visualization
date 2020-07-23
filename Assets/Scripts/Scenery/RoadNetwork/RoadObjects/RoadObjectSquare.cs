using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Scenery.RoadNetwork.RoadObjects {
    public class RoadObjectSquare : RoadObject {
        public float Length { get; set; }
        
        public float Width { get; set; }

        private GameObject _model;

        private void Repeat() {
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
                newChild.Width = Width;
                newChild.Length = Length;
                newChild.Show();
            }

            markedForDelete = true;
        }

        public override void Show() {
            Repeat();
            if (markedForDelete) return;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (RoadObjectType) {
                case RoadObjectType.Building:
                    ShowBuilding();
                    break;
                case RoadObjectType.CrossWalk:
                case RoadObjectType.ParkingSpace:
                    var mat = RoadDesign.GetRoadObjectMaterial(RoadObjectType, SubType);
                    ShowPlane(mat.material);
                    break;
                case RoadObjectType.Tree:
                case RoadObjectType.StreetLamp:
                case RoadObjectType.Pole:
                case RoadObjectType.None: // these types are no supported square objects, destroy this object
                    markedForDelete = true;
                    break;
            }

            transform.parent = Parent.transform;
            
            Destroy(_model.GetComponent<Collider>());
            
            MaybeDelete();
        }

        private void ShowPlane(Material material) {
            _model = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _model.layer = 19;
            // divide by 10 since base plane has size 10x10
            _model.transform.SetGlobalScale(new Vector3(Length / 10f, Height, Width / 10f));
            
            var mat = new Material(material);
            var p = material.GetTextureScale(BaseMap);
            var v = new Vector2(Length * p.y, Width * p.x);
            mat.SetTextureScale(BumpMap, v);
            mat.SetTextureScale(BaseMap, v);
            mat.SetTextureScale(OcclusionMap, v);

            _model.GetComponent<MeshRenderer>().material = mat;
            _model.transform.parent = transform;
            
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            _model.transform.position = Parent.EvaluatePoint(S, m * T, ZOffset);
            
            var completeHdg = Mathf.PI - Parent.EvaluateHeading(S) + Heading;
            _model.transform.Rotate(Vector3.up, Mathf.Rad2Deg * completeHdg);
        }

        private void ShowBuilding() {
            _model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _model.layer = 19;
            
            _model.transform.SetGlobalScale(new Vector3(Length, Height, Width));
            _model.GetComponent<MeshRenderer>().material =
                RoadDesign.GetRoadObjectMaterial(RoadObjectType, SubType).material;
            _model.transform.parent = transform;
            
            var m = Orientation == RoadObjectOrientation.Negative ? -1 : 1;
            _model.transform.position = Parent.EvaluatePoint(S, m * T, ZOffset + Height / 2f);
            
            var completeHdg = Parent.EvaluateHeading(S) + Heading;
            _model.transform.Rotate(Vector3.up, Mathf.Rad2Deg * completeHdg);
        }
        
        public override bool MaybeDelete() {
            if (!markedForDelete) return false;
            Destroy(this);
            return true;
        }

        private void AddCollider() {
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            
            
        }

        public override void HandleHit() {
            Debug.Log("SQUARE OBJ " + name +  " IS HANDLING A HIT");
        }

        public override void HandleNonHit() {
            Debug.Log("SQUARE OBJ " + name +  " IS NO LONGER HANDLING A HIT");
        }
    }
}