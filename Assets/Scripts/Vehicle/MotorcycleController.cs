using System;
using UnityEngine;

namespace KnifeWheel.Vehicle
{
    /// <summary>
    /// Applies <see cref="MotorcycleDriveModel"/> outputs to a Rigidbody each physics step.
    /// Input reading is injected so PlayMode tests can drive with scripted controls.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MotorcycleController : MonoBehaviour
    {
        [SerializeField] private MotorcycleDriveSettings settings = new MotorcycleDriveSettings();
        [SerializeField] private bool useLegacyInput = true;

        private Rigidbody _rigidbody;
        private IVehicleInputSource _inputSource;
        private bool _initialized;

        public MotorcycleDriveSettings Settings => settings;
        public Rigidbody Body => _rigidbody;
        public IVehicleInputSource InputSource => _inputSource;
        public bool IsReady => _initialized && _rigidbody != null;

        public void SetInputSource(IVehicleInputSource inputSource)
        {
            _inputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
        }

        public void SetSettings(MotorcycleDriveSettings driveSettings)
        {
            settings = driveSettings ?? throw new ArgumentNullException(nameof(driveSettings));
            settings.ValidateOrThrow();
        }

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                enabled = false;
                throw new InvalidOperationException(
                    $"{nameof(MotorcycleController)} on '{name}' requires a Rigidbody.");
            }

            settings ??= new MotorcycleDriveSettings();
            settings.ValidateOrThrow();

            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.constraints = RigidbodyConstraints.None;

            if (_inputSource == null && useLegacyInput)
                _inputSource = new LegacyVehicleInputSource();

            _initialized = true;
        }

        private void FixedUpdate()
        {
            if (!IsReady)
                return;

            if (_inputSource == null)
            {
                Debug.LogError(
                    $"{nameof(MotorcycleController)} on '{name}' has no {nameof(IVehicleInputSource)}.",
                    this);
                enabled = false;
                return;
            }

            ApplyControl(_inputSource.ReadInput(), Time.fixedDeltaTime);
        }

        /// <summary>
        /// Applies one physics step of control. Exposed for PlayMode tests.
        /// </summary>
        public void ApplyControl(in VehicleControlInput input, float deltaTime)
        {
            if (!IsReady)
                throw new InvalidOperationException("Controller is not initialized.");
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime));

            Vector3 forward = transform.forward;
            float forwardSpeed = MotorcycleDriveModel.GetForwardSpeed(_rigidbody.velocity, forward);

            Vector3 driveForce = MotorcycleDriveModel.ComputeDriveForce(
                forward,
                forwardSpeed,
                _rigidbody.mass,
                in input,
                settings);
            _rigidbody.AddForce(driveForce, ForceMode.Force);

            float yawAccel = MotorcycleDriveModel.ComputeYawAcceleration(forwardSpeed, in input, settings);
            // Acceleration mode keeps arcade steer predictable regardless of inertia tensor.
            _rigidbody.AddTorque(transform.up * yawAccel, ForceMode.Acceleration);

            // Soft-clamp yaw rate after applying torque intent.
            Vector3 localAngular = transform.InverseTransformDirection(_rigidbody.angularVelocity);
            localAngular.y = MotorcycleDriveModel.ClampYawRate(localAngular.y, settings);
            _rigidbody.angularVelocity = transform.TransformDirection(localAngular);

            Vector3 stability = MotorcycleDriveModel.ComputeStabilityTorque(transform.up, settings);
            _rigidbody.AddTorque(stability, ForceMode.Acceleration);

            if (settings.AngularDampingAssist > 0f)
            {
                Vector3 damp = -_rigidbody.angularVelocity * settings.AngularDampingAssist;
                // Keep yaw mostly from steering; damp roll/pitch more aggressively.
                damp = Vector3.Scale(damp, new Vector3(1f, 0.25f, 1f));
                _rigidbody.AddTorque(damp, ForceMode.Acceleration);
            }
        }
    }
}
