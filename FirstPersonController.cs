using System;
using UnityEngine;
using Zenject;

namespace Assets.KVLC
{
    [System.Serializable]
    public class PlayerMovement
    {
        public float RunSpeed = 3f;
        public float WalkSpeed = 1.5f;
        public float SprintSpeed = 6f;
        public float MouseSensitivity = 1.5f;
    }

    [System.Serializable]
    public class GravityAndGroundSettings
    {
        public LayerMask LayerMask;
        public float Gravity = -9.81f;
        public float GroundRadius = 0.22f;
        public float GroundOffset = -0.2f;

        public bool IsGrounded;
    }

    [System.Serializable]
    public class PlayerFollowCameraSettings
    {
        public GameObject Target;

        [Range(-360f, 360)] public float CameraBottomClamp = -60f;
        [Range(-360f, 360)] public float CameraTopClamp = 70f;
    }

    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _movement;
        [SerializeField] private GravityAndGroundSettings _gravityAndGroundSettings;
        [SerializeField] private PlayerFollowCameraSettings _cameraSettings;

        [SerializeField][Inject] private Player _player;

        private float _targetSpeed;
        private float _verticalVelocity;
        private float _cinemachineTargetPitch;
        private float _rotationVelocity;
        private float _speed;

        private const float _threshold = 0.01f;

        public PlayerMovement PlayerMovement => _movement;
        public PlayerFollowCameraSettings CameraSettings => _cameraSettings;

        private void Update()
        {
            Movement();
            Gravity();
            GroundedCheck();
        }
        private void LateUpdate()
        {
            CameraRotation();
        }

        private void Movement()
        {
            _targetSpeed = SetTargetSpeed();

            Vector3 inputDirection = new Vector3(_player.Input.Move.x, 0.0f, _player.Input.Move.y).normalized;

            if (_player.Input.Move != Vector2.zero)
                inputDirection = transform.right * _player.Input.Move.x + transform.forward * _player.Input.Move.y;

            _player.CharacterController.Move(inputDirection.normalized * (_targetSpeed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void CameraRotation()
        {
            if (_player.Input.Look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = 1f;

                _cinemachineTargetPitch += _player.Input.Look.y * _movement.MouseSensitivity * deltaTimeMultiplier;
                _rotationVelocity = _player.Input.Look.x * _movement.MouseSensitivity * deltaTimeMultiplier;

                _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, _cameraSettings.CameraBottomClamp,
                    _cameraSettings.CameraTopClamp);

                _cameraSettings.Target.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void GroundedCheck()
        {
            bool isGrounded = _gravityAndGroundSettings.IsGrounded;

            float offset = _gravityAndGroundSettings.GroundOffset;
            float radius = _gravityAndGroundSettings.GroundRadius;

            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - offset, transform.position.z);

            _gravityAndGroundSettings.IsGrounded = Physics.CheckSphere(spherePosition, radius, _gravityAndGroundSettings.LayerMask);
        }

        private void Gravity()
        {
            if (_gravityAndGroundSettings.IsGrounded && _verticalVelocity < 0.0f)
                _verticalVelocity = -2f;

            _verticalVelocity += _gravityAndGroundSettings.Gravity * Time.deltaTime;
        }

        private float SetTargetSpeed()
        {
            if (_player.Input.Move != Vector2.zero)
            {
                if (_player.Input.Sprint)
                    return _movement.SprintSpeed;
                else if (_player.Input.Walk)
                    return _movement.WalkSpeed;

                return _movement.RunSpeed;
            }

            return 0.0f;
        }

        //private void OnDrawGizmos()
        //{
        //    float offset = _gravityAndGroundSettings.GroundOffset;
        //    float radius = _gravityAndGroundSettings.GroundRadius;

        //    Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - offset, transform.position.z);
        //    Gizmos.color = _gravityAndGroundSettings.IsGrounded ? Color.green : Color.red;

        //    Gizmos.DrawWireSphere(spherePosition, radius);
        //}
    }
}
