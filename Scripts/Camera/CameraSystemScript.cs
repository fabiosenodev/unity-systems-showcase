using System.ComponentModel.Design.Serialization;
using FabioSenoDev;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CameraSystemScript : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new(0f, 0f, -8f);

    [Header ("Rotation")]
    [SerializeField] private float cameraSensitivityHorizontal = 3f;
    [SerializeField] private float cameraSensitivityVertical = 0.5f;
    [SerializeField] private float idleTimeBeforeReset = 2f;
    [SerializeField] private float resetSpeed = 2f;
    [SerializeField] private float maxLookDown = -30f;
    [SerializeField] private float maxLookUp = 30f;
    [SerializeField] private float gamepadLookMultiplier = 120f;

    [Header("Collision")]
    [SerializeField] private float minimumDistance = 1f;
    [SerializeField] private float smoothCollision = 10f;
    [SerializeField] private LayerMask collisionLayer = ~0;

    private PlayerControls playerControls;
    private Vector2 lookInput;


    private float horizontalRotation;
    private float verticalRotation;
    private float lastInputTime;
    private float currentDistance;


    private void Start()
    {
        currentDistance = offset.magnitude;
        if(target != null )
        {
            horizontalRotation  = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (PauseMenuScript.gameIsPaused || target == null)
        {
            return;
        }

        Vector2 lookInput = ReadLookInput();

        if (lookInput.sqrMagnitude > 0.0001f)
        {
            lastInputTime = Time.time;

            horizontalRotation += lookInput.x * cameraSensitivityHorizontal;
            verticalRotation -= lookInput.y * cameraSensitivityVertical;
            verticalRotation = Mathf.Clamp(verticalRotation, maxLookDown, maxLookUp);
        }
        else if (Time.time - lastInputTime > idleTimeBeforeReset)
        {
            float targetHorizontal = target.eulerAngles.y;
            horizontalRotation = Mathf.LerpAngle(horizontalRotation, targetHorizontal, Time.deltaTime * resetSpeed);
        }

        Quaternion rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        Vector3 origin = target.position + Vector3.up;
        Vector3 desiredPosition = target.position + rotation * offset;
        Vector3 direction = ( desiredPosition - origin).normalized;

        float targetDistance = offset.magnitude;
        if (Physics.Raycast(origin, direction, out RaycastHit hit, offset.magnitude, collisionLayer))
        {
           targetDistance = Mathf.Clamp(hit.distance, minimumDistance, offset.magnitude);
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothCollision);
        transform.position = origin + direction * currentDistance;
        transform.LookAt(origin);

        
        }
private Vector2 ReadLookInput()
        {
            Vector2 look = Vector2.zero;

            if (Mouse.current != null)
            {
                look += Mouse.current.delta.ReadValue();
            }

            if (Gamepad.current != null)
            {
                look += Gamepad.current.rightStick.ReadValue() * gamepadLookMultiplier * Time.deltaTime;
            }

            return look;
    }
}
