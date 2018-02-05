using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using commons;
using comunity;


namespace convinity {
	class ShaderSearchTab : SearchTab<Material> {
		
		public ShaderSearchTab(TabbedEditorWindow window) : base("Shader", window) {
		}

		public override void OnSelected(bool sel) {}
		public override void OnFocus(bool focus) {}

		
		public override void OnHeaderGUI(List<Material> found) {
			EditorGUILayout.BeginHorizontal();
			EditorGUIUtil.TextField(null, ref shaderName);
			if (GUILayout.Button("Search")) {
				Search();
			}
			EditorGUILayout.EndHorizontal();
		}
		
		protected override List<Material> SearchResource(Object root) {
			List<Material> list = new List<Material>();
			foreach (Object o in SearchAssets(typeof(Material), FileType.Prefab, FileType.Material)) {
				Material mat = o as Material;
				if (mat != null && mat.shader.name.Contains(shaderName)) {
					list.Add(mat);
				}
			}
			foreach (Object o in SearchAssets(typeof(Renderer), FileType.Prefab, FileType.Material)) {
				Renderer r = o as Renderer;
				if (r != null && r.sharedMaterial != null && r.sharedMaterial.shader != null 
				    && r.sharedMaterial.shader.name.Contains(shaderName)) {
					list.Add(r.sharedMaterial);
				}
			}
			list.Sort(new MaterialSorter());
			return list;
		}
		
		protected override void OnInspectorGUI(List<Material> found) {
			EditorGUI.indentLevel += 2;
			Material remove = null;
			foreach (Material m in found) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(m.shader.name);
				EditorGUILayout.ObjectField(m, m.GetType(), false);
				if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
					remove = m;
				}
				EditorGUILayout.EndHorizontal();
			}
			if (remove != null) {
				found.Remove(remove);
			}
			EditorGUI.indentLevel -= 2;
		}
		
		public override void OnFooterGUI(List<Material> found) {
		}
		
		private class MaterialSorter :  IComparer<Material> {
			public int Compare(Material x, Material y) {
				if (x == null) {
					return 1;
				} else if (y == null) {
					return -1;
				}
				return x.shader.name.CompareTo(y.shader.name);
			}
		}
		
		private bool showShader;
		private string shaderName;

		public override void OnChangePlayMode() {}
		public override void OnChangeScene(string sceneName) {}
	}
	
}