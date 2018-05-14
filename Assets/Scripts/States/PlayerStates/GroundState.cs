using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/States/Ground")]
public class GroundState : PlayerState {

    [Header("Movement")]
    public float Acceleration = 100f;
    public float ExtraFriction = 30f;
    public float ExtraSecondaryFriction = 30f;
    public float StopSlidingLimit = 1.5f;
    [HideInInspector] public bool InputGotten = false;

    [Header("Ground")]
    public float StickToGroundForce = 10f;

    [Header("Jumping")]
    public MinMaxFloat JumpHeight;
    public float TimeToJumpApex = 0.5f;
    public float InitialJumpDistance = 0.15f;
    public float MaxGhostAirJumpTime = 0.2f;
    [HideInInspector] public MinMaxFloat JumpVelocity;
    [HideInInspector] public float TimeOfAirJump;

    [Header("Temporary Variables")]
    private RaycastHit hit;

    private float Friction { get { return Acceleration / _controller.MaxSpeed; } }

    public override void Initialize(Controller owner)
    {
        base.Initialize(owner);

        _controller.Gravity = (2 * JumpHeight.Max) / Mathf.Pow(TimeToJumpApex, 2);
        JumpVelocity.Max = _controller.Gravity * TimeToJumpApex;
        JumpVelocity.Min = Mathf.Sqrt(2 * _controller.Gravity * JumpHeight.Min);
        TimeOfAirJump = -100f;
    }

    public override void Enter()
    {
        InputGotten = false;

        //Reset amount of dashes
        if (_controller.PreviousState is AirState && Time.time - TimeOfAirJump <= MaxGhostAirJumpTime)
            UpdateJump();

        _controller.GetState<DashState>().ResetDashes();
    }

    public override void Exit()
    {
        transform.parent = null;
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetButtonDown("Jump"))
            UpdateJump();

        if (Input.GetButtonDown("Dash"))
            _controller.TransitionTo<DashState>();

        if (Input.GetButtonDown("Grapple"))
            _controller.TransitionTo<GrapplingState>();

        /*hit = _controller.GroundCheck();

        UpdateMovement();
        UpdateFriction();

        if (hit.collider != null)
            _controller.MoveDir.y = -StickToGroundForce;
        else
            _controller.TransitionTo<AirState>();

        StopSliding();*/

        _controller._collision = _charCtrl.Move(MoveDir * Time.deltaTime);

        /*if (hit.collider != null)
            _controller.MoveDir.y = 0f;*/
    }

    public override void FixedUpdate()
    {
        hit = _controller.GroundCheck();

        UpdateMovement();
        UpdateFriction();

        if (hit.collider != null)
            _controller.MoveDir.y = -StickToGroundForce;
        else
            _controller.TransitionTo<AirState>();

        StopSliding();
    }

    private void UpdateMovement()
    {
        Vector3 input = _controller.Input;
        input = transform.forward * input.z + transform.right * input.x;

        if (hit.collider != null && hit.collider.CompareTag("Platform"))
        {
            hit.collider.GetComponent<Platform>().Activate();
            transform.parent = hit.collider.gameObject.transform.parent;
        }
        else
        {
            transform.parent = null;
        }

        input = Vector3.ProjectOnPlane(input, hit.normal).normalized;

        MoveDir += input * Acceleration;

        if(input.magnitude > 0)
        {
            InputGotten = true;

            if (MoveDir.magnitude > _controller.MaxSpeed)
                MoveDir = MoveDir.normalized * _controller.MaxSpeed;
        }
    }

    private void UpdateFriction()
    {
        float extraFriction = InputGotten ? _controller.Input.magnitude < _controller.InputRequiredToMove ? ExtraFriction : 0.0f : ExtraSecondaryFriction; //_controller.Input.magnitude < _controller.InputRequiredToMove ? ExtraFriction : 0.0f;
        float friction = Mathf.Clamp01((Friction + extraFriction) * Time.fixedDeltaTime);
        MoveDir -= MoveDir * friction;
    }

    private void StopSliding()
    {
        if (MoveDir.magnitude < StopSlidingLimit && _controller.Input.magnitude < _controller.InputRequiredToMove)
            MoveDir = Vector3.zero;
    }

    public void UpdateJump()
    {
        transform.position += Vector3.up * InitialJumpDistance;
        _controller.MoveDir.y = JumpVelocity.Max;
        _controller.GetState<AirState>().GhostJumpAllowed = false;
        _controller.GetState<AirState>().CanCancelJump = true;
        if(!(_controller.CurrentState is AirState))_controller.TransitionTo<AirState>();
    }
}
