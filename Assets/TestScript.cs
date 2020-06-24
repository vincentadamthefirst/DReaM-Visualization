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

    public NetworkHolder networkHolder;

    // Start is called before the first frame update
    void Start() {
        SceneryXmlHandler handler = new SceneryXmlHandler();
        handler.SetFilePath("C:\\OpenPass\\bin\\configs\\SceneryConfiguration.xodr");
        //handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Ackermann_Zellesch.xodr");
        //handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Budapester_Nossener.xodr");
        //handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Chemnitzer_Würzburger.xodr");
        handler.networkHolder = networkHolder;
        
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
