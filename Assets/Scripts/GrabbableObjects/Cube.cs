using System;
using System.Collections.Generic;
using UnityEngine;

namespace Illumetry.Unity.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    public class Cube : MonoBehaviour, IStylusPointerHandler, IStylusPointerGrabbable
    {
        public Transform visual;
        private Dictionary<int, BaseStylusPointer> _activePointers = new Dictionary<int, BaseStylusPointer>();
        private Rigidbody _rb;
        private FixedJoint _fixedJoint;
        private Vector3 _startPos;
        private Quaternion _startRot;
        private Vector3 _lastGrabVelocity;
        private Vector3 _lastGrabAngularVelocity;

        private Vector3 _visualLocalPos;
        private Quaternion _visualLocalRot;
        private Vector3 _visualLocalScale;
        private Collider _mainCollider;

        public Collider ColliderForGrab => _mainCollider;

        private void Awake()
        {
            _mainCollider = GetComponent<Collider>();
            _startPos = transform.position;
            _startRot = transform.rotation;

            _visualLocalPos = visual.localPosition;
            _visualLocalRot = visual.localRotation;
            _visualLocalScale = visual.localScale;
        }

        private void OnEnable()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody>();
            }
        }

        private void SaveLastGrabVelocities(BaseStylusPointer baseStylusPointer)
        {
            if (baseStylusPointer == null || baseStylusPointer.Stylus == null)
            {
                _lastGrabVelocity = Vector3.zero;
                _lastGrabAngularVelocity = Vector3.zero;
                return;
            }

            _lastGrabVelocity = baseStylusPointer.Stylus.ExtrapolatedVelocity;
            _lastGrabAngularVelocity = baseStylusPointer.Stylus.ExtrapolatedAngularVelocity;
        }

        private void UpdateGrabState()
        {
            BaseStylusPointer stylusGrabber = GetCurrentGrabber();
            bool isGrab = stylusGrabber != null;

            if (isGrab)
            {
                if (_fixedJoint == null)
                {
                    _fixedJoint = gameObject.AddComponent<FixedJoint>();
                }

                SetVisualToParentGrabber(stylusGrabber);
                _fixedJoint.connectedBody = stylusGrabber.PhysicComponent;
                _rb.useGravity = false;
            }
            else
            {
                if (_fixedJoint != null)
                {
                    Destroy(_fixedJoint);
                }


                SetVisualParentMe();
                _rb.useGravity = true;
                _rb.velocity = _lastGrabVelocity;
                _rb.angularVelocity = _lastGrabAngularVelocity;
            }
        }

        private void OnDestroy()
        {
            SetVisualParentMe();
        }

        private void SetVisualToParentGrabber(BaseStylusPointer baseStylusPointer)
        {
            visual.SetParent(baseStylusPointer.transform);
        }

        private void SetVisualParentMe()
        {
            visual.SetParent(transform);
            visual.transform.localPosition = _visualLocalPos;
            visual.transform.localRotation = _visualLocalRot;
            visual.transform.localScale = _visualLocalScale;
        }

        private BaseStylusPointer GetCurrentGrabber()
        {
            foreach (var kvp in _activePointers)
            {
                return kvp.Value;
            }

            return null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_rb != null)
                {
                    _rb.velocity = Vector3.zero; 
                    _rb.angularVelocity = Vector3.zero;
                }

                transform.position = _startPos;
                transform.rotation = _startRot;
            }
        }


        public void OnStartStylusPointerGrab(BaseStylusGrabberPointer baseStylusGrabberPointer) {
            _activePointers[baseStylusGrabberPointer.Stylus.Id] = baseStylusGrabberPointer;
            UpdateGrabState();
        }

        public void OnStylusPointerGrabbing(BaseStylusGrabberPointer baseStylusGrabberPointer) {
          
        }

        public void OnEndStylusPointerGrab(BaseStylusGrabberPointer baseStylusGrabberPointer) {
            _activePointers.Remove(baseStylusGrabberPointer.Stylus.Id);
            SaveLastGrabVelocities(baseStylusGrabberPointer);
            UpdateGrabState();
        }

        public MonoBehaviour GetMonoBehaviour() {
            return this;
        }

        public void OnStylusPointerWasEnter(BaseStylusPointer baseStylusPointer) {
           
        }

        public void OnStylusPointerWasExit(BaseStylusPointer baseStylusPointer) {
            
        }
    }
}