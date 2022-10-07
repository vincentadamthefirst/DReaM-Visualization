using UnityEngine;
using Utils;
using Visualization.Agents;

/// <summary>
/// Class to be used on the main camera in the scene. Handles Movement, is based on the Unity Standard Movement-System.
/// Controls:
///     - W A S D     : Movement
///     - Q / E       : Up / Down
///     - Shift       : Speed up
///     - Scroll      : Change movement speed
///     - Right click : Hold to move the camera (locks cursor)
/// </summary>
public class SimpleCameraController : MonoBehaviour {
    public class CameraState {
        public float yaw;
        public float pitch;
        private float _roll;
        private float _x;
        private float _y;
        private float _z;

        public void SetFromTransform(Transform t) {
            var eulerAngles = t.eulerAngles;
            pitch = eulerAngles.x;
            yaw = eulerAngles.y;
            _roll = eulerAngles.z;
            var position = t.position;
            _x = position.x;
            _y = position.y;
            _z = position.z;
        }

        public void Translate(Vector3 translation) {
            var rotatedTranslation = Quaternion.Euler(pitch, yaw, _roll) * translation;

            _x += rotatedTranslation.x;
            _y += rotatedTranslation.y;
            _z += rotatedTranslation.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct) {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            _roll = Mathf.Lerp(_roll, target._roll, rotationLerpPct);

            _x = Mathf.Lerp(_x, target._x, positionLerpPct);
            _y = Mathf.Lerp(_y, target._y, positionLerpPct);
            _z = Mathf.Lerp(_z, target._z, positionLerpPct);
        }

        public void UpdateTransform(Transform t) {
            t.eulerAngles = new Vector3(pitch, yaw, _roll);
            t.position = new Vector3(_x, _y, _z);
        }
    }

    public CameraState targetCameraState = new CameraState();
    public CameraState interpolatingCameraState = new CameraState();

    [Header("Movement Settings")] [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
    public float boost = 3.5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    public AnimationCurve mouseSensitivityCurve =
        new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
    public bool invertY;

    /// <summary>
    /// Set if the Quantitative Evaluation is performed and the camera is moved throughout the scene.
    /// </summary>
    public bool AutomaticMovement { get; set; }
    
    /// <summary>
    /// The current Agent to follow with the camera.
    /// </summary>
    public Agent LockedOnAgent { get; set; }
    
    /// <summary>
    /// If a new Agent was selected as the locked on target.
    /// </summary>
    public bool LockedOnAgentIsSet { get; set; }

    /// <summary>
    /// The total distance travelled in units (m).
    /// </summary>
    public float TravelledDistance { get; private set;  }
    
    /// <summary>
    /// The Total rotation on all axis in degrees.
    /// </summary>
    public float TotalRotation { get; private set; }
    
    // information for the statistics
    private Vector3 _lastPosition;
    private Vector3 _lastRotation;
    
    // if the settings panel is currently open
    private bool _settingsOpen;

    /// <summary>
    /// Sets information on the state of the settings panel.
    /// </summary>
    /// <param name="value">If the settings panel has been opened</param>
    public void SetMenuOpen(bool value) {
        _settingsOpen = value;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnEnable() {
        targetCameraState.SetFromTransform(transform);
        interpolatingCameraState.SetFromTransform(transform);

        _lastPosition = transform.position;
    }

    /// <summary>
    /// Returns a Vector3 representing the current button input.
    /// </summary>
    /// <returns>A vector containing the x, y and z movement based on W A S D, E & Q</returns>
    private static Vector3 GetInputTranslationDirection() {
        var direction = new Vector3();
        if (Input.GetKey(KeyCode.W)) {
            direction += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S)) {
            direction += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A)) {
            direction += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D)) {
            direction += Vector3.right;
        }

        if (Input.GetKey(KeyCode.Q)) {
            direction += Vector3.down;
        }

        if (Input.GetKey(KeyCode.E)) {
            direction += Vector3.up;
        }

        return direction;
    }

    private void Update() {
        if (_settingsOpen || AutomaticMovement) return;

        // Hide and lock cursor when right mouse button pressed
        if (Input.GetMouseButtonDown(1)) {
            Cursor.lockState = CursorLockMode.Locked;
            LockedOnAgentIsSet = false;
        }

        // Unlock and show cursor when right mouse button released
        if (Input.GetMouseButtonUp(1)) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // The camera should follow an agent
        if (LockedOnAgentIsSet) {
            var offsetVector2 = new Vector2(20, 0);
            offsetVector2.RotateRadians(LockedOnAgent.CurrentRotation + Mathf.PI);

            var modelPosition = LockedOnAgent.Model.transform.position;
            transform.position = new Vector3(offsetVector2.x, 30, offsetVector2.y) +
                                 modelPosition;
            
            var secondOffsetVector = new Vector2(.5f, 0);
            secondOffsetVector.RotateRadians(LockedOnAgent.CurrentRotation + Mathf.PI - Mathf.PI / 2f);
            
            transform.LookAt(modelPosition + new Vector3(secondOffsetVector.x, 0, secondOffsetVector.y));
            return;
        }

        // Rotation
        if (Input.GetMouseButton(1)) {
            var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            targetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            targetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // Translation
        var translation = GetInputTranslationDirection() * Time.deltaTime;

        // Speed up movement when shift key held
        if (Input.GetKey(KeyCode.LeftShift)) {
            translation *= 10.0f;
        }

        // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
        boost += Input.mouseScrollDelta.y * 0.2f;
        translation *= Mathf.Pow(2.0f, boost);

        targetCameraState.Translate(translation);

        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        interpolatingCameraState.LerpTowards(targetCameraState, positionLerpPct, rotationLerpPct);

        interpolatingCameraState.UpdateTransform(transform);

        var currentPosition = transform.position;
        TravelledDistance += Mathf.Abs(Vector3.Distance(_lastPosition, currentPosition));

        var currentRotation = transform.eulerAngles;
        TotalRotation += Mathf.Abs(_lastRotation.x - currentRotation.x);
        TotalRotation += Mathf.Abs(_lastRotation.y - currentRotation.y);
        TotalRotation += Mathf.Abs(_lastRotation.z - currentRotation.z);
        
        _lastPosition = currentPosition;
        _lastRotation = currentRotation;
    }
}