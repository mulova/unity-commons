using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;
using System.Text;
using System;
using comunity;

namespace build
{
	public class AssetBuilderTab : EditorTab
	{
		private string[] names;
		private string selected;
		private TexFormatGroup texFormat = TexFormatGroup.AUTO;
        private AssetBundlePath path = AssetBundlePath.inst;

		public AssetBuilderTab(string[] names, TabbedEditorWindow window) : base("AssetBuilder", window)
		{
			this.names = names;
			this.selected = names[0];
		}

		private AssetBuilderV1 CreateBuilder()
		{
			return new AssetBuilderV1(selected, EditorUserBuildSettings.activeBuildTarget, texFormat);
		}

		public override void OnEnable()
		{
		}

		public override void OnHeaderGUI()
		{
			EditorGUIUtil.Popup<string>("Zone", ref selected, names);
			EditorGUIUtil.Popup<TexFormatGroup>("Texture Format", ref texFormat, TexFormatGroup.Values);
		}

		private bool appendCleanSnapshot;

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			EditorGUIUtil.Toggle("Append Clean Snapshot", ref appendCleanSnapshot);
			EditorGUILayout.EndVertical();
			if (GUILayout.Button("Build", GUILayout.Height(50)))
			{
                path.Save(AssetBundlePath.PATH);
				Build();
			}
			EditorGUILayout.EndHorizontal();

			path.DrawInspectorGUI();
		}

		public override void OnFooterGUI()
		{
		}

		public override void OnDisable()
		{
		}

		public override void OnChangePlayMode()
		{
		}

		public override void OnChangeScene(string sceneName)
		{
		}

		public override void OnSelected(bool sel)
		{
		}

		public override void OnFocus(bool focus)
		{
		}

		private void AddObjects(List<Object> list, params Object[] objs)
		{
			foreach (Object o in objs)
			{
				if (o == null)
				{
					continue;
				}
				if (EditorAssetUtil.IsFolder(o)&&!list.Contains(o))
				{
					list.Add(o);
				}
			}
		}


		private string OpenFolderPanel(string title, string defaultDir)
		{
			if (!Directory.Exists(defaultDir))
			{
				Directory.CreateDirectory(defaultDir);
			}
			return EditorUtil.OpenFolderPanel(title);
		}

		private void Build()
		{
			try
			{
				AssetBuilderV1 builder = CreateBuilder();
				builder.Build();
				if (appendCleanSnapshot)
				{
					AssetBuilderV1 cleanBuilder = new AssetBuilderV1(builder.zone, builder.buildTarget, texFormat, "", builder.newVersion+"c");
					cleanBuilder.Build();
				}
				EditorUtil.OpenExplorer(builder.GetOutputDir());
				EditorUtility.DisplayDialog("Info", "Build Complete", "OK");
			} catch (Exception ex)
			{
				Debug.LogError(ex.Message+"\n"+ex.StackTrace);
				EditorUtility.DisplayDialog("Error", "Check error log", "OK");
			}
		}
	}
}
