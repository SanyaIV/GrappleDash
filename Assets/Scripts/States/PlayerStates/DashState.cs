using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/States/Dash")]
public class DashState : PlayerState {

    private bool _dashSuccess = false;
    private bool _bounce = false;

    [Header("Dash Settings")]
    public const int DEFAULT_DASH_AMOUNT = 1;
    public const int MAX_DASH_AMOUNT = 3;
    public int CurrentMaxDashAmount;
    public int CurrentDashes;
    public float GroundDashCooldown;
    private float _lastDash;
    private bool _ghostDash;

    [Header("Dash Travel")]
    public float DashSpeed = 5f;
    public float DashExitSpeed;
    public float MaxDashTime = 0.2f;
    private float _dashTimer = 0f;
    private Vector3 _direction;

    [Header("Sounds")]
    private AudioSource source;
    public AudioClip dash;

    public override void Initialize(Controller owner)
    {
        base.Initialize(owner);

        if (GlobalControl.Instance != null)
            CurrentMaxDashAmount = GlobalControl.CurrentDashes;
        else
            CurrentMaxDashAmount = 3;

        ResetDashes();
        source = _controller.GetComponent<AudioSource>();
        source.clip = dash;
    }


    public override void Enter()
    {
        _controller._collision = CollisionFlags.None;
        _direction = _controller.TPCamera.forward;
        _ghostDash = false;
        _bounce = false;
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetButtonDown("Grapple") && _controller.GetState<GrapplingState>().CheckGrappable())
        {
            _controller.TransitionTo<GrapplingState>();
            return;
        }
        if (Input.GetButtonDown("Dash"))
            _ghostDash = true;

        if (!UpdateDash()) return;

        _dashTimer += Time.deltaTime;

        _controller._collision = _charCtrl.Move(_controller.MoveDir * Time.deltaTime);
    }

    public override void Exit()
    {
        _dashTimer = 0f;
        if (_dashSuccess)
        {
            _dashSuccess = false;
            CurrentDashes--;
            _lastDash = Time.time;

            if(!_bounce)
                _controller.MoveDir = _controller.MoveDir.normalized * DashExitSpeed;

            if ((_controller._collision & CollisionFlags.Above) != 0 || (_controller._collision & CollisionFlags.Sides) != 0)
                CameraShake.SetIntensity(0.5f);
        }
        source.Stop();
    }

    private bool UpdateDash()
    {
        if (!_dashSuccess && CurrentDashes > 0 && (Time.time - _lastDash >= GroundDashCooldown || !_controller.IsGrounded()))
        {
            _dashSuccess = true;
            _controller.MoveDir = _direction * DashSpeed;
            PlayDashSound();
        }
        else if ((_controller._collision & CollisionFlags.Below) != 0 && _controller.IsGrounded() && _controller.PreviousState is AirState)
        {
            _controller.TransitionTo<GroundState>();
            return false;
        }
        else if (_ghostDash && CurrentDashes >= 0)
        {
            _controller.TransitionTo<DashState>();
            return false;
        }
        else if (!_dashSuccess || _dashTimer >= MaxDashTime || (Time.time - _lastDash <= GroundDashCooldown && _controller.IsGrounded()))
        {
            StopDash();
            return false;
        }

        return true;
    }

    public void StopDash(bool bounce = false)
    {
        if (_controller.CurrentState is DashState)
        {
            _bounce = bounce;

            if (_controller.IsGrounded())
                _controller.TransitionTo<GroundState>();
            else
                _controller.TransitionTo<AirState>();
        }
    }

    public void ResetDashes()
    {
        CurrentDashes = CurrentMaxDashAmount;
    }

    public void DashUp()
    {
        _controller.HUD.StartCoroutine(_controller.HUD.DashUpgraded());
        CurrentDashes = ++CurrentMaxDashAmount;
        GlobalControl.AddDash();
    }

    public void PlayDashSound() {
         if (!source.isPlaying)
            source.PlayOneShot(dash, 0.3f);
    }
    
    public Vector3 GetDirection()
    {
        if (_dashSuccess)
            return _direction;
        else
            return Vector3.zero;
    }
}
