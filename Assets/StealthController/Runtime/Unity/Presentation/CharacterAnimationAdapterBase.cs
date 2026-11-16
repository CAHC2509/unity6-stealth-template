using StealthController.Core;
using UnityEngine;

namespace StealthController.Unity
{
    [RequireComponent(typeof(Animator))]
    public abstract class CharacterAnimationAdapterBase : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash(StealthUnityConstants.ANIMATOR_SPEED_PARAMETER);

        [Header("Blending")]
        [SerializeField, Min(0f)] private float crossFadeDuration = StealthUnityConstants.ANIMATION_CROSSFADE_DURATION;
        [SerializeField, Min(0f)] private float parameterDampTime = StealthUnityConstants.ANIMATION_PARAMETER_DAMP_TIME;

        private Animator animator;

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false;
        }

        protected void SetSpeed(float speed, float deltaTime)
        {
            SetDamped(SpeedId, speed, deltaTime);
        }

        protected void SetDamped(int parameterId, float value, float deltaTime)
        {
            animator.SetFloat(parameterId, value, parameterDampTime, deltaTime);
        }

        protected void SetImmediate(int parameterId, float value)
        {
            animator.SetFloat(parameterId, value);
        }

        protected void ResetSpeed()
        {
            SetImmediate(SpeedId, 0f);
        }

        protected void CrossFadeTo(int stateHash)
        {
            animator.CrossFadeInFixedTime(stateHash, crossFadeDuration);
        }

        protected void PlayImmediately(int stateHash)
        {
            animator.Play(stateHash, 0, 0f);
        }
    }
}
