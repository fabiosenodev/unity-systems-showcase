
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;


namespace FabioSenoDev
{

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StaminaScript))]
    public class MovementManagerScript : MonoBehaviour
    {
        [Header("movement system")]
        [SerializeField] private float walkSpeed = 10f;
        [SerializeField] private float sprintspeed = 20f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashSpeed = 35f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("stamina coast")]
        [SerializeField] private float sprintStaminaPerSecond = 5f;
        [SerializeField] private float dashStaminaCost = 15f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private StaminaScript stamina;

        private Vector2 moveInput;
        private Vector3 verticalVelocity;
        private Vector3 dashDirection;

        private float targetRotation;
        private float rotationVelocity;
        private float dashTimeLeft;

        private bool isGrounded;
        private bool isSprinting;
        private bool isDashing;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stamina = GetComponent<StaminaScript>();

            if (cameraTransform == null && Camera.main !=  null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
           if (cameraTransform == null)
            {
                return;     
            }
           
           isGrounded = controller.isGrounded;
            if (isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            HandleHorizontalMovement();
            HandleSprintStamina();

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }
        private void HandleHorizontalMovement ()
        {
            Vector3 moveDirection = Vector3.zero;

            if(moveInput.sqrMagnitude >= 0.01f)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;

                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDirection = (camRight * moveInput.x + camForward * moveInput.y).normalized;
                dashDirection = moveDirection;

                targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                float smooothRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smooothRotation, 0f);
            }

            float currentSpeed = isSprinting ? sprintspeed : walkSpeed;

            if (isDashing)
            {
                controller.Move(dashDirection * dashSpeed * Time.deltaTime);
                dashTimeLeft -= Time.deltaTime;

                if (dashTimeLeft < 0f)
                {
                    isDashing = false;  
                }

                return;
            }

            controller.Move(moveDirection * currentSpeed *Time.deltaTime);
        }
        private void HandleSprintStamina()
        {
            if (!isSprinting || moveInput == Vector2.zero)
            {
                return;
            }
            bool hasStamina = stamina.UseStamina(sprintStaminaPerSecond * Time.deltaTime);
            if (!hasStamina)
            {
                StopSprinting();
            }
        }

        public void SetMoveInput (Vector2 input)
        {
            moveInput = input;
        }

        public void Jump()
        {
            if (!isGrounded)
            {
                return;
            }
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        public void StartDash()
        {
            if (isDashing || moveInput == Vector2.zero)
            { 
                return;
            }

            if (!stamina.UseStamina(dashStaminaCost))
            {
                return ;
            }

            isDashing = true;
            dashTimeLeft = dashDuration;
        }

        public void StartSprinting()
        {
            isSprinting = true;
        }

        public void StopSprinting()
        {
            isSprinting = false;
        }
    }
}
