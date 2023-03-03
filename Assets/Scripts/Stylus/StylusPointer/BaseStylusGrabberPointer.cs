using UnityEngine;

public abstract class BaseStylusGrabberPointer : BaseStylusPointer
{
    private IStylusPointerGrabbable _grabbedObject;
    protected IStylusPointerGrabbable GrabbedObject => _grabbedObject;

    protected override void OnDeActivated() {
        base.OnDeActivated();
        _grabbedObject = null;
    }

    protected override void ResetPointer() {
        base.ResetPointer();
        _grabbedObject = null;
    }

    protected override void HandleButtonPhaseDown(IStylusPointerClickHandler stylusPointerClickHandler, MonoBehaviour monoBehaviour) {
        base.HandleButtonPhaseDown(stylusPointerClickHandler, monoBehaviour);

        if (_grabbedObject == null) {
            _grabbedObject = monoBehaviour.GetComponent<IStylusPointerGrabbable>();
            HandleStartGrabObject(_grabbedObject);
        }
    }

    protected override void HandleButtonPhaseUp(IStylusPointerClickHandler stylusPointerClickHandler, MonoBehaviour monoBehaviour) {
        base.HandleButtonPhaseUp(stylusPointerClickHandler, monoBehaviour);

        if (_grabbedObject != null) {
            HandleEndGrabObject(_grabbedObject);
        }

        _grabbedObject = null;
    }

    protected virtual void HandleStartGrabObject(IStylusPointerGrabbable grabbableObject) {
        grabbableObject?.OnStartStylusPointerGrab(this);
    }

    protected virtual void HandleEndGrabObject(IStylusPointerGrabbable grabbableObject) {
        grabbableObject?.OnEndStylusPointerGrab(this);
    }

    protected override void HandleUpdatePointerPose(Pose stylusPose, Vector3 stylusWorldVelocity, Vector3 stylusAngularVelocity) {
        base.HandleUpdatePointerPose(stylusPose, stylusWorldVelocity, stylusAngularVelocity);

        if(_grabbedObject != null) {
            _grabbedObject.OnStylusPointerGrabbing(this);
        }
    }
}
