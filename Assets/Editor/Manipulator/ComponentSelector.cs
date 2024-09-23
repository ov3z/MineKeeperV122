using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils.HelperScripts.Editor.Manipulator.Base;
using Utils.HelperScripts.Utils;
namespace Utils.HelperScripts.Editor.Manipulator
{
	public class ComponentSelector : AllComponentSelector
	{
		protected override bool JustChildren => true;

		[MenuItem("Tools/Manipulator/Component Selector &s")]
		public static void OpenWindow()
		{
			ComponentSelector selector = GetWindow<ComponentSelector>();
			selector.Init();
			if (Selection.count > 0)
			{
				selector.SetTarget(Selection.gameObjects[0].transform);
			}
		}

		protected override void DoAction()
		{
			List<GameObject> selections = new List<GameObject>();

			TransformHelper.ApplyToAllChildren(Parent, transform =>
			{
				if (transform.GetComponent(TargetComponent) != null)
				{
					selections.Add(transform.gameObject);
				}
			});

			Selection.objects = selections.ToArray();
		}
	}
}