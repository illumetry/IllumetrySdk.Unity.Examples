using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Cube : MonoBehaviour, IStylusPointerHandler, IStylusPointerGrabbable {
    public Transform visual;
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
    public Collider ColliderForGrab => _mainCollider;
    public bool IsAvaiableGrabByStylusPointer => GetCurrentGrabPointer() == null;

    private void Awake() {
        _mainCollider = GetComponent<Collider>();
        _startPos = transform.position;
        _startRot = transform.rotation;

        _visualLocalPos = visual.localPosition;
        _visualLocalRot = visual.localRotation;
        _visualLocalScale = visual.localScale;
    }

    private void OnEnable() {
        if (_rb == null) {
            _rb = GetComponent<Rigidbody>();
        }
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

    private void UpdateGrabState() {
        BaseStylusPointer stylusPointer = GetCurrentGrabPointer();
        bool isGrab = stylusPointer != null;

        if (isGrab) {
            if (_fixedJoint == null) {
                _fixedJoint = gameObject.AddComponent<FixedJoint>();
            }

            SaveOffsets(stylusPointer);
            _fixedJoint.connectedBody = stylusPointer.PhysicComponent;
            _rb.useGravity = false;
        }
        else {
            if (_fixedJoint != null) {
                Destroy(_fixedJoint);
                _fixedJoint = null;
            }


            ReturnVisualToBaseParent();
            _rb.useGravity = true;
            _rb.velocity = _lastGrabVelocity;
            _rb.angularVelocity = _lastGrabAngularVelocity;
        }
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

    private BaseStylusPointer GetCurrentGrabPointer() {
        foreach (var kvp in _activePointers) {
            return kvp.Value;
        }

        return null;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetPoseCube();
        }
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
        _activePointers[baseStylusGrabberPointer.Stylus.Id] = baseStylusGrabberPointer;
        UpdateGrabState();
    }

    public void OnStylusPointerGrabbing(BaseStylusGrabberPointer baseStylusGrabberPointer) {
        visual.position = baseStylusGrabberPointer.transform.position + baseStylusGrabberPointer.transform.rotation * _offsetPoseGrabber.position;
        visual.rotation = baseStylusGrabberPointer.transform.rotation * _offsetPoseGrabber.rotation;
    }

    public void OnEndStylusPointerGrab(BaseStylusGrabberPointer baseStylusGrabberPointer) {
        _activePointers.Remove(baseStylusGrabberPointer.Stylus.Id);
        SaveLastGrabVelocities(baseStylusGrabberPointer);
        UpdateGrabState();
    }

    public MonoBehaviour GetMonoBehaviour() {
        return this;
    }

    public void OnStylusPointerWasEnter(BaseStylusPointer baseStylusPointer) {

    }

    public void OnStylusPointerWasExit(BaseStylusPointer baseStylusPointer) {

    }
}
