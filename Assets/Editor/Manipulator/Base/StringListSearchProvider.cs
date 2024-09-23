using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Utils.HelperScripts.Editor.Manipulator.Base
{
	public class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		private string[] _listItems;
		private Action<string> onSetIndexCallback;

		public StringListSearchProvider(string[] listItems, Action<string> onSet)
		{
			_listItems = listItems;
			onSetIndexCallback = onSet;
		}

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			List<SearchTreeEntry> searchList = new List<SearchTreeEntry>();
			searchList.Add(new SearchTreeGroupEntry(new GUIContent("Components"), 0));


			List<string> sortedListItems = _listItems.ToList();

			sortedListItems = sortedListItems.OrderBy(s => s).ToList();
			List<string> groups = new List<string>();
			foreach (string item in sortedListItems)
			{
				string[] entryTitle = item.Split('/');
				string groupName = "";
				for (var i = 0; i < entryTitle.Length - 1; i++)
				{
					groupName += entryTitle[i];
					if (!groups.Contains(groupName))
					{
						searchList.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
						groups.Add(groupName);
					}

					groupName += "/";
				}

				SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
				entry.level = entryTitle.Length;
				entry.userData = entryTitle.Last();
				searchList.Add(entry);
			}


			return searchList;
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			onSetIndexCallback?.Invoke((string)SearchTreeEntry.userData);
			return true;
		}
	}
}