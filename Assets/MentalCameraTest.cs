using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Camera))]
public class MentalCameraTest : MonoBehaviour {

    private Camera _camera;
    
    public float cameraWidth = 50f;
    public float cameraHeight = 50f;

    private RenderTexture _renderTexture;

    public GameObject miniMap;

    public RawImage image;
    
    // Start is called before the first frame update
    void Start() {
        _camera = GetComponent<Camera>();
        _renderTexture = new RenderTexture(250, 250, 1);

        image.texture = _renderTexture;

        miniMap.GetComponent<MeshRenderer>().material.mainTexture = _renderTexture;

        _camera.aspect = 1f;
        _camera.orthographicSize = 25f;
        
        _camera.targetTexture = _renderTexture;
        //
        // var x = (100f - 100f / (Screen.width / cameraWidth)) / 100f;
        // var y = (100f - 100f / (Screen.height / cameraHeight)) / 100f;
        // GetComponent<Camera>().rect = new Rect(x, y, 1, 1);

        Debug.Log("Width: " + _camera.pixelWidth + " & Height: " + _camera.pixelHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
