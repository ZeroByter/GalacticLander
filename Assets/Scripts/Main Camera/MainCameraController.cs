using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour {
    private class CameraTarget
    {
        public float timeAdded;
        public PlayerShipController controller;

        public CameraTarget(PlayerShipController controller)
        {
            this.timeAdded = Time.time;
            this.controller = controller;
        }

        public override bool Equals(object obj)
        {
            if(obj is CameraTarget)
            {
                CameraTarget otherTarget = (CameraTarget)obj;
                return controller == otherTarget.controller;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static MainCameraController Singletron;

    public float dampTime = 0.2f;
    public float screenEdgeBuffer = 4f;
    public float minSize = 3f;

    [HideInInspector]
    public Camera selfCamera;

    private Vector3 moveSpeed;
    private float zoomSpeed;

    private float lastShake = 0;
    private float shakeDuration = 0.5f;
    private float shakeMagnitude = 2;
    private Vector3 lastPosition = new Vector3(0,0,-30);

    private List<CameraTarget> cameraTargets = new List<CameraTarget>();
    private List<Transform> nonPlayerTargets = new List<Transform>();

    private float defaultScreenEdgeBuffer = -1;
    private float defaultMinSize = -1;

    public float GetDefaultScreenEdgeBuffer()
    {
        return defaultScreenEdgeBuffer;
    }

    public void ResetScreenEdgeBuffer()
    {
        screenEdgeBuffer = defaultScreenEdgeBuffer;
    }

    public void SetScreenEdgeBuffer(float newSize)
    {
        screenEdgeBuffer = newSize;
    }

    public float GetDefaultMinSize()
    {
        return defaultMinSize;
    }

    public void SetDefaultMinSize(float newSize)
    {
        minSize = newSize;
    }

    public void ResetDefaultMinSize()
    {
        minSize = defaultMinSize;
    }

    private void Awake() {
        Singletron = this;
        selfCamera = GetComponent<Camera>();

        if (defaultMinSize != -1 || defaultScreenEdgeBuffer != -1)
        {
            defaultScreenEdgeBuffer = screenEdgeBuffer;
            defaultMinSize = minSize;
        }
    }

    private void Update() {
        /*if(SceneManager.GetActiveScene().name == "Main Menu" && Input.GetKeyDown(KeyCode.Slash)) {
            StartShake(1.5f, 0.5f);
        }*/
    }

    private void FixedUpdate() {
        if (GetTargetsCount() > 0) {
            lastPosition = transform.position;
            selfCamera.orthographicSize = Mathf.SmoothDamp(selfCamera.orthographicSize, GetRequiredSize(), ref zoomSpeed, dampTime);
            transform.position = Vector3.SmoothDamp(transform.position, GetAveragePosition(), ref moveSpeed, dampTime) + GetShakePosition();
        } else {
            transform.position = lastPosition + GetShakePosition();
        }
    }

    public static void AddTarget(PlayerShipController target) {
        if (Singletron == null) return;

        CameraTarget newTarget = new CameraTarget(target);

        if (Singletron.cameraTargets.Contains(newTarget)) return; //no duplicates
        Singletron.cameraTargets.Add(newTarget);
    }

    public static void RemoveTarget(PlayerShipController target) {
        Singletron.cameraTargets.RemoveAll(x => x.controller == target);
    }

    public static void AddNonPlayerTarget(Transform target)
    {
        if (Singletron == null) return;

        if (Singletron.nonPlayerTargets.Contains(target)) return; //no duplicates
        Singletron.nonPlayerTargets.Add(target);
    }

    public static void RemoveNonPlayerTarget(Transform target)
    {
        Singletron.nonPlayerTargets.Remove(target);
    }

    public static int GetTargetsCount() {
        int count = Singletron.nonPlayerTargets.Count;

        foreach(CameraTarget target in Singletron.cameraTargets)
        {
            if (!target.controller.IsMine() && Time.time - target.timeAdded < 0.025f) continue;

            count++;
        }

        return count;
    }

    public static void ForcePosition()
    {
        if (Singletron == null) return;

        var averagePosition = Singletron.GetAveragePosition();

        if (float.IsNaN(averagePosition.x)) return;

        Singletron.transform.position = averagePosition;
        Singletron.selfCamera.orthographicSize = Singletron.GetRequiredSize();
    }

    public static void StartShake(float magnitude, float duration) {
        Singletron.shakeMagnitude = magnitude;
        Singletron.shakeDuration = duration;
        Singletron.lastShake = Time.time;

        print(string.Format("mag = {0} - dur = {1} - time = {2}", magnitude, duration, Time.time));
    }

    private Vector3 GetShakePosition() {
        if (lastShake == 0) return Vector3.zero;

        return Random.insideUnitCircle * Mathf.Lerp(shakeMagnitude, 0, (Time.time - lastShake) / shakeDuration);
    }

    private Vector3 GetAveragePosition() {
        var targetsCount = GetTargetsCount();
        if (targetsCount == 0) return new Vector3(0,0,-30);

        float x = 0;
        float y = 0;

        foreach(CameraTarget target in cameraTargets) {
            if (!target.controller.IsMine() && Time.time - target.timeAdded < 0.2f) continue;

            Vector3 velocityOffset = GetVelocityOffset(target.controller.selfRigidbody, target.controller.velocityViewOffsetModifier);

            x += target.controller.transform.position.x + velocityOffset.x;
            y += target.controller.transform.position.y + velocityOffset.y;
        }

        foreach (Transform target in nonPlayerTargets)
        {
            x += target.position.x;
            y += target.position.y;
        }

        return new Vector3(x / targetsCount, y / targetsCount, -30);
    }

    private Vector3 GetVelocityOffset(Rigidbody2D rigidbody, float modifier) {
        return new Vector3(rigidbody.velocity.x * modifier, rigidbody.velocity.y * modifier, 0);
    }

    private float GetRequiredSize() {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(GetAveragePosition());

        float size = 0;

        foreach (CameraTarget target in cameraTargets)
        {
            if (!target.controller.IsMine() && Time.time - target.timeAdded < 0.2f) continue;

            Vector3 targetLocalPos = transform.InverseTransformPoint(target.controller.transform.position);
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / selfCamera.aspect);
        }

        foreach (Transform target in nonPlayerTargets)
        {
            Vector3 targetLocalPos = transform.InverseTransformPoint(target.position);
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / selfCamera.aspect);
        }

        size += screenEdgeBuffer;
        size = Mathf.Max(size, minSize);

        return size;
    }
}
