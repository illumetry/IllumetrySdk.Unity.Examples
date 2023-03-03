using UnityEngine;

namespace Illumetry.Unity.Demo
{
    using Illumetry.Unity.Stylus;
    public class StylusVisualContainer : MonoBehaviour
    {
        public BaseStylusPointer StylusPointer => _baseStylusPointer;
        [SerializeField] private BaseStylusPointer _baseStylusPointer;

        internal void Inititalize(Stylus stylus)
        {
            if (stylus == null)
            {
                if (Application.isEditor || Debug.isDebugBuild)
                {
                    Debug.LogError("StylusVisualContainer: Initialize failed! Stylus is null!");
                }

                return;
            }

            if (_baseStylusPointer != null)
            {
                _baseStylusPointer.SetStylus(stylus);
            }
        }
    }
}