using System.Collections;
using KnifeWheel.Prototype;
using KnifeWheel.Vehicle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KnifeWheel.Tests.PlayMode
{
    public class MotorcycleControllerPlayModeTests
    {
        private GameObject _bootstrapGo;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _bootstrapGo = new GameObject("TestBootstrap");
            _bootstrapGo.AddComponent<MotorcyclePrototypeBootstrap>();
            yield return new WaitForFixedUpdate();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            DestroyIfFound(MotorcyclePrototypeBootstrap.MotorcycleName);
            DestroyIfFound(MotorcyclePrototypeBootstrap.GroundName);
            DestroyIfFound(MotorcyclePrototypeBootstrap.CameraName);
            DestroyIfFound(MotorcyclePrototypeBootstrap.LightName);

            if (_bootstrapGo != null)
                Object.Destroy(_bootstrapGo);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Throttle_IncreasesForwardVelocity()
        {
            var controller = FindController();
            var scripted = new ScriptedVehicleInputSource
            {
                Current = new VehicleControlInput(1f, 0f, 0f)
            };
            controller.SetInputSource(scripted);

            float start = MotorcycleDriveModel.GetForwardSpeed(
                controller.Body.velocity,
                controller.transform.forward);

            for (int i = 0; i < 25; i++)
                yield return new WaitForFixedUpdate();

            float end = MotorcycleDriveModel.GetForwardSpeed(
                controller.Body.velocity,
                controller.transform.forward);

            Assert.Greater(end, start + 0.2f, "Expected forward speed to increase under throttle.");
        }

        [UnityTest]
        public IEnumerator Brake_ReducesForwardVelocity()
        {
            var controller = FindController();
            var scripted = new ScriptedVehicleInputSource();
            controller.SetInputSource(scripted);

            scripted.Current = new VehicleControlInput(1f, 0f, 0f);
            for (int i = 0; i < 30; i++)
                yield return new WaitForFixedUpdate();

            float beforeBrake = MotorcycleDriveModel.GetForwardSpeed(
                controller.Body.velocity,
                controller.transform.forward);
            Assert.Greater(beforeBrake, 1f);

            scripted.Current = new VehicleControlInput(0f, 1f, 0f);
            for (int i = 0; i < 30; i++)
                yield return new WaitForFixedUpdate();

            float afterBrake = MotorcycleDriveModel.GetForwardSpeed(
                controller.Body.velocity,
                controller.transform.forward);

            Assert.Less(afterBrake, beforeBrake - 0.2f, "Expected braking to reduce forward speed.");
        }

        [UnityTest]
        public IEnumerator SteerAtSpeed_ChangesYawOrientation()
        {
            var controller = FindController();
            var scripted = new ScriptedVehicleInputSource();
            controller.SetInputSource(scripted);

            scripted.Current = new VehicleControlInput(1f, 0f, 0f);
            for (int i = 0; i < 25; i++)
                yield return new WaitForFixedUpdate();

            float yawBefore = controller.transform.eulerAngles.y;

            scripted.Current = new VehicleControlInput(1f, 0f, 1f);
            for (int i = 0; i < 30; i++)
                yield return new WaitForFixedUpdate();

            float yawAfter = controller.transform.eulerAngles.y;
            float delta = Mathf.DeltaAngle(yawBefore, yawAfter);
            Assert.Greater(Mathf.Abs(delta), 2f, "Expected steering to change yaw at speed.");
        }

        [UnityTest]
        public IEnumerator Bootstrap_CreatesGroundMotorcycleAndCamera()
        {
            Assert.IsNotNull(GameObject.Find(MotorcyclePrototypeBootstrap.GroundName));
            Assert.IsNotNull(GameObject.Find(MotorcyclePrototypeBootstrap.MotorcycleName));
            Assert.IsNotNull(GameObject.Find(MotorcyclePrototypeBootstrap.CameraName));

            var controller = FindController();
            Assert.IsTrue(controller.IsReady);
            Assert.IsNotNull(controller.Body);
            yield return null;
        }

        [Test]
        public void SetInputSource_Null_Throws()
        {
            var controller = FindController();
            Assert.Throws<System.ArgumentNullException>(() => controller.SetInputSource(null));
        }

        private static MotorcycleController FindController()
        {
            var go = GameObject.Find(MotorcyclePrototypeBootstrap.MotorcycleName);
            Assert.IsNotNull(go, "Motorcycle prototype was not spawned.");
            var controller = go.GetComponent<MotorcycleController>();
            Assert.IsNotNull(controller);
            Assert.IsTrue(controller.IsReady);
            return controller;
        }

        private static void DestroyIfFound(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null)
                Object.Destroy(go);
        }
    }
}
