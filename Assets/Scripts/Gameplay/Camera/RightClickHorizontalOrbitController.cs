using KnockBallsCase.UI.MainMenu;
using UnityEngine;

namespace KnockBallsCase.Gameplay.CameraFx
{
    public sealed class RightClickHorizontalOrbitController : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Transform cannonTarget;

        [Header("Orbit")]
        [SerializeField] private Vector3 orbitCenter = Vector3.zero;
        [SerializeField] private float degreesPerMouseUnit = 180f;
        [SerializeField] private bool invertDirection;

        [Header("Input")]
        [SerializeField] private int orbitMouseButton = 1;

        private void Update()
        {
            if (MainMenuController.IsMenuOpen || MainMenuController.IsGameplayInputBlocked)
            {
                return;
            }

            if (!Input.GetMouseButton(orbitMouseButton))
            {
                return;
            }

            var mouseX = Input.GetAxisRaw("Mouse X");
            if (Mathf.Abs(mouseX) < 0.0001f)
            {
                return;
            }

            var sign = invertDirection ? -1f : 1f;
            var angle = mouseX * degreesPerMouseUnit * sign * Time.deltaTime;
            RotateTargetAroundCenter(cameraTarget, angle);
            RotateTargetAroundCenter(cannonTarget, angle);
        }

        private void RotateTargetAroundCenter(Transform target, float angle)
        {
            if (target == null)
            {
                return;
            }

            target.RotateAround(orbitCenter, Vector3.up, angle);
        }
    }
}
