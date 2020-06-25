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

    // void FixedUpdate() {
    //     Debug.Log("FUDT - " + Time.fixedUnscaledDeltaTime);
    //     Debug.Log("FDT  - " + Time.fixedDeltaTime);
    // }
}
