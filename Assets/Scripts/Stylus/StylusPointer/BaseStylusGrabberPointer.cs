using UnityEngine;

public abstract class BaseStylusGrabberPointer : BaseStylusPointer
{
    private IStylusPointerGrabbable _grabbingObject;
    protected IStylusPointerGrabbable GrabbedObject => _grabbingObject;

    protected override void OnDeActivated() {
        base.OnDeActivated();
        _grabbingObject = null;
    }

    protected override void ResetPointer() {
        base.ResetPointer();
        _grabbingObject = null;
    }

    protected override void HandleExitObject(IStylusPointerHandler stylusPointerHandler, MonoBehaviour monoBehaviour) {

        if (_grabbingObject != null && monoBehaviour.GetComponent<IStylusPointerGrabbable>() is IStylusPointerGrabbable grabbableObject) {

            if(grabbableObject.ColliderForGrab != null && _grabbingObject.ColliderForGrab != null && grabbableObject.ColliderForGrab.GetInstanceID() == _grabbingObject.ColliderForGrab.GetInstanceID()) {
                HandleEndGrabObject(_grabbingObject);
                _grabbingObject = null;
            }
        }

        base.HandleExitObject(stylusPointerHandler, monoBehaviour);
    }

    protected override void HandleButtonPhaseDown(IStylusPointerClickHandler stylusPointerClickHandler, MonoBehaviour monoBehaviour) {
        base.HandleButtonPhaseDown(stylusPointerClickHandler, monoBehaviour);
        TryStartGrab(monoBehaviour);
    }

    private void Update() {
       
        if(ButtonPhaseIsDown) {
            if(_grabbingObject == null) {
                foreach(var kvp in _enteredObjects) {

                    if(!IsStartedClick(kvp.Key)) {
                        continue;
                    }

                    TryStartGrab(kvp.Value);

                    if(_grabbingObject != null) {
                        break;
                    }
                }
            }
        }
    }

    private void TryStartGrab(MonoBehaviour monoBehaviour) {
        if (_grabbingObject == null) {
            IStylusPointerGrabbable grabbable = monoBehaviour.GetComponent<IStylusPointerGrabbable>();

            if (grabbable != null && grabbable.IsAvaiableGrabByStylusPointer) {
                _grabbingObject = grabbable;
                HandleStartGrabObject(_grabbingObject);
            }
            else {
                _grabbingObject = null;
            }
        }
    }

    protected override void HandleButtonPhaseUp(IStylusPointerClickHandler stylusPointerClickHandler, MonoBehaviour monoBehaviour) {
        base.HandleButtonPhaseUp(stylusPointerClickHandler, monoBehaviour);

        HandleEndGrabObject(_grabbingObject);
        _grabbingObject = null;
    }

    protected virtual void HandleStartGrabObject(IStylusPointerGrabbable grabbableObject) {
        grabbableObject?.OnStartStylusPointerGrab(this);
    }

    protected virtual void HandleEndGrabObject(IStylusPointerGrabbable grabbableObject) {
        grabbableObject?.OnEndStylusPointerGrab(this);
    }

    protected override void HandleUpdatePointerPose(Pose stylusPose, Vector3 stylusWorldVelocity, Vector3 stylusAngularVelocity) {
        base.HandleUpdatePointerPose(stylusPose, stylusWorldVelocity, stylusAngularVelocity);

        if(_grabbingObject != null) {
            _grabbingObject.OnStylusPointerGrabbing(this);
        }
    }
}
