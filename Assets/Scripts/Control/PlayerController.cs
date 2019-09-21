using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Attributes;
using System;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using RPG.Control.Cursor;
using RPG.Core;
using Cinemachine;

namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
        // Cache references
        private Mover mover;
        private Fighter fighter;
        private Health health;
        ActionScheduler actionScheduler;

        [SerializeField] CinemachineFreeLook freelookCam;
        [SerializeField] ActionMarker waypointMarker, attackMarker;
        bool camControl = false;

        [SerializeField] float navMeshProjectionDistance = 1f;
        [SerializeField] float maxPathLength = 40f;
        const int priority = 2;
        
        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings;

        void Awake()
        {
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        void Update()
        {
            PlayerViewControls();

            if (InteractWithUI()) return;

            if (camControl || health.GetIsDead())
            {
                SetCursor(CursorType.None);
                return;
            }

            if (InteractWithComponent()) return;
            if (InteractWithMovement()) return;
            SetCursor(CursorType.None);
        }

        private void PlayerViewControls()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1))
            {
                SwitchCameraControl();
            }

            freelookCam.m_Lens.FieldOfView = Mathf.Clamp(freelookCam.m_Lens.FieldOfView - Input.GetAxisRaw("Mouse ScrollWheel") * 20, 5f, 80f);
        }

        private void SwitchCameraControl()
        {
            camControl = !camControl;

            if(camControl)
            {
                freelookCam.m_XAxis.m_InputAxisName = "Mouse X";
                freelookCam.m_YAxis.m_InputAxisName = "Mouse Y";
            }
            else
            {
                freelookCam.m_XAxis.m_InputAxisName = "";
                freelookCam.m_YAxis.m_InputAxisName = "";
                freelookCam.m_XAxis.Reset();
                freelookCam.m_YAxis.Reset();
            }
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();

                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(gameObject, attackMarker, waypointMarker, priority))
                    {
                        SetCursor(raycastable.GetCursorType()); 
                        return true;
                    }
                }
            }

            return false;
        }

        private RaycastHit[] RaycastAllSorted()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            float[] distances = new float[hits.Length];

            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = hits[i].distance;
            }

            Array.Sort(distances, hits);

            return hits;
        }

        private bool InteractWithUI()
        {
            bool isInteracting = EventSystem.current.IsPointerOverGameObject();
            if (isInteracting) SetCursor(CursorType.UI); 

            return isInteracting;
        }

        private bool InteractWithMovement()
        {
            bool hasHit = RaycastNavMesh(out Vector3 target);
            if (hasHit)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mover.StartMoveAction(target, 1f, priority);

                    ActionMarker marker = Instantiate(waypointMarker, mover.GetDestination(), waypointMarker.transform.rotation);
                    marker.SetMarker(actionScheduler, null);
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            bool hasHit = Physics.Raycast(GetMouseRay(), out RaycastHit hit);
            target = hit.point;
            if (!hasHit) return false;

            bool isNearNavMesh = NavMesh.SamplePosition(target, out NavMeshHit navHit, navMeshProjectionDistance, NavMesh.AllAreas);
            if (!isNearNavMesh)
            {
                return false;
            }

            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            if (!hasPath) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;
            if (GetPathLength(path) > maxPathLength) return false;

            return true;
        }

        private float GetPathLength(NavMeshPath path)
        {
            float pathLength = 0f;
            if (path.corners.Length < 2) return pathLength;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                pathLength += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return pathLength;
        }

        public void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            UnityEngine.Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }

            return cursorMappings[0];
        }

        private Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
