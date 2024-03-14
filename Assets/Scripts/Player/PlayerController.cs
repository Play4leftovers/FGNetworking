using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInput;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class PlayerController : NetworkBehaviour, IPlayerActions
{
    [SerializeField] private List<Sprite> spriteMovementList;
    [SerializeField] private Sprite nonMovementSprite;
    private int currentSprite;
    private NetworkVariable<bool> isMoving = new NetworkVariable<bool>();

    private PlayerInput _playerInput;
    private Vector2 _moveInput = new();
    private Vector2 _cursorLocation;

    private Transform _shipTransform;
    private Rigidbody2D _rb;
    private Health _health;

    private Transform turretPivotTransform;


    public UnityAction<bool> onFireEvent;
    public UnityAction<bool> onMissileEvent;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float shipRotationSpeed = 100f;
    [SerializeField] private float turretRotationSpeed = 4f;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;

        if (_playerInput == null)
        {
            _playerInput = new();
            _playerInput.Player.SetCallbacks(this);
        }
        _playerInput.Player.Enable();
        _rb = GetComponent<Rigidbody2D>();
        
        isMoving.Value = false;
        
        _health = GetComponent<Health>();
        _health.dead.OnValueChanged += OnDeathStateChange;
        
        _shipTransform = transform;
        turretPivotTransform = transform.Find("PivotTurret");
        if (turretPivotTransform == null) Debug.LogError("PivotTurret is not found", gameObject);
    }

    //The DeathStateChange event listeners function is to ensure that the player can not continue to shoot or move while they are dead.
    //It listens to the dead network bool in Health.cs
    private void OnDeathStateChange(bool previousvalue, bool newvalue)
    {
        if(newvalue) _playerInput.Player.Disable();
        else _playerInput.Player.Enable();
    }

    public void OnFire(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onFireEvent.Invoke(true);
        }
        else if (context.canceled)
        {
            onFireEvent.Invoke(false);
        }
    }
    
    //All this does is add another event listener for the missile button
    public void OnMissile(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onMissileEvent.Invoke(true);
        }
        else if (context.canceled)
        {
            onMissileEvent.Invoke(false);
        }
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!isMoving.Value)
        {
            Debug.Log("Ship Standing Still");
            currentSprite = 0;
            GetComponent<SpriteRenderer>().sprite = nonMovementSprite;
        }
        
        if (isMoving.Value)
        {
            Debug.Log("Ship Moving");
            GetComponent<SpriteRenderer>().sprite = spriteMovementList[currentSprite];
            currentSprite++;
            if (currentSprite == spriteMovementList.Count)
            {
                currentSprite = 0;
            }
        }
        
        if(!IsOwner || _playerInput.Player.enabled == false) return;
        
        _rb.velocity = transform.up * (_moveInput.y * movementSpeed);
        _rb.MoveRotation(_rb.rotation + _moveInput.x * -shipRotationSpeed * Time.fixedDeltaTime);
        
        if (_rb.velocity.magnitude > 0) isMoving.Value = true;
        else isMoving.Value = false;
    }
    private void LateUpdate()
    {
        if(!IsOwner || _playerInput.Player.enabled == false) return;
        Vector2 screenToWorldPosition = Camera.main.ScreenToWorldPoint(_cursorLocation);
        var position = turretPivotTransform.position;
        
        Vector2 targetDirection = new Vector2(screenToWorldPosition.x - position.x, screenToWorldPosition.y - position.y).normalized;
        Vector2 currentDirection = Vector2.Lerp(turretPivotTransform.up, targetDirection, Time.deltaTime * turretRotationSpeed);
        turretPivotTransform.up = currentDirection;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        _cursorLocation = context.ReadValue<Vector2>();
    }

    //This tells the player to move. I placed it here as in the future there may be other reasons you'd want to teleport as well as that the PlayerController should handle movement and player input as much as possible.
    public void TeleportPlayer(Vector3 coordinates)
    {
        GetComponent<NetworkTransform>().Teleport(coordinates, Quaternion.identity, new Vector3(1,1,1));
    }
}