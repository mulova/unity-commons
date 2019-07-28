﻿using UnityEngine;
using comunity;
using UnityEditor;
using System.Collections.Generic;

namespace build
{
	public class AssetBundleDupReorderList : ReorderList<AssetBundleDup>
    {
		public AssetBundleDupReorderList(List<AssetBundleDup> list) : base(list) {
            displayAdd = false;
            displayRemove = false;
            base.drawer.elementHeightCallback = GetHeight;
        }

        private float GetHeight(int index)
        {
            return base.drawer.elementHeight * (this[index].refs.Count+1);
        }

        protected override bool DrawItem(AssetBundleDup item, Rect rect, int index, bool isActive, bool isFocused)
        {
			float lineHeight = 16;
			Rect lineRect = rect;
			lineRect.height = lineHeight;
			var color = GUI.backgroundColor;
			GUI.contentColor = Color.yellow;
			EditorGUI.ObjectField(lineRect, item.dup.reference, typeof(Object), false);
			GUI.contentColor = color;
			lineRect.x += 10;
			lineRect.width -= 10;
			for (int i=0; i<this[index].refs.Count; ++i)
			{
				lineRect.y += lineHeight+2;
				EditorGUI.ObjectField(lineRect, item.refs[i].reference, typeof(Object),  false);
			}
            return false;
        }
    }
}
