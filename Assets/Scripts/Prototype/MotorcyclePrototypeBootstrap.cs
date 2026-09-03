using KnifeWheel.CameraRig;
using KnifeWheel.Vehicle;
using UnityEngine;

namespace KnifeWheel.Prototype
{
    /// <summary>
    /// Builds a flat ground, placeholder motorcycle, lighting, and follow camera at runtime.
    /// Keeps the committed scene file minimal and guarantees PlayMode tests can spawn the same setup.
    /// </summary>
    public sealed class MotorcyclePrototypeBootstrap : MonoBehaviour
    {
        public const string GroundName = "PrototypeGround";
        public const string MotorcycleName = "PrototypeMotorcycle";
        public const string CameraName = "PrototypeFollowCamera";
        public const string LightName = "PrototypeSun";

        [SerializeField] private bool buildOnAwake = true;
        [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0.6f, 0f);

        public MotorcycleController Motorcycle { get; private set; }
        public SimpleFollowCamera FollowCamera { get; private set; }

        private void Awake()
        {
            if (buildOnAwake)
                Build();
        }

        public MotorcycleController Build()
        {
            EnsureLighting();
            EnsureGround();
            Motorcycle = EnsureMotorcycle(spawnPosition);
            FollowCamera = EnsureCamera(Motorcycle.transform);
            return Motorcycle;
        }

        public static GameObject EnsureGround()
        {
            var existing = GameObject.Find(GroundName);
            if (existing != null)
                return existing;

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = GroundName;
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(80f, 1f, 80f);
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = new Color(0.35f, 0.38f, 0.32f)
                };
            return ground;
        }

        public static MotorcycleController EnsureMotorcycle(Vector3 position)
        {
            var existing = GameObject.Find(MotorcycleName);
            if (existing != null)
            {
                var existingController = existing.GetComponent<MotorcycleController>();
                if (existingController != null)
                    return existingController;
            }

            var root = new GameObject(MotorcycleName);
            root.transform.position = position;
            root.transform.rotation = Quaternion.identity;

            // Body placeholder
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            body.transform.localScale = new Vector3(0.45f, 0.7f, 0.45f);
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Front / rear wheel placeholders (visual only)
            CreateWheelPlaceholder(root.transform, "FrontWheel", new Vector3(0f, -0.15f, 0.75f));
            CreateWheelPlaceholder(root.transform, "RearWheel", new Vector3(0f, -0.15f, -0.75f));

            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.1f, 0f);
            collider.size = new Vector3(0.5f, 0.9f, 1.8f);

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 180f;
            rb.drag = 0.05f;
            rb.angularDrag = 0.5f;
            rb.centerOfMass = new Vector3(0f, -0.2f, 0f);

            var controller = root.AddComponent<MotorcycleController>();
            controller.Initialize();
            return controller;
        }

        public static SimpleFollowCamera EnsureCamera(Transform target)
        {
            Camera main = Camera.main;
            GameObject camGo;
            if (main != null)
            {
                camGo = main.gameObject;
                camGo.name = CameraName;
            }
            else
            {
                var existing = GameObject.Find(CameraName);
                camGo = existing != null ? existing : new GameObject(CameraName);
                if (camGo.GetComponent<Camera>() == null)
                    camGo.AddComponent<Camera>();
                if (camGo.GetComponent<AudioListener>() == null)
                    camGo.AddComponent<AudioListener>();
            }

            var follow = camGo.GetComponent<SimpleFollowCamera>();
            if (follow == null)
                follow = camGo.AddComponent<SimpleFollowCamera>();

            follow.Target = target;
            camGo.transform.position = target.TransformPoint(new Vector3(0f, 3.5f, -8f));
            camGo.transform.LookAt(target.position + Vector3.up * 1.2f);
            return follow;
        }

        public static void EnsureLighting()
        {
            if (GameObject.Find(LightName) != null)
                return;

            var lightGo = new GameObject(LightName);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.5f);
        }

        private static void CreateWheelPlaceholder(Transform parent, string name, Vector3 localPosition)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = localPosition;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);
            Object.DestroyImmediate(wheel.GetComponent<Collider>());
        }
    }
}
