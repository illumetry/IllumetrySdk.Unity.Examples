public interface IStylusPointerClickHandler
{
    void OnStylusButtonPhaseDown(BaseStylusPointer baseStylusPointer);
    void OnStylusButtonPhaseUp(BaseStylusPointer baseStylusPointer);
    void OnStylusButtonClicked(BaseStylusPointer baseStylusPointer);
}
