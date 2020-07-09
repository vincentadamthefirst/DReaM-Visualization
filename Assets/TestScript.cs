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
using Visualization;

public class TestScript : MonoBehaviour {

    public RoadNetworkHolder roadNetworkHolder;

    public VisualizationMaster visualizationMaster;

    public Terrain terrain;

    public int debugTestFile = 0;
    
    void Start() {
        // IMPORT OF OPENDRIVE
        
        var sceneryImporter = new SceneryXmlHandler();

        switch (debugTestFile) {
            case 0:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\SceneryConfiguration.xodr");
                break;
            case 1:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\Ackermann_Zellesch.xodr");
                break;
            case 2:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\Budapester_Nossener.xodr");
                break;
            case 3:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\Chemnitzer_Würzburger.xodr");
                break;
            case 4:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\DeadEnd.xodr");
                break;
            case 5:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\Crossing8Course.xodr");
                break;
            case 6:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\Roundabout8Course.xodr");
                break;
            case 7:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\CrossingComplex8Course.xodr");
                break;
            case 8:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\Configset Juli 2020\\whack.xodr");
                break;
            default:
                sceneryImporter.SetFilePath("C:\\OpenPass\\bin\\configs\\SceneryConfiguration.xodr");
                break;
        }
        
        sceneryImporter.roadNetworkHolder = roadNetworkHolder;
        
        sceneryImporter.StartImport();
        
        roadNetworkHolder.ShowSimpleGround(terrain);
        
        // IMPORT OF OPENPASS (OpenSpaaaaaaaß)
        
        var outputImporter = new SimulationOutputXmlHandler();
        outputImporter.SetFilePath("C:\\OpenPass\\bin - Kopie\\results\\simulationOutput.xml");
        outputImporter.visualizationMaster = visualizationMaster;
        outputImporter.StartImport();

        // TODO implement parsing of ModelsCatalog
        
        visualizationMaster.PrepareAgents();
        
    }
}
