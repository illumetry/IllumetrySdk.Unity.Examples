using UnityEngine;

public interface IStylusPointerHandler
{
    MonoBehaviour GetMonoBehaviour();
    void OnStylusPointerWasEnter(BaseStylusPointer baseStylusPointer);
    void OnStylusPointerWasExit(BaseStylusPointer baseStylusPointer);
}
