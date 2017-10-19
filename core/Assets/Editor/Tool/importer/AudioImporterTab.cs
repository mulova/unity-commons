using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;



namespace core {
	
	public class AudioImporterTab : EditorTab {
		private static List<AudioImport> settings = new List<AudioImport>();
		
		public AudioImporterTab(TabbedEditorWindow window) : base("Audio", window) {}
		
		public override void OnEnable() {
			settings = AudioImport.Load();
		}
		
		public override void OnDisable() {}

		public override void OnChangePlayMode() {}
		public override void OnChangeScene(string sceneName) {}
		
		public override void OnHeaderGUI() {
		}
		
		public override void OnInspectorGUI() {
			AudioImport remove = null;
			foreach (AudioImport s in settings) {
				if (EditorUI.DrawHeader(s.path)) {
					EditorUI.BeginContents();
					GUILayout.BeginHorizontal();
					Object obj = AssetDatabase.LoadAssetAtPath(s.path, typeof(Object));
					if (EditorGUIUtil.ObjectField<Object>(ref obj, false)) {
						if (obj != null) {
							s.path = AssetDatabase.GetAssetPath(obj);
						} else {
							s.path = string.Empty;
						}
					}
					if (GUILayout.Button("-", GUILayout.Width(20))) {
						remove = s;
					}
					GUILayout.EndHorizontal();
					EditorGUIUtil.Toggle("Apply", ref s.apply);
					GUI.enabled = s.apply;
					EditorGUIUtil.Toggle("Force To Mono", ref s.forceToMono);
					EditorUI.EndContents();
					GUI.enabled = true;
				}
			}
			if (remove != null) {
				settings.Remove(remove);
			}
		}
		
		public override void OnFooterGUI() {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+")) {
				settings.Add(new AudioImport());
			}
			if (GUILayout.Button("Load")) {
				settings = AudioImport.Load();
				CustomAssetPostprocessor.audioSettings = settings;
			}
			if (GUILayout.Button("Save")) {
				AudioImport.Save(settings);
			}
			EditorGUILayout.EndHorizontal();
		}

		public override void OnFocus(bool focus) {}
		public override void OnSelected(bool sel) {}
	}
}