namespace Evaluation {

    
    public enum QualitativeEvaluationType {
        None = 0, CountingOccOff, CountingOccOn, LabelScene, LabelScreen, OccTransparency, OccWireFrame, OccShader
    }

    public enum QuantitativeEvaluationType {
        None = 0, Shader, RayCast, Polygon, Transparency, WireFrame, Nothing
    }
}