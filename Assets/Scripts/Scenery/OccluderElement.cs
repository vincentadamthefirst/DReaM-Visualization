namespace Scenery {
    
    public abstract class OccluderElement : VisualizationElement {
        
        public int TargetsOccluded { get; set; } = 0;
        
        public abstract void OcclusionStart();
        
        public abstract void OcclusionEnd();

        public abstract void SetupOccludedMaterials();
    }
}