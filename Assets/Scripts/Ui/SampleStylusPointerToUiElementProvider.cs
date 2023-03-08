using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class SampleStylusPointerToUiElementProvider : MonoBehaviour, IStylusPointerHandler, IStylusPointerClickHandler {

    [SerializeField, Header("IStylusPointerClickHandler")] private List<MonoBehaviour> _clickHandlers = new List<MonoBehaviour>();

    private void OnValidate() {
        ValidateHandlers();
    }

    private void OnEnable() {
        ValidateHandlers();
    }

    private void ValidateHandlers() {
        for (int i = _clickHandlers.Count - 1; i >= 0; i--) {
            if (_clickHandlers[i] as IStylusPointerClickHandler == null || _clickHandlers[i].GetInstanceID() == GetInstanceID()) {
                _clickHandlers.RemoveAt(i);
            }
        }
    }

    public MonoBehaviour GetMonoBehaviourForStylusPointer() {
        return this;
    }

    public void OnStylusPointerWasEnter(BaseStylusPointer baseStylusPointer) {
        //Debug.Log("Stylus Pointer - ENTER");

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.pointerEnter = gameObject;
        ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
    }

    public void OnStylusPointerWasExit(BaseStylusPointer baseStylusPointer) {
        //Debug.Log("Stylus Pointer - EXIT");

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerExitHandler);
        EventSystem.current.SetSelectedGameObject(null, pointerEventData);
    }

    public void OnStylusButtonPhaseDown(BaseStylusPointer baseStylusPointer) {
        //Debug.Log("Stylus Pointer - DOWN");

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.pointerPress = gameObject;
        ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerDownHandler);

        foreach(var handler in _clickHandlers) {
            (handler as IStylusPointerClickHandler)?.OnStylusButtonPhaseDown(baseStylusPointer);
        }
    }

    public void OnStylusButtonPhaseUp(BaseStylusPointer baseStylusPointer) {
        //Debug.Log("Stylus Pointer - UP");

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerUpHandler);

        foreach (var handler in _clickHandlers) {
            (handler as IStylusPointerClickHandler)?.OnStylusButtonPhaseUp(baseStylusPointer);
        }
    }

    public void OnStylusButtonClicked(BaseStylusPointer baseStylusPointer) {
        //Debug.Log("Stylus Pointer - CLICKED");

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.pointerClick = gameObject;
        ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);

        foreach (var handler in _clickHandlers) {
            (handler as IStylusPointerClickHandler)?.OnStylusButtonClicked(baseStylusPointer);
        }
    }
}
