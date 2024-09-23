using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace UI
{
    public class ProgressShadowed : ProgressUI
    {
        [SerializeField] private RectTransform Shadow;
        private Tween[] _animations;

        private float _fillShadow;

        public override float FillAmountX
        {
            get => _fill;
            set
            {
                value = value.Clamp(0, 1f);
                _fill = value;
                _fillShadow = value;
                InitX();
                //SetFill(_fill * Width);
                SetFillX(_fill * Width);
            }
        }

        public override float FillAmountY
        {
            get => _fill;
            set
            {
                value = value.Clamp(0, 1f);
                _fill = value;
                _fillShadow = value;
                InitY();
                SetFillY(_fill * Width);
            }
        }

        private void SetShadowFillAmount(float value)
        {
            InitY();
            if (Shadow == null) return;
            if (Rect == null) return;
            _fillShadow = value.Clamp(0, 1f);
            value = _fillShadow * Width;
            Shadow.sizeDelta = Rect.sizeDelta.SetY(Mathf.Max(value, MinWidth));
        }

        private void SetMainFillAmount(float value)
        {
            InitY();
            if (Rect == null) return;
            _fill = value.Clamp(0, 1f);
            value = _fill * Width;
            Rect.sizeDelta = Rect.sizeDelta.SetY(Mathf.Max(value, MinWidth));
        }

        private void SetShadowFillAmountX(float value)
        {
            InitX();
            if (Shadow == null) return;
            _fillShadow = value.Clamp(0, 1f);
            value = _fillShadow * Width;
            Shadow.sizeDelta = Rect.sizeDelta.SetX(Mathf.Max(value, MinWidth));
        }

        private void SetMainFillAmountX(float value)
        {
            InitX();
            if (Rect == null) return;
            _fill = value.Clamp(0, 1f);
            value = _fill * Width;
            Rect.sizeDelta = Rect.sizeDelta.SetX(Mathf.Max(value, MinWidth));
        }

        protected override void SetFillX(float value)
        {
            base.SetFillX(value);
            Shadow.sizeDelta = Rect.sizeDelta;
        }

        protected override void SetFillY(float value)
        {
            base.SetFillY(value);
            Shadow.sizeDelta = Rect.sizeDelta;
        }

        public override void DoFillY(float value, float duration = 0.3f)
        {
            StopAnimations();
            value = value.Clamp(0, 1f);
            InitY();

            if (_fill > value) _fill = 0;
            if (_fillShadow > value) _fillShadow = 0;

            _animations = new Tween[]
            {
                DOVirtual.Float(_fillShadow, value, duration, SetShadowFillAmount),
                DOVirtual.Float(FillAmountY, value, duration, SetMainFillAmount).SetDelay(0.2f)
                    .OnComplete(ResetAnimations)
            };
        }

        public void DoFill2X(float value, float duration = 0.3f, Action Oncomplete = null)
        {
            StopAnimations();
            value = value.Clamp(0, 1f);
            InitX();

            _animations = new Tween[]
            {
                DOVirtual.Float(FillAmountX, value, duration, SetMainFillAmountX),
                DOVirtual.Float(_fillShadow, value, duration, SetShadowFillAmountX).SetDelay(0.2f)
                    .OnComplete(() =>
                    {
                        ResetAnimations();
                        Oncomplete?.Invoke();
                    })
            };
        }

        private void ResetAnimations()
        {
            _animations = null;
        }

        private void StopAnimations()
        {
            if (_animations is { Length: > 0 })
            {
                foreach (Tween tween in _animations)
                {
                    if (tween != null && tween.IsActive())
                    {
                        tween.Kill();
                    }
                }
            }
        }
    }
}