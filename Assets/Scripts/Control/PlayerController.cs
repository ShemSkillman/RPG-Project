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

        [Header("Free Camera Config")]
        [SerializeField] CinemachineFreeLook freelookCam;
        [SerializeField] float zoomSensitivity = 20f;
        [SerializeField] float minFov = 5;
        [SerializeField] float maxFov = 80;

        [SerializeField] ActionMarker waypointMarker, attackMarker;
        bool freeCamControl = false;

        // Max distance allowed from a valid move position
        [SerializeField] float navMeshProjectionDistance = 1f;

        // Prevents lengthy autopilot of player movement
        [SerializeField] float maxPathLength = 40f;

        // Highest priority
        const int priority = 2;
        
        [System.Serializable]
        struct CursorMapping
        {
            // Player action
            public CursorType type;

            // Cursor graphic
            public Texture2D texture;

            // Click point offset
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

            // UI blocks cursor
            if (InteractWithUI()) return;

            // Cannot perform actions when free cam mode or dead
            if (freeCamControl || health.GetIsDead())
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

            ZoomCamera();
        }

        private void ZoomCamera()
        {
            float fov = freelookCam.m_Lens.FieldOfView - (Input.GetAxisRaw("Mouse ScrollWheel") * zoomSensitivity);
            freelookCam.m_Lens.FieldOfView = Mathf.Clamp(fov, minFov, maxFov);
        }

        // Hold right mouse button to rotate orbital camera
        private void SwitchCameraControl()
        {
            freeCamControl = !freeCamControl;

            if(freeCamControl)
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

        // Checks if player wants to interact (perform actions) on interactable world objects
        private bool InteractWithComponent()
        {
            // Processes closest raycastable objects first
            RaycastHit[] hits = RaycastAllSorted();

            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();

                foreach (IRaycastable raycastable in raycastables)
                {
                    // Checks raycastable can be interacted with at that time
                    if (raycastable.HandleRaycast(gameObject, attackMarker, waypointMarker, priority))
                    {
                        SetCursor(raycastable.GetCursorType()); 
                        return true;
                    }
                }
            }

            return false;
        }

        // Returns raycasts in order of distance
        private RaycastHit[] RaycastAllSorted()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            float[] distances = new float[hits.Length];

            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = hits[i].distance;
            }

            // Orders distances and re-arranges raycast array
            Array.Sort(distances, hits);

            return hits;
        }

        // Checks if cursor over UI
        private bool InteractWithUI()
        {
            bool isInteracting = EventSystem.current.IsPointerOverGameObject();
            if (isInteracting) SetCursor(CursorType.UI); 

            return isInteracting;
        }

        // Check if player can move to pointed location
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

        // Check if desired move location is valid
        private bool RaycastNavMesh(out Vector3 target)
        {
            bool hasHit = Physics.Raycast(GetMouseRay(), out RaycastHit hit);
            target = hit.point;

            // Hit anything?
            if (!hasHit) return false;

            bool isNearNavMesh = NavMesh.SamplePosition(target, out NavMeshHit navHit, navMeshProjectionDistance, NavMesh.AllAreas);
            if (!isNearNavMesh)
                return false;

            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);

            if (!hasPath) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;
            if (GetPathLength(path) > maxPathLength) return false;

            return true;
        }

        // Path length manually calculated
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

        // Raycasts from mouse position in camera perspective 
        private Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
