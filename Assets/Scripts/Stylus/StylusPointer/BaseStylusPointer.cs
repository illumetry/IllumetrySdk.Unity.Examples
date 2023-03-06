using Illumetry.Unity.Stylus;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseStylusPointer: MonoBehaviour {

    /// <summary>
    /// Pose - Pointer pose.
    /// Vector3 - Pointer world space velocity.
    /// Vector3 - Pointer world space angular velocity.
    /// </summary>
    internal event Action<Pose, Vector3, Vector3> OnPointerUpdatedPose;

    public Stylus Stylus => _stylus;
    public Rigidbody PhysicComponent => physicComponent;

    /// <summary>
    /// Equals Stylus.Id
    /// </summary>
    public int Id => Stylus != null ? Stylus.Id : -1;

    [SerializeField] protected Stylus _stylus;
    [SerializeField] protected Rigidbody physicComponent;
    
    protected Dictionary<long, MonoBehaviour> _enteredObjects = new Dictionary<long, MonoBehaviour>();

    protected Dictionary<long, long> _colliderInstanceIdToEnteredObjectInstanceId = new Dictionary<long, long>();
    protected Dictionary<long, long> _enteredObjectInstanceIdToColliderInstanceId = new Dictionary<long, long>();
    protected Dictionary<long, Collider> _enteredObjectInstanceIdToColliders = new Dictionary<long, Collider>();
    protected bool ButtonPhaseIsDown => _previousButtonPhase;

    private HashSet<long> _startClickEventObjects = new HashSet<long>();
    private bool _previousButtonPhase = false;

    private void OnEnable() {
        ResetPointer();
        OnActivated();
    }

    private void OnDisable() {
        OnDeActivated();
        ResetPointer();
    }

    private void OnDestroy() {
        OnDeActivated();
        ResetPointer();
    }

    protected virtual void Reset() {

        if (transform.parent != null) {
            _stylus = transform.parent.GetComponentInChildren<Stylus>();
        }

        physicComponent = GetComponent<Rigidbody>();
    }

    protected virtual void OnActivated() {

        if (physicComponent != null) {
            physicComponent.isKinematic = true;
        }

        if (Stylus != null) {
            Stylus.OnUpdatedButtonPhase += HandleButtonPhase;
            Stylus.OnUpdatedPose += HandleUpdatePointerPose;
        }
        else {
            Debug.LogError($"Stylus is null, check reference");
        }
    }

    protected virtual void OnDeActivated() {
        
        ReleaseAllObjects();
        _previousButtonPhase = false;

        if(Stylus != null ) {
            Stylus.OnUpdatedButtonPhase -= HandleButtonPhase;
            Stylus.OnUpdatedPose -= HandleUpdatePointerPose;
        }
    }

    internal void SetStylus(Stylus stylus) {
        if (_stylus != null) {
            ResetPointer();
        }

        _stylus = stylus;
    }

    protected virtual void ResetPointer() {
      
        if (_enteredObjects == null) {
            _enteredObjects = new Dictionary<long, MonoBehaviour>();
        }

        _startClickEventObjects.Clear();
        _enteredObjects.Clear();
    }

    protected MonoBehaviour TryAddOrUpdateStylusEnterHandler(Collider col) {

        if (col == null) {
            return null;
        }

        IStylusPointerHandler stylusPointerHandler = col.GetComponent<IStylusPointerHandler>();
        MonoBehaviour monoBehaviour = stylusPointerHandler?.GetMonoBehaviour();

        if (stylusPointerHandler != null && monoBehaviour != null) {

            long instanceId = monoBehaviour.GetInstanceID();
            long colliderInstanceId = col.GetInstanceID();
            bool isEntered = _enteredObjects.ContainsKey(instanceId) == false;

            _enteredObjects[instanceId] = monoBehaviour;
            _colliderInstanceIdToEnteredObjectInstanceId[colliderInstanceId] = instanceId;
            _enteredObjectInstanceIdToColliderInstanceId[instanceId] = colliderInstanceId;
            _enteredObjectInstanceIdToColliders[instanceId] = col;

            if (isEntered) {
                HandleEnterObject(stylusPointerHandler, monoBehaviour);
            }

            return monoBehaviour;
        }

        return null;
    }

    private void ReleaseAllObjects() {

        long[] entereObjectsInstanceIds = _enteredObjects.Keys.ToArray();

        foreach(var instanceId in entereObjectsInstanceIds) {
            HandeExitObjectInternal(instanceId, _enteredObjects[instanceId]);
        }

        _enteredObjects.Clear();
        _colliderInstanceIdToEnteredObjectInstanceId.Clear();
        _enteredObjectInstanceIdToColliderInstanceId.Clear();
        _enteredObjectInstanceIdToColliders.Clear();
        _startClickEventObjects.Clear();
    }

    protected bool IsStartedClick(long objectInstanceId) {
        return _startClickEventObjects.Contains(objectInstanceId);
    }
        
    protected void HandleExitCollider(Collider col) {

        if (col == null) {
            return;
        }

        long colliderInstanceId = col.GetInstanceID();

        if (_colliderInstanceIdToEnteredObjectInstanceId.ContainsKey(colliderInstanceId)) {

            long objectInstanceId = _colliderInstanceIdToEnteredObjectInstanceId[colliderInstanceId];
            MonoBehaviour monoBehaviour = _enteredObjects[objectInstanceId];
            HandeExitObjectInternal(objectInstanceId, monoBehaviour);
        }
    }

    private void HandeExitObjectInternal(long objectInstanceId, MonoBehaviour monoBehaviour) {

        if (monoBehaviour == null) {
            return;
        }

        if (_enteredObjects.ContainsKey(objectInstanceId)) {

            RemoveObjectFromDictionaries(objectInstanceId);

            if (monoBehaviour.GetComponent<IStylusPointerHandler>() is IStylusPointerHandler stylusPointerHandler) {
                HandleExitObject(stylusPointerHandler, monoBehaviour);
            }
        }
    } 

    private void RemoveObjectFromDictionaries(long objectInstanceId) {

        long colliderInstanceId = _enteredObjectInstanceIdToColliderInstanceId[objectInstanceId];
        _enteredObjectInstanceIdToColliderInstanceId.Remove(objectInstanceId);
        _enteredObjectInstanceIdToColliders.Remove(objectInstanceId);
        _enteredObjects.Remove(objectInstanceId);
        _startClickEventObjects.Remove(_colliderInstanceIdToEnteredObjectInstanceId[colliderInstanceId]);
        _colliderInstanceIdToEnteredObjectInstanceId.Remove(colliderInstanceId);
    }

    private void HandleButtonPhase(Stylus stylus, bool phase) {

        if (_previousButtonPhase != phase) {

            if (phase) {
                foreach (var kvpObject in _enteredObjects) {

                    IStylusPointerClickHandler stylusPointerClickHandler = kvpObject.Value?.GetComponent<IStylusPointerClickHandler>();
                    HandleButtonPhaseDown(stylusPointerClickHandler, kvpObject.Value);

                    if (!_startClickEventObjects.Contains(kvpObject.Key)) {
                        _startClickEventObjects.Add(kvpObject.Key);
                    }
                }
            }
            else {
                foreach (var kvpObject in _enteredObjects) {

                    IStylusPointerClickHandler stylusPointerClickHandler = kvpObject.Value?.GetComponent<IStylusPointerClickHandler>();
                    HandleButtonPhaseUp(stylusPointerClickHandler, kvpObject.Value);

                    if (_startClickEventObjects.Contains(kvpObject.Key)) {
                        _startClickEventObjects.Remove(kvpObject.Key);
                        HandleButtonClick(stylusPointerClickHandler, kvpObject.Value);
                    }
                }
            }
        }

        _previousButtonPhase = phase;
    }

    protected virtual void HandleEnterObject(IStylusPointerHandler stylusPointerEnterHandler, MonoBehaviour monoBehaviour) {
        stylusPointerEnterHandler?.OnStylusPointerWasEnter(this);
    }

    protected virtual void HandleExitObject(IStylusPointerHandler stylusPointerHandler, MonoBehaviour monoBehaviour) {
        stylusPointerHandler?.OnStylusPointerWasExit(this);
    }

    protected virtual void HandleButtonPhaseDown(IStylusPointerClickHandler stylusPointerClickHandler, MonoBehaviour monoBehaviour) {
        stylusPointerClickHandler?.OnStylusButtonPhaseDown(this);
    }

    protected virtual void HandleButtonPhaseUp(IStylusPointerClickHandler stylusPointerClickHandler, MonoBehaviour monoBehaviour) {
        stylusPointerClickHandler?.OnStylusButtonPhaseUp(this);
    }

    protected virtual void HandleButtonClick(IStylusPointerClickHandler stylusPointerClickHandler, MonoBehaviour monoBehaviour) {
        stylusPointerClickHandler?.OnStylusButtonClicked(this);
    }

    protected virtual void HandleUpdatePointerPose(Pose stylusPose, Vector3 stylusWorldVelocity, Vector3 stylusAngularVelocity) {
        OnPointerUpdatedPose?.Invoke(stylusPose, stylusWorldVelocity, stylusAngularVelocity);
    }
}
