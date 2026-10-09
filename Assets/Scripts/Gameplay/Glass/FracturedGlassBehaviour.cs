using System.Collections;
using KnockBallsCase.Core.Pooling;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Glass
{
    [RequireComponent(typeof(PooledInstance))]
    public sealed class FracturedGlassBehaviour : MonoBehaviour, IPoolable
    {
        public static int ActiveFractureCount { get; private set; }

        [Header("Scatter")]
        [SerializeField] private float scatterForce = 3.5f;
        [SerializeField] private float inheritedImpactFactor = 0.35f;
        [SerializeField] private float randomSpread = 0.2f;
        [SerializeField] private float randomTorque = 2f;

        [Header("Cleanup")]
        [SerializeField] private float shrinkDelay = 0.8f;
        [SerializeField] private float shrinkDuration = 0.3f;
        [SerializeField] private float pieceShrinkStagger = 0.04f;

        private Rigidbody[] _pieceBodies;
        private Transform[] _pieceTransforms;
        private Vector3[] _initialLocalPositions;
        private Quaternion[] _initialLocalRotations;
        private Vector3[] _initialLocalScales;

        private PooledInstance _pooledInstance;
        private Coroutine _lifecycleRoutine;
        private bool _isFractureActive;

        private void Awake()
        {
            _pooledInstance = GetComponent<PooledInstance>();

            _pieceBodies = GetComponentsInChildren<Rigidbody>(true);
            _pieceTransforms = new Transform[_pieceBodies.Length];
            _initialLocalPositions = new Vector3[_pieceBodies.Length];
            _initialLocalRotations = new Quaternion[_pieceBodies.Length];
            _initialLocalScales = new Vector3[_pieceBodies.Length];

            for (var i = 0; i < _pieceBodies.Length; i++)
            {
                var pieceTransform = _pieceBodies[i].transform;
                _pieceTransforms[i] = pieceTransform;
                _initialLocalPositions[i] = pieceTransform.localPosition;
                _initialLocalRotations[i] = pieceTransform.localRotation;
                _initialLocalScales[i] = pieceTransform.localScale;
            }
        }

        public void OnSpawned()
        {
            ResetPieces();
        }

        public void OnDespawned()
        {
            if (_lifecycleRoutine != null)
            {
                StopCoroutine(_lifecycleRoutine);
                _lifecycleRoutine = null;
            }

            EndFractureLifecycle();

            ResetPieces();
        }

        public void StartFracture(Vector3 impactPoint, Vector3 impactVelocity)
        {
            if (_lifecycleRoutine != null)
            {
                StopCoroutine(_lifecycleRoutine);
            }

            BeginFractureLifecycle();

            ScatterPieces(impactPoint, impactVelocity);
            _lifecycleRoutine = StartCoroutine(ShrinkAndReturnRoutine());
        }

        private void ScatterPieces(Vector3 impactPoint, Vector3 impactVelocity)
        {
            var inherited = impactVelocity.normalized * inheritedImpactFactor;

            for (var i = 0; i < _pieceBodies.Length; i++)
            {
                var body = _pieceBodies[i];
                if (body == null)
                {
                    continue;
                }

                var piecePosition = body.worldCenterOfMass;
                var fromImpact = (piecePosition - impactPoint).normalized;
                if (fromImpact.sqrMagnitude < 0.001f)
                {
                    fromImpact = Random.onUnitSphere;
                }

                var random = Random.insideUnitSphere * randomSpread;
                var forceDir = (fromImpact + inherited + random).normalized;

                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.AddForce(forceDir * scatterForce, ForceMode.Impulse);
                body.AddTorque(Random.onUnitSphere * randomTorque, ForceMode.Impulse);
            }
        }

        private IEnumerator ShrinkAndReturnRoutine()
        {
            if (shrinkDelay > 0f)
            {
                yield return new WaitForSeconds(shrinkDelay);
            }

            var pieceCount = _pieceTransforms.Length;
            if (pieceCount == 0)
            {
                ReturnToPool();
                yield break;
            }

            var startTimes = new float[pieceCount];
            for (var i = 0; i < pieceCount; i++)
            {
                startTimes[i] = i * Mathf.Max(0f, pieceShrinkStagger);
            }

            var totalDuration = startTimes[pieceCount - 1] + Mathf.Max(0.01f, shrinkDuration);
            var timer = 0f;

            while (timer < totalDuration)
            {
                timer += Time.deltaTime;

                for (var i = 0; i < pieceCount; i++)
                {
                    var piece = _pieceTransforms[i];
                    if (piece == null)
                    {
                        continue;
                    }

                    var localTime = timer - startTimes[i];
                    if (localTime <= 0f)
                    {
                        continue;
                    }

                    var t = Mathf.Clamp01(localTime / Mathf.Max(0.01f, shrinkDuration));
                    piece.localScale = Vector3.Lerp(_initialLocalScales[i], Vector3.zero, t);
                }

                yield return null;
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _lifecycleRoutine = null;

            if (_pooledInstance != null)
            {
                _pooledInstance.ReturnToPool();
            }
            else
            {
                EndFractureLifecycle();
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            EndFractureLifecycle();
        }

        private void BeginFractureLifecycle()
        {
            if (_isFractureActive)
            {
                return;
            }

            _isFractureActive = true;
            ActiveFractureCount++;
        }

        private void EndFractureLifecycle()
        {
            if (!_isFractureActive)
            {
                return;
            }

            _isFractureActive = false;
            ActiveFractureCount = Mathf.Max(0, ActiveFractureCount - 1);
        }

        private void ResetPieces()
        {
            for (var i = 0; i < _pieceBodies.Length; i++)
            {
                var body = _pieceBodies[i];
                var pieceTransform = _pieceTransforms[i];
                if (pieceTransform == null)
                {
                    continue;
                }

                pieceTransform.localPosition = _initialLocalPositions[i];
                pieceTransform.localRotation = _initialLocalRotations[i];
                pieceTransform.localScale = _initialLocalScales[i];

                if (body == null)
                {
                    continue;
                }

                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }
}
