using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace core {
	class ColliderSearchTab : SearchTab<Collider>
	{
		private Camera cam;
		private Ray ray;
		private RaycastHit[] hits;
		
		public ColliderSearchTab(TabbedEditorWindow window) : base("Collider", window)
		{
			cam = Camera.main;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}
		
		public override void OnHeaderGUI(List<Collider> found) {
			EditorGUIUtil.Popup<Camera>("Camera", ref cam, Camera.allCameras);
		}
		
		protected override List<Collider> SearchResource(Object root)
		{
			List<Collider> list = new List<Collider>();
			foreach (RaycastHit h in Physics.RaycastAll(ray)) {
				list.Add(h.collider);
			}
			return list;
		}
		public override void OnChangePlayMode() {}
		public override void OnChangeScene(string sceneName) {}
		public override void OnSelected(bool sel) {}
		public override void OnFocus(bool focus) {}
		
		protected override void OnInspectorGUI(List<Collider> found)
		{
			foreach (Collider o in found) {
				EditorGUILayout.ObjectField (o, o.GetType (), true);
			}
		}
		
		public override void OnFooterGUI(List<Collider> found) {}
		
		public override void OnDisable() {
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}
		
		
		void OnSceneGUI(SceneView view) {
			Event e = Event.current;
			if (e.type == EventType.MouseDown) {
				
				ray = UnityEditor.HandleUtility.GUIPointToWorldRay(e.mousePosition);
				//				ray = cam.ScreenPointToRay(new Vector3(e.mousePosition.x, e.mousePosition.y, 0));
				Search();
			}
		}
	}
}
