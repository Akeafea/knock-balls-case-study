using System.Collections;
using KnockBallsCase.Core.Pooling;
using KnockBallsCase.Gameplay.Audio;
using KnockBallsCase.Gameplay.CameraFx;
using KnockBallsCase.Gameplay.PowerUp;
using KnockBallsCase.UI.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace KnockBallsCase.Gameplay.Ball
{
    public sealed class BallThrowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObjectPool ballPool;
        [SerializeField] private TMP_Text ammoCountText;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private LayerMask aimMask = ~0;
        [SerializeField] private float noHitAimDistance = 40f;
        [SerializeField] private Transform cannonVisual;
        [SerializeField] private CameraShakeController cameraShakeController;
        [SerializeField] private PowerUpController specialFireModeHandler;

        [Header("Throw")]
        [SerializeField] private float throwForce = 18f;
        [SerializeField] private float fireDelay = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip cannonShootSfx;
        [SerializeField] [Range(0f, 1f)] private float cannonShootSfxVolume = 1f;

        [Header("Cannon Aim")]
        [SerializeField] private float maxYawAngle = 75f;
        [SerializeField] private float maxPitchAngle = 40f;

        [Header("Cannon Recoil")]
        [SerializeField] private float recoilDistance = 0.1f;
        [SerializeField] private float recoilOutDuration = 0.05f;
        [SerializeField] private float recoilReturnDuration = 0.1f;

        [Header("Ammo")]
        [SerializeField] private int startAmmo = 5;
        [SerializeField] private float loseStateDelayAfterAmmoDepleted = 2.5f;
        [SerializeField] private UnityEvent onAmmoDepleted;

        private float _nextFireTime;
        private int _currentAmmo;
        private bool _depletedInvoked;
        private Vector3 _cannonDefaultLocalEuler;
        private Vector3 _cannonDefaultLocalPosition;
        private Coroutine _recoilRoutine;
        private Coroutine _depletedRoutine;
        private ISpecialFireModeHandler _specialFireModeHandler;

        private void Awake()
        {
            _specialFireModeHandler = specialFireModeHandler;

            if (cameraShakeController == null)
            {
                cameraShakeController = FindAnyObjectByType<CameraShakeController>();
            }

            if (cannonVisual == null)
            {
                return;
            }

            _cannonDefaultLocalEuler = cannonVisual.localEulerAngles;
            _cannonDefaultLocalPosition = cannonVisual.localPosition;
        }

        private void OnEnable()
        {
            _specialFireModeHandler = specialFireModeHandler;
            ResetForNewMiniLevel(startAmmo);
        }

        private void OnDisable()
        {
            if (_depletedRoutine != null)
            {
                StopCoroutine(_depletedRoutine);
                _depletedRoutine = null;
            }
        }

        private void Update()
        {
            UpdateCannonAimFromInput();

            // Menü açıkken veya menüden yeni çıkılmışken atış inputunu kilitliyoruz.
            if (MainMenuController.IsMenuOpen || MainMenuController.IsGameplayInputBlocked)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                TryThrow();
            }
        }

        private void UpdateCannonAimFromInput()
        {
            if (cannonVisual == null || shootPoint == null)
            {
                return;
            }

            var shootDirection = GetShootDirection();
            AimCannon(shootDirection);
        }

        private void TryThrow()
        {
            if (ballPool == null || shootPoint == null)
            {
                return;
            }

            if (Time.time < _nextFireTime)
            {
                return;
            }

            if (_specialFireModeHandler != null && _specialFireModeHandler.TryHandlePrimaryFire())
            {
                _nextFireTime = Time.time + fireDelay;
                return;
            }

            if (_currentAmmo <= 0)
            {
                TriggerAmmoDepletedIfNeeded();
                return;
            }

            var shootDirection = GetShootDirection();
            if (TryFireFromDirection(shootDirection, true))
            {
                _nextFireTime = Time.time + fireDelay;
            }
        }

        public Vector3 GetShootDirectionForCurrentInput()
        {
            return GetShootDirection();
        }

        public void ResetForNewMiniLevel(int ammoCount)
        {
            _currentAmmo = Mathf.Max(0, ammoCount);
            _depletedInvoked = false;
            _nextFireTime = 0f;

            if (_depletedRoutine != null)
            {
                StopCoroutine(_depletedRoutine);
                _depletedRoutine = null;
            }

            RefreshUI();
        }

        public bool TryFireFromDirection(Vector3 shootDirection, bool consumeAmmo)
        {
            if (ballPool == null || shootPoint == null)
            {
                return false;
            }

            if (consumeAmmo && _currentAmmo <= 0)
            {
                TriggerAmmoDepletedIfNeeded();
                return false;
            }

            var ball = ballPool.Rent();
            if (ball == null)
            {
                return false;
            }

            AimCannon(shootDirection);
            var launchPosition = shootPoint.position;
            var launchRotation = Quaternion.LookRotation(shootDirection);

            var rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.position = launchPosition;
                rb.rotation = launchRotation;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = shootDirection * throwForce;
            }
            else
            {
                ball.transform.SetPositionAndRotation(launchPosition, launchRotation);
            }

            PlayCannonRecoil();
            if (cameraShakeController != null)
            {
                cameraShakeController.PlayCannonFireShake();
            }

            GlobalSfx2D.Play(cannonShootSfx, cannonShootSfxVolume);

            if (consumeAmmo)
            {
                _currentAmmo = Mathf.Max(0, _currentAmmo - 1);
                if (_currentAmmo <= 0)
                {
                    TriggerAmmoDepletedIfNeeded();
                }

                RefreshUI();
            }

            return true;
        }

        private void AimCannon(Vector3 shootDirection)
        {
            if (cannonVisual == null)
            {
                return;
            }

            var basis = cannonVisual.parent != null ? cannonVisual.parent : transform;
            var localDirection = basis.InverseTransformDirection(shootDirection).normalized;

            var yaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            var horizontal = Mathf.Sqrt((localDirection.x * localDirection.x) + (localDirection.z * localDirection.z));
            var pitch = -Mathf.Atan2(localDirection.y, horizontal) * Mathf.Rad2Deg;

            yaw = Mathf.Clamp(yaw, -maxYawAngle, maxYawAngle);
            pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);

            var aimedEuler = new Vector3(
                _cannonDefaultLocalEuler.x + pitch,
                _cannonDefaultLocalEuler.y + yaw,
                _cannonDefaultLocalEuler.z - pitch);

            cannonVisual.localEulerAngles = aimedEuler;
        }

        private void PlayCannonRecoil()
        {
            if (cannonVisual == null)
            {
                return;
            }

            if (_recoilRoutine != null)
            {
                StopCoroutine(_recoilRoutine);
            }

            _recoilRoutine = StartCoroutine(CannonRecoilRoutine());
        }

        private IEnumerator CannonRecoilRoutine()
        {
            var start = _cannonDefaultLocalPosition;
            var recoilTarget = _cannonDefaultLocalPosition + (Vector3.back * recoilDistance);

            var outElapsed = 0f;
            while (outElapsed < recoilOutDuration)
            {
                outElapsed += Time.deltaTime;
                var t = recoilOutDuration <= 0f ? 1f : Mathf.Clamp01(outElapsed / recoilOutDuration);
                cannonVisual.localPosition = Vector3.Lerp(start, recoilTarget, t);
                yield return null;
            }

            var backElapsed = 0f;
            while (backElapsed < recoilReturnDuration)
            {
                backElapsed += Time.deltaTime;
                var t = recoilReturnDuration <= 0f ? 1f : Mathf.Clamp01(backElapsed / recoilReturnDuration);
                cannonVisual.localPosition = Vector3.Lerp(recoilTarget, _cannonDefaultLocalPosition, t);
                yield return null;
            }

            cannonVisual.localPosition = _cannonDefaultLocalPosition;
            _recoilRoutine = null;
        }

        private Vector3 GetShootDirection()
        {
            var cam = aimCamera != null ? aimCamera : Camera.main;
            if (cam == null)
            {
                return shootPoint.forward;
            }

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPoint;
            if (Physics.Raycast(ray, out var hit, 500f, aimMask))
            {
                targetPoint = hit.point;
            }
            else
            {
                // If the ray does not hit anything (common when aiming upward), keep mouse direction using a far point on the ray.
                targetPoint = ray.GetPoint(noHitAimDistance);
            }

            var direction = (targetPoint - shootPoint.position).normalized;
            return direction.sqrMagnitude > 0.0001f ? direction : shootPoint.forward;
        }

        private void TriggerAmmoDepletedIfNeeded()
        {
            if (_depletedInvoked)
            {
                return;
            }

            _depletedInvoked = true;

            if (_depletedRoutine != null)
            {
                StopCoroutine(_depletedRoutine);
            }

            _depletedRoutine = StartCoroutine(InvokeAmmoDepletedAfterDelay());
        }

        private IEnumerator InvokeAmmoDepletedAfterDelay()
        {
            var delay = Mathf.Max(0f, loseStateDelayAfterAmmoDepleted);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            onAmmoDepleted?.Invoke();
            _depletedRoutine = null;
        }

        private void RefreshUI()
        {
            if (ammoCountText != null)
            {
                ammoCountText.text = _currentAmmo.ToString();
            }
        }
    }
}
