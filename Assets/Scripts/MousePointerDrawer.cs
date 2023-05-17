using UnityEngine;

public class MousePointerDrawer : MonoBehaviour
{
    [SerializeField] private RectTransform _pointer;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private MouseHandler _mouseHandler;
    private RectTransform _canvasRectTransform;

    private void OnEnable() {

        if(_canvasRectTransform == null) {
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        }

        _mouseHandler.OnUpdatedPosition += OnUpdatedPosition;
    }

    private void OnDisable() {
        _mouseHandler.OnUpdatedPosition -= OnUpdatedPosition;
    }

    private void OnUpdatedPosition(int x,int y) {

        Vector2 vector;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRectTransform, Input.mousePosition, _canvas.worldCamera, out vector);
        _pointer.localPosition = vector;
    }
}
