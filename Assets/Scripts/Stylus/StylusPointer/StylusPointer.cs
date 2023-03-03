using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StylusPointer: BaseStylusPointer {

    [SerializeField] private Vector3 _triggerSize = Vector3.one;
    [SerializeField] private Vector3 _triggerOffset;
    [SerializeField, Header("If no, will using BoxCast")] private bool _useOverlapBox;
    [SerializeField] private bool _detectOnlyNearestCollider = true;

    private List<Collider> _enteredColliders = null;

    protected override void ResetPointer() {
        base.ResetPointer();

        if (_enteredColliders == null) {
            _enteredColliders = new List<Collider>();
        }

        _enteredObjects.Clear();
        _enteredColliders.Clear();
    }

    private void FixedUpdate() {
        UpdateColliders(_useOverlapBox);
    }

    private void UpdateColliders(bool useOverlapBox) {

        Vector3 posTrigger = transform.position;
        Vector3 triggerSize = _triggerSize;
        triggerSize.Scale(transform.lossyScale);

        RaycastHit[] hits = useOverlapBox ? new RaycastHit[0] : Physics.BoxCastAll(posTrigger, triggerSize * 0.5f, transform.forward, transform.rotation, 0.0001f);
        Collider[] currentColliders = useOverlapBox ? Physics.OverlapBox(posTrigger, triggerSize * 0.5f, transform.rotation) : new Collider[hits.Length];

        if(!useOverlapBox) {

            for (int i = 0; i < hits.Length; i++) {
                currentColliders[i] = hits[i].collider;
            }
        }

        if (currentColliders.Length > 0) {
            currentColliders = currentColliders.OrderBy(c => (c.transform.position - transform.position).magnitude).ToArray();
        }

        List<Collider> noFoundColliders = new List<Collider>();
        noFoundColliders.AddRange(_enteredColliders);
        _enteredColliders.Clear();

        if (!_detectOnlyNearestCollider) {
            for (int i = currentColliders.Length - 1; i >= 0; i--) {
                Collider tryCollider = currentColliders[i];
                Collider currentCollider = tryCollider == null ? hits[i].collider : tryCollider;

                bool isFound = false;

                for (int j = noFoundColliders.Count - 1; j >= 0; j--) {
                    Collider pCollider = noFoundColliders[j];

                    if (pCollider == currentCollider) {
                        isFound = true;
                        noFoundColliders.RemoveAt(j);
                        break;
                    }
                }

                if (!isFound) {
                    _enteredColliders.Add(currentCollider);
                    ColliderWasEnter(currentCollider);
                } else {
                    _enteredColliders.Add(currentCollider);
                    ColliderStay(currentCollider);
                }
            }
        }
        else {
            Collider nearestCollider = currentColliders.Length > 0 ? currentColliders[0] : null;
            bool isFound = false;

            if (nearestCollider != null) {
                for (int j = noFoundColliders.Count - 1; j >= 0; j--) {
                    Collider pCollider = noFoundColliders[j];

                    if (pCollider == nearestCollider) {
                        isFound = true;
                        noFoundColliders.RemoveAt(j);
                    }
                }

                if (!isFound) {
                    _enteredColliders.Add(nearestCollider);
                    ColliderWasEnter(nearestCollider);
                } else {
                    _enteredColliders.Add(nearestCollider);
                    ColliderStay(nearestCollider);
                }
            }
        }


        for (int i = noFoundColliders.Count - 1; i >= 0; i--) {
            Collider col = noFoundColliders[i];
            ColliderWasExit(col);
        }
    }

    private void ColliderWasEnter(Collider col) {
        TryAddOrUpdateStylusEnterHandler(col);
    }

    private void ColliderStay(Collider col) {
        TryAddOrUpdateStylusEnterHandler(col);
    }

    private void ColliderWasExit(Collider col) {
        HandleExitCollider(col);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Vector3 posTrigger = transform.InverseTransformPoint(transform.position);
        Gizmos.DrawWireCube(posTrigger + _triggerOffset, _triggerSize);
        Gizmos.matrix = prevMatrix;
    }
#endif
}
