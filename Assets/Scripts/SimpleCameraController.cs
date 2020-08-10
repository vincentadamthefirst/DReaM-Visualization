using JetBrains.Annotations;
using UnityEngine;
using Utils;
using Visualization.Agents;

public class SimpleCameraController : MonoBehaviour {
    public class CameraState {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t) {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation) {
            Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct) {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t) {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
        }
    }

    public CameraState m_TargetCameraState = new CameraState();
    public CameraState m_InterpolatingCameraState = new CameraState();

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

    private bool _settingsOpen;
    
    /// <summary>
    /// Set if the Quantitative Evaluation is performed and the camera is moved throughout the scene.
    /// </summary>
    public bool AutomaticMovement { get; set; }
    
    public Agent LockedOnAgent { get; set; }
    
    public bool LockedOnAgentIsSet { get; set; }

    private Vector3 _lastPosition;

    private Vector3 _lastRotation;
    
    public float TravelledDistance { get; private set;  }
    
    public float TotalRotation { get; private set; }

    public void SetSettingsOpen(bool value) {
        _settingsOpen = value;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void OnEnable() {
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);

        _lastPosition = transform.position;
    }

    Vector3 GetInputTranslationDirection() {
        Vector3 direction = new Vector3();
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

            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
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

        m_TargetCameraState.Translate(translation);

        // Framerate-independent interpolation
        // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
        var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

        m_InterpolatingCameraState.UpdateTransform(transform);

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