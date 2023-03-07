using UnityEngine;

public class ChangeCurrentPointerByClick : MonoBehaviour, IStylusPointerClickHandler {

    public void OnStylusButtonClicked(BaseStylusPointer baseStylusPointer) {
        baseStylusPointer.Stylus.GetComponent<StylusPointerVisualChanger>()?.NextVisual();
    }

    public void OnStylusButtonPhaseDown(BaseStylusPointer baseStylusPointer) {
       
    }

    public void OnStylusButtonPhaseUp(BaseStylusPointer baseStylusPointer) {
        
    }
}
