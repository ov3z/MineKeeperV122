using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Utils
{
	public static class MiscHelper
	{
		public static void ForEach<T>(this T[] source, [NotNull] Action<T> predicate)
		{
			foreach (T item in source)
			{
				predicate(item);
			}
		}

		public static void SafeText(this TextMeshProUGUI textMesh, string text)
		{
			if (textMesh.text != text)
			{
				textMesh.text = text;
			}
		}


		public static void FadeAlpha(this Image image, float a)
		{
			Color color = image.color;
			color.a = a;
			image.color = color;
		}


		public static void FadeAlpha(this TextMeshProUGUI image, float a)
		{
			Color color = image.color;
			color.a = a;
			image.color = color;
		}

		public static float Cap(ref this float number, float min, float max)
		{
			if (number > max) number = max;
			if (number < min) number = min;

			return number;
		}

		public static T GetMiddle<T>(this T[] arr)
		{
			// Calculate the starting and ending indices of the central elements
			int mid = arr.Length / 2;
			int start = mid - (1 / 2);
			int end = mid + (1 / 2) + (1 % 2); // Add 1 for odd n to include the middle element

			// Extract and return the central elements
			return arr.Skip(start).Take(1).First();
		}

		public static T[] ToArray<T>(this T target) where T : Enum => (T[])Enum.GetValues(target.GetType());

		public static float Abs(this float f) => Mathf.Abs(f);
		public static int Abs(this int f) => Mathf.Abs(f);

		public static int IndexOf<T>(this T target, T[] array)
		{
			return Array.IndexOf(array, target);
		}


		public static int Clamp(this int value, int min, int max) => Math.Clamp(value, min, max);
		public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);



		public static T Random<T>(this T[] array) => array is { Length: > 0 } ? array[UnityEngine.Random.Range(0, array.Length)] : default;
		public static T Random<T>(this T[] array, T except)
		{
			if (except == null) return array.Random();
			IEnumerable<T> exc = array.Except(new[]
			{
				except
			});
			return exc.Random();
		}
		public static T Random<T>(this List<T> array) => array is { Count: > 0 } ? array[UnityEngine.Random.Range(0, array.Count)] : default;
		public static T Random<T>(this IEnumerable<T> array) => array != null && array.Count() > 0 ? array.ElementAt(UnityEngine.Random.Range(0, array.Count())) : default;

		public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
		{
			TV value;
			return dict.TryGetValue(key, out value) ? value : defaultValue;
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>
			(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		public static Quaternion Euler(this Vector3 rotation) => Quaternion.Euler(rotation);

		public static Vector3 SetX(this Vector3 vector3, float x) => vector3.SetAllVector(x: x);
		public static Vector3 SetY(this Vector3 vector3, float y) => vector3.SetAllVector(y: y);
		public static Vector3 SetZ(this Vector3 vector3, float z) => vector3.SetAllVector(z: z);

		public static Vector2 SetX(this Vector2 vector2, float x) => new Vector2(x, vector2.y);
		public static Vector2 SetY(this Vector2 vector2, float y) => new Vector2(vector2.x, y);

		public static void RefLerp(ref this Vector3 vector3, Vector3 to, float time)
		{
			vector3 = Vector3.Lerp(vector3, to, time);
		}


		private static Vector3 SetAllVector(this Vector3 vector3, float? x = null, float? y = null, float? z = null)
		{
			if (x != null) vector3.x = (float)x;
			if (y != null) vector3.y = (float)y;
			if (z != null) vector3.z = (float)z;

			return vector3;
		}


		public static T Mode<T>(this IEnumerable<T> array, Func<T, IComparable> compare)
		{
			var groups = array.GroupBy(compare, arg => arg);
			int maxCount = groups.Max(g => g.Count());
			return groups.First(g => g.Count() == maxCount).First();

		}
		
		
		public static void CopyRectTransform(Transform target, Transform source)
		{
			if (target.TryGetComponent<RectTransform>(out var targetRect))
			{
				source.TryGetComponent<RectTransform>(out var sourceRect);
				targetRect.localPosition = sourceRect.localPosition;
				targetRect.localRotation = sourceRect.localRotation;
				targetRect.localScale = sourceRect.localScale;
				targetRect.anchorMin = sourceRect.anchorMin;
				targetRect.anchorMax = sourceRect.anchorMax;
				targetRect.anchoredPosition = sourceRect.anchoredPosition;
				targetRect.sizeDelta = sourceRect.sizeDelta;
				targetRect.pivot = sourceRect.pivot;
			}
		}

		private static Transform GetAvailableTransform(Transform[] target, Transform[] source, int i)
		{
			if (target.Length <= i)
			{
				return source[i];
			}
			else if (source.Length <= i)
			{
				return target[i];
			}
			return target[i];
		}

	}
}