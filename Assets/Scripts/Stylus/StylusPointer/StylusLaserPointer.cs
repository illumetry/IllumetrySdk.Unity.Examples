using Illumetry.Unity;
using Illumetry.Unity.Stylus;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StylusLaserPointer: BaseStylusGrabberPointer{

    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _distance;
    private Dictionary<long, Collider> _activeColliders;
    private Dictionary<long, float> _objectIdToDistanceRay;

    protected override void OnActivated() {
        base.OnActivated();

        _activeColliders = new Dictionary<long, Collider>();
        _objectIdToDistanceRay = new Dictionary<long, float>();
        RendererLCD.OnBeforeRendererL += UpdateLaser;
        RendererLCD.OnBeforeRendererR += UpdateLaser;
        MonoRendererLCD.OnBeforeRenderer += UpdateLaser;
    }


    protected override void OnDeActivated() {
        base.OnDeActivated();

        RendererLCD.OnBeforeRendererL -= UpdateLaser;
        RendererLCD.OnBeforeRendererR -= UpdateLaser;
        MonoRendererLCD.OnBeforeRenderer -= UpdateLaser;

        RemoveAllNotFoundColliders(null);
    }


    private void UpdateLaser() {

        Vector3 direction = transform.forward;
        Vector3 endPos = Vector3.zero;

        Ray ray = new Ray(transform.position, direction * _distance);

        RaycastHit[] hits = Physics.RaycastAll(ray, _distance);
        hits = hits.OrderBy(i => i.distance).ToArray();

        List<Collider> foundsColliders = new List<Collider>();

        if (GrabbedObject == null) {
            if (hits.Length > 0) {

                RaycastHit hitInfo = hits[0];
                endPos = hitInfo.point;

                foundsColliders.Add(hitInfo.collider);

                _activeColliders[hitInfo.colliderInstanceID] = hitInfo.collider;
                _objectIdToDistanceRay[hitInfo.colliderInstanceID] = hitInfo.distance;

                TryAddOrUpdateStylusEnterHandler(hitInfo.collider);
                RemoveAllNotFoundColliders(foundsColliders);
            }
            else {
                RemoveAllNotFoundColliders(null);
                endPos = transform.position + direction * _distance;
            }
        }
        else {
            endPos = transform.position + direction * _objectIdToDistanceRay[GrabbedObject.ColliderForGrab.GetInstanceID()];
        }


        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, endPos);
    }


    private void RemoveAllNotFoundColliders(List<Collider> foundColliders) {
        List<long> willRemoveColliders = new List<long>();

        //Remove missing colliders.
        foreach (var kvp in _activeColliders) {
            bool wasFoundInNewColliders = false;

            if(foundColliders != null) {
                wasFoundInNewColliders = foundColliders.Find(c => c.GetInstanceID() == kvp.Key) != null;
            }

            if (!wasFoundInNewColliders) {
                willRemoveColliders.Add(kvp.Key);
            }
        }

        foreach (long instanceId in willRemoveColliders) {
            HandleExitCollider(_activeColliders[instanceId]);
            _activeColliders.Remove(instanceId);
            _objectIdToDistanceRay.Remove(instanceId);
        }

        willRemoveColliders.Clear();
    }
}
