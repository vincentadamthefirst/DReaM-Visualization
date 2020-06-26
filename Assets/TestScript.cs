using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using Importer;
using Importer.XMLHandlers;
using Scenery.RoadNetwork;
using Scenery.RoadNetwork.RoadGeometries;
using UnityEngine;
using Utils.VersionSystem;

public class TestScript : MonoBehaviour {

    public RoadNetworkHolder roadNetworkHolder;

    public int debugTestFile = 0;

    // Start is called before the first frame update
    void Start() {
        SceneryXmlHandler handler = new SceneryXmlHandler();

        switch (debugTestFile) {
            case 0:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\SceneryConfiguration.xodr");
                break;
            case 1:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Ackermann_Zellesch.xodr");
                break;
            case 2:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Budapester_Nossener.xodr");
                break;
            case 3:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Chemnitzer_Würzburger.xodr");
                break;
            case 4:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\DeadEnd.xodr");
                break;
            case 5:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Crossing8Course.xodr");
                break;
            default:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\SceneryConfiguration.xodr");
                break;
        }
        
        handler.roadNetworkHolder = roadNetworkHolder;
        
        handler.StartImport();

        var pp3 = new ParamPoly3Geometry(0,20.8223882805909f, 23.8280193652608f, 1.21674411102422f, 30.0002638524053f, 0, 0, 0, 1, -0.184118986238162f, 86.9999999970155f, 0.184220104515203f, -57.9999999980103f);
        ps.Add(pp3.Evaluate(0,0));
        ps.Add(pp3.Evaluate(0, 2));
        ps.Add(pp3.Evaluate(0, -2));
        ps.Add(pp3.Evaluate(30.0002638524053f,0));
        ps.Add(pp3.Evaluate(30.0002638524053f, 2));
        ps.Add(pp3.Evaluate(30.0002638524053f,-2));

        // foreach (var p in ps) {
        //     Debug.Log(p);
        // }
        //
        
        // Vector2 tmp = new Vector2(2, 3);
        // Debug.Log(2 * tmp);
        //
        // var fi = new FolderImporter();
        //
        // var pf = fi.GetPossibleFiles("C:\\OpenPass\\bin");
        // foreach (var file in pf) {
        //     Debug.Log(file.Item1 + " - " + file.Item2.GetFilePath());
        // }
        //
        // Time.fixedDeltaTime = 0.01f;
    }
    
    List<Vector3> ps = new List<Vector3>();

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (var p in ps) {
            Gizmos.DrawSphere(p, 0.5f);
        }
    }

    // void FixedUpdate() {
    //     Debug.Log("FUDT - " + Time.fixedUnscaledDeltaTime);
    //     Debug.Log("FDT  - " + Time.fixedDeltaTime);
    // }
}
