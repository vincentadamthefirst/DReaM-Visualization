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

    public Terrain terrain;

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
            case 6:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\Roundabout8Course.xodr");
                break;
            case 7:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\CrossingComplex8Course.xodr");
                break;
            case 8:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\whack.xodr");
                break;
            default:
                handler.SetFilePath("C:\\OpenPass\\bin\\configs\\SceneryConfiguration.xodr");
                break;
        }
        
        handler.roadNetworkHolder = roadNetworkHolder;
        
        handler.StartImport();
        
        roadNetworkHolder.ShowSimpleGround(terrain);
    }
}
