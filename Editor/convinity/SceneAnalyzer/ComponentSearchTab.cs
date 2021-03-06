﻿using System;
using System.Collections.Generic;
using mulova.commons;
using mulova.unicore;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace convinity
{
    class ComponentSearchTab : SearchTab<Object>
    {
        private TypeSelector selector;

        public ComponentSearchTab(TabbedEditorWindow window) : base("Comp Search", window)
        {
            selector = new TypeSelector(typeof(Object));
            selector.SetSelected(typeof(Transform));
        }

        public override void OnHeaderGUI(List<Object> found)
        {
            EditorGUILayout.BeginHorizontal();
            selector.DrawSelector();
            if (GUILayout.Button("Search", EditorStyles.miniButton))
            {
                Search();
            }
            EditorGUILayout.EndHorizontal();
        }

        protected override List<Object> SearchResource()
        {
            Type type = selector.type;
            List<Object> list = new List<Object>();
			foreach (var root in roots)
			{
				if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(root)))
				{
					if (type != null)
					{
						foreach (Object o in SearchAssets(type, FileType.Prefab, FileType.Asset, FileType.Material, FileType.Anim))
						{
							Object c = o as Object;
							list.Add(c);
						}
					}
				} else
				{
					IEnumerable<Transform> trans = null;
					if (root is GameObject)
					{
						trans = new Transform[] { (root as GameObject).transform };
					} else if (root == null)
					{
						trans = EditorUtil.GetSceneRoots().ConvertAll(o=>o.transform);
					}
					if (trans != null)
					{
						foreach (Transform t in trans)
						{
							Component[] comps = type != null? t.GetComponentsInChildren(type, true) : GetMissingComponents(t.gameObject);
							foreach (Object c in comps)
							{
								list.Add(c);
							}
						}
					}
				}
			}
            return list;
        }

        public override void OnChangePlayMode(PlayModeStateChange stateChange)
        {
        }

        public override void OnChangeScene(string sceneName)
        {
        }

        private Component[] GetMissingComponents(GameObject g)
        {
            List<Component> comps = new List<Component>();
            Transform[] trans = g.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in trans)
            {
                foreach (Component c in t.GetComponents<Component>())
                {
                    if (c == null)
                    {
                        comps.Add(t);
                        break;
                    }
                }
            }
            return comps.ToArray();
        }

        protected override void OnInspectorGUI(List<Object> found)
        {
            GUI.enabled = true;
            ListDrawer<Object> drawer = new ListDrawer<Object>(found);
			drawer.Draw();
        }

        public override void OnFooterGUI(List<Object> found)
        {
        }

        public override void OnFocus(bool focus)
        {
        }

        public override void OnSelected(bool sel)
        {
        }
    }
}