using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockOn : MonoBehaviour
{
    [SerializeField]
    private DetectionSphere _detection;

    private HashSet<Transform> _targets = new();

    private bool _locked = false;

    public EventHandler<Transform> LockOnStart;
    public EventHandler LockOnStop;

    private void Start()
    {
        _detection.ObjectEntered += Entered;
        _detection.ObjectExited += Exited;
    }

    private void Exited(object sender, GameObject e)
    {
        if (e != null && _targets.Contains(e.transform.root))
            _targets.Remove(e.transform.root);
    }

    private void Entered(object sender, GameObject e)
    {
        if (e != null)
            _targets.Add(e.transform.root);
    }

    public void LockOnTarget(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) 
            return;

        if (_targets.Count == 0)
            return;

        if (!_locked)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            // Get the center of the screen
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            foreach (Transform target in _targets)
            {
                if (target == null) continue;

                // Convert the target's position to screen space
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position);

                // Calculate the distance from the center of the screen
                float distance = Vector2.Distance(screenCenter, screenPosition);

                // Check if this target is the closest
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            if (closestTarget != null)
            {
                _locked = true;
                LockOnStart.Invoke(this, closestTarget);

                List<Transform> targetsToRemove = new();
                foreach (Transform target in _targets)
                {
                    if (target == null) 
                        targetsToRemove.Add(target);
                }

                foreach (Transform target in targetsToRemove)
                {
                    _targets.Remove(target);
                }
            }
        }
        else
        {
            LockOnStop.Invoke(this, EventArgs.Empty);
            _locked = false;
        }        
    }
}
