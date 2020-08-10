namespace Evaluation {

    
    public enum EvaluationType {
        None = 0, CountingOccOff, CountingOccOn, LabelScene, LabelScreen, OccTransparency, OccWireFrame, OccShader
    }

    public enum FpsTest {
        None = 0, Shader, RayCast, Polygon, Transparency, WireFrame, Nothing
    }
}