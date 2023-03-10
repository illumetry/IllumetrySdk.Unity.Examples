using System.Collections.Generic;
using UnityEngine;
using Illumetry.Unity.Stylus;

public class StylusPointerVisualChanger : MonoBehaviour {
    [SerializeField] private Stylus _stylus;
    [SerializeField] private List<StylusVisualPointerVariant> _visuals = new List<StylusVisualPointerVariant>();
    [SerializeField] private float _maxPressTimeForDetectClick = 1.5f;
    [SerializeField, Header("Key P (EN)")] private bool _changeVisualByKey;

    private int _currentVisibleIndex = 0;
    private bool _previousPhaseStylusButton;
    private float _startPressButtonTime;

    private void OnEnable() {
        if (_stylus != null) {
            _stylus.OnUpdatedButtonPhase += OnUpdatedButtonPhase;
        }

        foreach (var visual in _visuals) {
            if (!IsValidVisualContainer(visual)) {
                continue;
            }

            visual.gameObject.SetActive(false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        UpdateSelectedVisual();
    }

    private void OnDisable() {
        if (_stylus != null) {
            _stylus.OnUpdatedButtonPhase -= OnUpdatedButtonPhase;
        }
    }

    private void OnDestroy() {
        if (_stylus != null) {
            _stylus.OnUpdatedButtonPhase -= OnUpdatedButtonPhase;
        }
    }

    internal void SetStylus(Stylus stylus) {
        if (stylus == null) {
            Debug.LogWarning("Try set null stylus!");
            return;
        }

        if (_stylus != null) {
            Debug.LogError("Stylus not null!");
            return;
        }

        _stylus = stylus;
        _stylus.OnUpdatedButtonPhase += OnUpdatedButtonPhase;

        foreach (var visual in _visuals) {
            if (!IsValidVisualContainer(visual)) {
                continue;
            }

            visual.Inititalize(stylus);
            visual.gameObject.SetActive(false);
        }
    }

    private void OnUpdatedButtonPhase(Stylus stylus, bool isPressed) {
        if (_previousPhaseStylusButton != isPressed) {
            if (isPressed) {
                //Button down phase.
                _startPressButtonTime = Time.time;
            }
            else {
                //Click completed.
                if (_maxPressTimeForDetectClick > Time.time - _startPressButtonTime) {
                    StylusVisualPointerVariant visualContainer = GetCurrentVisualContainer();
                    BaseStylusPointer baseStylusPointer = visualContainer == null ? null : visualContainer.StylusPointer;

                    if (baseStylusPointer == null || !baseStylusPointer.enabled) {
                        if (!_changeVisualByKey) {
                            NextVisual();
                        }
                    }
                }
            }
        }

        _previousPhaseStylusButton = isPressed;
    }

    public void NextVisual() {
        _currentVisibleIndex++;
        if (_currentVisibleIndex >= _visuals.Count) {
            _currentVisibleIndex = 0;
        }

        UpdateSelectedVisual();
    }

    private void UpdateSelectedVisual() {
        for (int i = 0; i < _visuals.Count; i++) {
            StylusVisualPointerVariant stylusVisualContainer = _visuals[i];

            if (!IsValidVisualContainer(stylusVisualContainer)) {
                continue;
            }

            bool isActive = _currentVisibleIndex == i;
            stylusVisualContainer.gameObject.SetActive(isActive);
        }
    }

    private void TryFixCurrentVisibleIndex() {
        if (_currentVisibleIndex >= _visuals.Count) {
            _currentVisibleIndex = 0;
        }

        if (_currentVisibleIndex < 0) {
            _currentVisibleIndex = _visuals.Count - 1;
        }
    }

    private bool IsValidVisualContainer(StylusVisualPointerVariant visualContainer) {
        if (visualContainer == null) {
            if (Application.isEditor || Debug.isDebugBuild) {
                Debug.LogError("StylusVisualSetter: visual container, one or more is null!");
            }

            return false;
        }

        return true;
    }

    private StylusVisualPointerVariant GetCurrentVisualContainer() {
        TryFixCurrentVisibleIndex();

        if (_visuals.Count == 0) {
            return null;
        }

        return _visuals[_currentVisibleIndex];
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            NextVisual();
        }
    }
}