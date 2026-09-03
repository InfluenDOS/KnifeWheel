using UnityEngine;

namespace KnifeWheel.CameraRig
{
    /// <summary>
    /// Minimal third-person follow camera for prototype driving tests.
    /// </summary>
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -8f);
        [SerializeField] private float followLerp = 8f;
        [SerializeField] private float lookAtHeight = 1.2f;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desired = target.TransformPoint(offset);
            float t = 1f - Mathf.Exp(-followLerp * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);

            Vector3 lookPoint = target.position + Vector3.up * lookAtHeight;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookPoint - transform.position, Vector3.up),
                t);
        }
    }
}
