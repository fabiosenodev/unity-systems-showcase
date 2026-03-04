using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FabioSenoDev

{
    [RequireComponent(typeof(MovementManagerScript))]
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerManagerScript : MonoBehaviour
    {
        [Header("Interaction variables")]
        [SerializeField] public float interactionRange = 2f;
        [SerializeField] private float interactionRefreshInteraval = 0.1f;
        [SerializeField] private LayerMask interactableLayers = ~0;

        private MovementManagerScript movement;
        private PlayerControls playerControls;
        private PauseMenuScript pauseMenu;
        private IInteractable currentInteractable;
        private SphereCollider interactionTrigger;
        private float interactionRefreshTimer;

        private readonly Dictionary<IInteractable, int> nearbyInteractables = new();

        public event Action<string> InteractionTextChanged;


        private void Awake()
        {
            movement = GetComponent<MovementManagerScript>();

            pauseMenu = FindFirstObjectByType<PauseMenuScript>();

            playerControls = new PlayerControls();

            interactionTrigger = GetComponent<SphereCollider>();
            interactionTrigger.isTrigger = true;
            interactionTrigger.radius = interactionRange;
        }

        private void OnEnable()
        {
            playerControls.Enable();

            playerControls.Player.Move.performed += OnMovePerformed;
            playerControls.Player.Move.canceled += OnMoveCanceled;
            playerControls.Player.Jump.performed += OnJumpPerformed;
            playerControls.Player.Sprint.performed += OnSprintPerformed;
            playerControls.Player.Sprint.canceled += OnSprintCanceled;
            playerControls.Player.Dash.performed += OnDashPerformed;
            playerControls.Player.Interact.performed += OnInteractPerformed;
            playerControls.Player.Pause.performed += OnPausePerformed;


            Time.timeScale = 1f;
        }
        private void OnDisable()
        {


            playerControls.Player.Move.performed -= OnMovePerformed;
            playerControls.Player.Move.canceled -= OnMoveCanceled;
            playerControls.Player.Jump.performed -= OnJumpPerformed;
            playerControls.Player.Sprint.performed -= OnSprintPerformed;
            playerControls.Player.Sprint.canceled -= OnSprintCanceled;
            playerControls.Player.Dash.performed -= OnDashPerformed;
            playerControls.Player.Interact.performed -= OnInteractPerformed;
            playerControls.Player.Pause.performed -= OnPausePerformed;
           

            playerControls.Disable();
        }

        private void Update()
        {
            interactionRefreshTimer -= Time.deltaTime;
            if (interactionRefreshTimer > 0f)
            {
                return;
            }
            interactionRefreshTimer = interactionRefreshInteraval;
            RefreshCurrentInteractable();
        }

        private void OnTriggerEnter(Collider other)
        {
            RegisterInteractable(other);
            RefreshCurrentInteractable();
        }
        private void OnTriggerExit(Collider other)
        {
            UnregisterInteractable(other);
            RefreshCurrentInteractable();
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            movement.SetMoveInput(ctx.ReadValue<Vector2>());
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            movement.SetMoveInput(Vector2.zero);
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            movement.Jump();
        }

        private void OnSprintPerformed(InputAction.CallbackContext ctx)
        {
            movement.StartSprinting();
        }

        private void OnSprintCanceled(InputAction.CallbackContext ctx)
        {
            movement.StopSprinting();
        }

        private void OnDashPerformed(InputAction.CallbackContext ctx)
        {
            movement.StartDash();
        }

        private void OnInteractPerformed(InputAction.CallbackContext ctx)
        {
            currentInteractable?.Interact();
        }

        private void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            if (pauseMenu == null)
            {
                return;
            }

            if (PauseMenuScript.gameIsPaused)
            {
                pauseMenu.ResumeGame();
            }
            else
            {
                pauseMenu.PauseGame();
            }
        }

        public IInteractable GetInteractableObject()
        {
            return currentInteractable;
        }

        public string GetCurrentInteractionText()
        {
            return currentInteractable == null ? string.Empty : currentInteractable.GetInteractionText();
        }

        private bool IsValidInteractionCollider(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.gameObject == gameObject)
            {
                return false;
            }

            return (interactableLayers.value & (1 << other.gameObject.layer)) != 0;
        }
        private void RegisterInteractable(Collider other)
        {
            if (!IsValidInteractionCollider(other))
            {
                return;
            }

            IInteractable interactable = other.GetComponent(typeof(IInteractable)) as IInteractable;
            if (interactable == null)
            {
                return;
            }

            if (!nearbyInteractables.TryAdd(interactable, 1))
            {
                nearbyInteractables[interactable] += 1;
            }
        }

        private void UnregisterInteractable(Collider other)
        {
            if (!IsValidInteractionCollider(other))
            {
                return;
            }

            IInteractable interactable = other.GetComponent(typeof(IInteractable)) as IInteractable;
            if (interactable == null || !nearbyInteractables.TryGetValue(interactable, out int count))
            {
                return;
            }

            if (count <= 1)
            {
                nearbyInteractables.Remove(interactable);
            }
            else
            {
                nearbyInteractables[interactable] = count - 1;
            }
        }

        private void RefreshCurrentInteractable()
        {
            IInteractable previousInteractable = currentInteractable;
            currentInteractable = FindClosestInteractable();

            if (!ReferenceEquals(previousInteractable, currentInteractable))
            {
                InteractionTextChanged?.Invoke(GetCurrentInteractionText());
            }
        }
        private IInteractable FindClosestInteractable()
        {
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (IInteractable interactable in nearbyInteractables.Keys)
            {
                if (interactable is not Component interactableComponent || interactableComponent == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, interactableComponent.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
            return closestInteractable;
        }



        }
}

