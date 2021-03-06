﻿using UnityEngine;
using System.Collections;
using UnityEditor;

namespace mulova.comunity {
	[CustomEditor(typeof(SixWayCam))]
	public class SixWayCamInspector : Editor {
		private SixWayCam comp;
		
		void OnEnable() {
			comp = (SixWayCam)target;
		}
		
		public override void OnInspectorGUI() {
			comp.Sync();
			DrawDefaultInspector();
			EditorGUILayout.Space();
		}
	}
	
}