using UnityEngine;
using Illumetry.Unity.Stylus;

public class StylusVisualPointerVariant : MonoBehaviour {
    public BaseStylusPointer StylusPointer => _baseStylusPointer;
    [SerializeField] private BaseStylusPointer _baseStylusPointer;

    internal void Inititalize(Stylus stylus) {
        if (stylus == null) {
            if (Application.isEditor || Debug.isDebugBuild) {
                Debug.LogError($"{GetType()}: Initialize failed! Stylus is null!");
            }

            return;
        }

        if (_baseStylusPointer != null) {
            _baseStylusPointer.SetStylus(stylus);
        }
    }
}