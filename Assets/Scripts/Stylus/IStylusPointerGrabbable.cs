using UnityEngine;

public interface IStylusPointerGrabbable
{
    Collider ColliderForGrab { get; }
    bool IsAvaiableGrabByStylusPointer { get; }
    void OnStartStylusPointerGrab(BaseStylusGrabberPointer baseStylusGrabberPointer);
    void OnStylusPointerGrabbing(BaseStylusGrabberPointer baseStylusGrabberPointer);
    void OnEndStylusPointerGrab(BaseStylusGrabberPointer baseStylusGrabberPointer);
}
