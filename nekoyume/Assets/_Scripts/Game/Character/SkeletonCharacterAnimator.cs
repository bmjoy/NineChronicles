using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Nekoyume.Game.Character
{
    public class SkeletonCharacterAnimator : MecanimCharacterAnimator
    {
        private const string StringHUD = "HUD";
        private const float ColorTweenFrom = 0f;
        private const float ColorTweenTo = 0.6f;
        private const float ColorTweenDuration = 0.1f;

        private MeshRenderer MeshRenderer { get; set; }
        private SkeletonAnimation Skeleton { get; set; }
        private Vector3 HUDPosition { get; set; }

        private Sequence _colorTweenSequence;

        public SkeletonCharacterAnimator(CharacterBase root) : base(root)
        {
        }

        public override void ResetTarget(GameObject value)
        {
            base.ResetTarget(value);

            if (!(Skeleton is null))
            {
                Skeleton.AnimationState.Event -= RaiseEvent;
            }

            MeshRenderer = value.GetComponent<MeshRenderer>();
            if (MeshRenderer is null)
                throw new NotFoundComponentException<MeshRenderer>();

            Skeleton = value.GetComponent<SkeletonAnimation>();
            if (Skeleton is null)
                throw new NotFoundComponentException<SkeletonAnimation>();

            Skeleton.timeScale = TimeScale;

            var hud = Skeleton.skeleton.FindBone(StringHUD);
            if (hud is null)
                throw new SpineBoneNotFoundException(StringHUD);
            
            HUDPosition = hud.GetWorldPosition(Target.transform) - Root.transform.position;
            Skeleton.AnimationState.Event += RaiseEvent;
        }

        public override bool ValidateAnimator()
        {
            return base.ValidateAnimator() && !ReferenceEquals(Skeleton, null);
        }

        public override Vector3 GetHUDPosition()
        {
            return HUDPosition;
        }

        private void RaiseEvent(TrackEntry trackEntry, Spine.Event e)
        {
            OnEvent.OnNext(e.Data.Name);
        }

        public override void Hit()
        {
            base.Hit();
            ColorTween();
        }

        public override void Die()
        {
            base.Die();
            ColorTween();
        }

        private void ColorTween()
        {
            var mat = MeshRenderer.material;

            _colorTweenSequence?.Kill();

            _colorTweenSequence = DOTween.Sequence();
            _colorTweenSequence.Append(DOTween.To(() => ColorTweenFrom, value => mat.SetFloat("_FillPhase", value),
                ColorTweenTo, ColorTweenDuration));
            _colorTweenSequence.Append(DOTween.To(() => ColorTweenTo, value => mat.SetFloat("_FillPhase", value),
                ColorTweenFrom, ColorTweenDuration));
            _colorTweenSequence.Play().OnComplete(() => _colorTweenSequence = null);
        }
    }
}
