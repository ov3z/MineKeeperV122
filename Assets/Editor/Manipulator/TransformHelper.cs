using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Utils.HelperScripts.Utils
{
	public static class TransformHelper
	{
		public static void ApplyToAllChildren(Queue<Transform> children, Action<Transform> actionForCurrent)
		{
			while (children.Count > 0)
			{
				Transform current = children.Dequeue();
				actionForCurrent(current);

				foreach (Transform child in current.transform)
				{
					// Debug.Log($"{child.name} {child.childCount}");
					children.Enqueue(child);
				}
			}
		}

		public static void ApplyToAllChildren(this Transform parent, Action<Transform> actionForCurrent)
		{
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(parent);
			ApplyToAllChildren(queue, actionForCurrent);
		}

		public static void ApplyToAllChildren<T>(Transform parent, Action<T> action) where T : Component
		{
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(parent);
			ApplyToAllChildren(queue, (t) =>
			{
				if (t.TryGetComponent(out T output))
				{
					action(output);
				}
			});
		}

		public static GameObject[] GetChildren(this Transform parent)
		{
			List<GameObject> children = new List<GameObject>();
			int count = parent.childCount;
			for (var i = 0; i < count; i++)
			{
				children.Add(parent.GetChild(i).gameObject);
			}

			return children.ToArray();
		}

		public static T[] GetChildren<T>(this Transform parent) where T : Component
		{
			List<T> children = new List<T>();
			int count = parent.childCount;
			for (var i = 0; i < count; i++)
			{
				Transform transform = parent.GetChild(i);
				if (transform.TryGetComponent(out T output))
				{
					children.Add(output);
				}
			}

			return children.ToArray();
		}

		public static T[] GetChildrenWithComponenets<T>(this Transform parent) where T : Component
		{
			List<T> children = new List<T>();
			int count = parent.childCount;
			for (var i = 0; i < count; i++)
			{
				Transform transform = parent.GetChild(i);
				if (transform.GetComponent<T>() != null)
				{
					children.AddRange(transform.GetComponents<T>());
				}
			}

			return children.ToArray();
		}

		public static GameObject[] GetChildren(this Transform parent, Func<GameObject, bool> condition)
		{
			GameObject[] children = GetChildren(parent);

			List<GameObject> newChildren = new List<GameObject>();

			if (children.Any(condition))
			{
				newChildren.AddRange(children.Where(condition));
			}

			return newChildren.ToArray();
		}

		public static GameObject GetChild(this Transform parent, Func<GameObject, bool> condition)
		{
			var children = GetChildren(parent, condition);
			if (children.Length > 0) return children.First();
			return null;
		}

		public static Transform GetClosestTarget(this Transform[] targets, Transform target)
		{

			Transform bestTarget = null;
			float closestDistanceSqr = 1000000;
			Vector3 currentPosition = target.position;
			for (int i = 0; i < targets.Length; i++)
			{
				Vector3 directionToTarget = targets[i].transform.position - currentPosition;
				float dSqrToTarget = directionToTarget.sqrMagnitude;
				if (dSqrToTarget < closestDistanceSqr)
				{
					closestDistanceSqr = dSqrToTarget;
					bestTarget = targets[i];
				}
			}

			if (bestTarget != null) return bestTarget;
			return null;
		}

		public static T GetComponentInParent<T>(this Transform transform)
		{
			if (transform.parent != null)
			{
				T type = transform.parent.GetComponent<T>();

				if (type != null)
				{
					return type;
				}
				return transform.parent.GetComponentInParent<T>();
			}
			return default;
		}


		public static T Random<T>(this T[] array) => array[UnityEngine.Random.Range(0, array.Length)];
		public static T Random<T>(this List<T> array) => array[UnityEngine.Random.Range(0, array.Count)];
		public static T Random<T>(this IEnumerable<T> array) => array.ElementAt(UnityEngine.Random.Range(0, array.Count()));

		public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
		{
			TV value;
			return dict.TryGetValue(key, out value) ? value : defaultValue;
		}




	}
}