using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Cube : MonoBehaviour, IStylusPointerHandler, IStylusPointerGrabbable {
    public Transform visual;
    public Collider ColliderForGrab => _mainCollider;
    public bool IsAvaiableGrabByStylusPointer => _isGrabbing == false;

    [SerializeField] private float _outlineWidth = 1f;
    [SerializeField] private Outline _outline;

    private Dictionary<int, BaseStylusPointer> _activePointers = new Dictionary<int, BaseStylusPointer>();
    private Rigidbody _rb;
    private FixedJoint _fixedJoint;
    private Vector3 _startPos;
    private Quaternion _startRot;
    private Vector3 _lastGrabVelocity;
    private Vector3 _lastGrabAngularVelocity;

    private Vector3 _visualLocalPos;
    private Quaternion _visualLocalRot;
    private Vector3 _visualLocalScale;
    private Collider _mainCollider;
    private Pose _offsetPoseGrabber;

    private float _startChangeOutline;
    private float _previousWidthOutline;
    private bool _isGrabbing;

    private void Awake() {
        _mainCollider = GetComponent<Collider>();
        _startPos = transform.position;
        _startRot = transform.rotation;

        _visualLocalPos = visual.localPosition;
        _visualLocalRot = visual.localRotation;
        _visualLocalScale = visual.localScale;

        _previousWidthOutline = 0f;
    }

    private void OnEnable() {
        if (_rb == null) {
            _rb = GetComponent<Rigidbody>();
        }
    }

    public MonoBehaviour GetMonoBehaviourForStylusPointer() {
        return this;
    }

    private void SaveLastGrabVelocities(BaseStylusPointer baseStylusPointer) {
        if (baseStylusPointer == null || baseStylusPointer.Stylus == null) {
            _lastGrabVelocity = Vector3.zero;
            _lastGrabAngularVelocity = Vector3.zero;
            return;
        }

        _lastGrabVelocity = baseStylusPointer.Stylus.ExtrapolatedVelocity;
        _lastGrabAngularVelocity = baseStylusPointer.Stylus.ExtrapolatedAngularVelocity;
    }

    private void OnDestroy() {
        if (visual.parent == null) {
            Destroy(visual.gameObject);
        }
    }

    private void SaveOffsets(BaseStylusPointer baseStylusPointer) {
        visual.SetParent(null);

        Quaternion invPointerRotation = Quaternion.Inverse(baseStylusPointer.transform.rotation);
        Vector3 invPointerPosition = -(invPointerRotation * baseStylusPointer.transform.position);

        _offsetPoseGrabber.position = invPointerPosition + invPointerRotation * visual.transform.position;
        _offsetPoseGrabber.rotation = invPointerRotation * visual.transform.rotation;
    }

    private void ReturnVisualToBaseParent() {
        visual.SetParent(transform);
        visual.transform.localPosition = _visualLocalPos;
        visual.transform.localRotation = _visualLocalRot;
        visual.transform.localScale = _visualLocalScale;
    }

    public void ResetPoseCube() {
        if (_rb != null) {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        transform.position = _startPos;
        transform.rotation = _startRot;
    }

    public void OnStartStylusPointerGrab(BaseStylusGrabberPointer baseStylusGrabberPointer) {

        if (_fixedJoint == null) {
            _fixedJoint = gameObject.AddComponent<FixedJoint>();
        }

        SaveOffsets(baseStylusGrabberPointer);
        _fixedJoint.connectedBody = baseStylusGrabberPointer.PhysicComponent;
        _rb.useGravity = false;
        _isGrabbing = true;
    }

    public void OnStylusPointerGrabbing(BaseStylusGrabberPointer baseStylusGrabberPointer) {
        visual.position = baseStylusGrabberPointer.transform.position + baseStylusGrabberPointer.transform.rotation * _offsetPoseGrabber.position;
        visual.rotation = baseStylusGrabberPointer.transform.rotation * _offsetPoseGrabber.rotation;
    }

    public void OnEndStylusPointerGrab(BaseStylusGrabberPointer baseStylusGrabberPointer) {

        SaveLastGrabVelocities(baseStylusGrabberPointer);

        if (_fixedJoint != null) {
            Destroy(_fixedJoint);
            _fixedJoint = null;
        }


        ReturnVisualToBaseParent();
        _rb.useGravity = true;
        _rb.velocity = _lastGrabVelocity;
        _rb.angularVelocity = _lastGrabAngularVelocity;
        _isGrabbing = false;
    }

    public void OnStylusPointerWasEnter(BaseStylusPointer baseStylusPointer) {
        _outline.enabled = true;

        if (_activePointers.Count == 0) {
            _startChangeOutline = Time.time;
            _previousWidthOutline = _outline.OutlineWidth;
        }

        _activePointers[baseStylusPointer.Id] = baseStylusPointer;
    }

    public void OnStylusPointerWasExit(BaseStylusPointer baseStylusPointer) {

        bool hasPointers = false;
        if (_activePointers.Count > 0) {
            hasPointers = true;
        }

        _activePointers.Remove(baseStylusPointer.Id);

        if (_activePointers.Count == 0 && hasPointers) {
            _startChangeOutline = Time.time;
            _previousWidthOutline = _outline.OutlineWidth;
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetPoseCube();
        }

        //Update otline width.

        if (_outline != null) {
            float outlineWidthTarget = _activePointers.Count == 0 ? 0f : _outlineWidth;

            float elapsedTime = Time.time - _startChangeOutline;
            float progressChangeWidth = Mathf.Clamp01(elapsedTime / 0.2f);
            _outline.OutlineWidth = Mathf.Lerp(_previousWidthOutline, outlineWidthTarget, progressChangeWidth);

            if (progressChangeWidth >= 1f) {
                if (outlineWidthTarget == 0) {
                    _outline.enabled = false;
                }
            }
        }
    }
}
