using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utils.HelperScripts.Utils;
namespace Utils.HelperScripts.Editor.Manipulator.Base
{
	public class AllComponentSelector : EditorWindow
	{
		protected virtual bool JustChildren => false;


		private Type _component;
		private Transform _parent;
		private Transform _prevParent;
		private Type[] _componentTypes;
		private Dictionary<string, Type> _dictionary = new();
		private string[] _componentNames;

		private int _selectedComponent;
		protected virtual string ButtonText => "Select";
		protected virtual string ComponentText => "Component";

		protected virtual bool RenderAfterComponents => true;

		protected Type TargetComponent => _component;

		public Transform Parent => _parent;

		protected virtual Type[] AvailableTypes => Array.Empty<Type>();

		protected void Init()
		{
			titleContent = new GUIContent(GetType().Name);
			if (!JustChildren)
				ObtainAllComponents();
			OnInit();
		}

		protected virtual void OnInit()
		{
		}

		private void ObtainAllComponents()
		{
			_componentTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.IsClass && type.IsSubclassOf(typeof(Component))).ToArray();


			List<Type> types = new List<Type>();

			Func<Type, bool> canAdd = GetTypeValidator();

			foreach (Type type in _componentTypes)
			{
				if (canAdd(type))
				{
					if (types.All(t => t.Name != type.Name))
					{
						types.Add(type);
					}
				}
			}

			_componentTypes = types.ToArray();
			GetIndex(ref _dictionary, _componentTypes, ref _componentNames);
		}
		private Func<Type, bool> GetTypeValidator()
		{

			Func<Type, bool> canAdd;

			if (AvailableTypes.Length > 0)
			{
				canAdd = type => AvailableTypes.Contains(type);
			}
			else
			{
				canAdd = type => true;
			}
			return canAdd;
		}

		protected void GetIndex(ref Dictionary<string, Type> dictionary, Type[] types, ref string[] names)
		{
			dictionary.Clear();
			dictionary = types.ToDictionary(t => t.Name, t => t);

			names = types.Select(t => t.Name).ToArray();
		}

		private void ObtainChildComponents(bool validate)
		{
			if (validate)
			{
				if (_prevParent == Parent)
				{
					return;
				}

				_selectedComponent = 0;
			}

			_prevParent = Parent;

			_componentTypes = GetChildrenTypes();
			GetIndex(ref _dictionary, _componentTypes, ref _componentNames);
		}

		protected Type[] GetChildrenTypes()
		{
			List<Type> types = new List<Type>();

			TransformHelper.ApplyToAllChildren(Parent, transform =>
			{
				Component[] components = transform.GetComponents(typeof(Component));
				foreach (Component component in components)
				{
					if (!types.Contains(component.GetType()))
					{
						types.Add(component.GetType());
					}
				}
			});
			return types.OrderBy(t => t.Name).ToArray();
		}

		private void ObtainComponents()
		{
			if (JustChildren)
				ObtainChildComponents(false);
			else
				ObtainAllComponents();
		}

		private void OnGUI()
		{
			var parent = EditorGUILayout.ObjectField("Parent", Parent, typeof(Transform), true);

			if (parent == null)
			{
				EditorGUILayout.LabelField("Parent is null");
				return;
			}

			_parent = parent as Transform;

			if (JustChildren)
			{
				ObtainChildComponents(true);
			}

			bool quit = false;
			if (!RenderAfterComponents)
				Render(out quit);

			// ReSharper disable once Unity.IncorrectScriptableObjectInstantiation
			StringListSearchProvider provider = new StringListSearchProvider(_componentNames, selection => GetSelectedByName(selection, _componentNames, i => _selectedComponent = i));
			DisplayComponentSelector(ComponentText, _componentNames, ref _selectedComponent, provider);

			if (RenderAfterComponents)
				Render(out quit);

			if (quit) return;

			if (GUILayout.Button(ButtonText))
			{
				if (_dictionary.Count == 0) ObtainComponents();

				_component = _dictionary.GetValue(_componentNames[_selectedComponent]);

				if (TargetComponent == null || Parent == null)
				{
					Debug.LogError($"{(TargetComponent == null ? ComponentText : "Parent")} is null, name: {_componentNames[_selectedComponent]}, count: {_dictionary.Count}");
					return;
				}

				DoAction();
			}

			if (!JustChildren)
			{
				if (GUILayout.Button("Refresh components"))
				{
					ObtainAllComponents();
					// ObtainChildComponents(false);
				}
			}
		}

		protected void DisplayComponentSelector(string text, string[] componentNames, ref int selected, StringListSearchProvider provider, int maxWidth = 70)
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(text, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(maxWidth));

			if (GUILayout.Button(componentNames[selected], EditorStyles.popup))
			{
				SearchWindowContext windowContext = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
				SearchWindow.Open(windowContext, provider);
			}

			EditorGUILayout.EndHorizontal();
		}

		protected virtual void Render(out bool quit)
		{
			quit = false;
		}

		protected virtual void DoAction()
		{
		}

		protected int GetSelectedByName(string selection, string[] names, Action<int> setTarget)
		{
			int index = Array.IndexOf(names, selection);
			setTarget(index);
			return index;
		}

		private static GUIContent Label(string text)
		{
			GUIContent label = new GUIContent
			{
				text = text
			};
			EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(label).x + 10;
			return label;
		}

		protected void SetTarget(Transform transform)
		{
			_parent = transform;
		}
	}
}