using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;
namespace UI
{
	public class ProgressUI : MonoBehaviour
	{
		[SerializeField] protected RectTransform Rect;

		[SerializeField] protected float MinWidth;

		protected float Width;

		protected float _fill;
		private bool _inited;

		public virtual float FillAmountX
		{
			get => _fill;
			set
			{
				value = value.Clamp(0, 1f);
				_fill = value;
				SetFillX(_fill * Width);
			}
		}public virtual float FillAmountY
		{
			get => _fill;
			set
			{
				value = value.Clamp(0, 1f);
				_fill = value;
				SetFillY(_fill * Width);
			}
		}

		
		private void Start()
		{
			InitY();
			InitX();
		}
		protected void InitY()
		{

			if (_inited) return;

			_inited = true;
			Width = Rect.sizeDelta.y;

			SetFillY(0);
		}
		protected void InitX()
		{

			if (_inited) return;

			_inited = true;
			Width = Rect.sizeDelta.x;

			SetFillX(0);
		}

		
		protected virtual void SetFillY(float value)
		{
			InitY();
			Rect.sizeDelta = Rect.sizeDelta.SetY(Mathf.Max(value, MinWidth));
		}
		protected virtual void SetFillX(float value)
		{
			InitY();
			Rect.sizeDelta = Rect.sizeDelta.SetX(Mathf.Max(value, MinWidth));
		}

		public virtual void DoFillY(float value, float duration = 0.3f)
		{
			value = value.Clamp(0, 1f);
			InitY();
			DOVirtual.Float(_fill, value, duration, f =>
			{
				FillAmountY = f;
			});
		}

		public virtual void DoFill2X(float value, float duration = 0.3f)
		{
			value = value.Clamp(0, 1f);
			InitX();
			DOVirtual.Float(_fill, value, duration, f =>
			{
				FillAmountX = f;
			});
		}
	}
}