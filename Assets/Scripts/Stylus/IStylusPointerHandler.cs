using UnityEngine;

public interface IStylusPointerHandler
{
    MonoBehaviour GetMonoBehaviourForStylusPointer();
    void OnStylusPointerWasEnter(BaseStylusPointer baseStylusPointer);
    void OnStylusPointerWasExit(BaseStylusPointer baseStylusPointer);
}
