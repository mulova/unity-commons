﻿using UnityEngine;
using System.Collections;
using comunity;
using UnityEditor;

namespace convinity
{
    public class SceneHistoryItemDrawer : ItemDrawer<SceneHistoryItem>
    {
        private UnityObjIdDrawer drawer = new UnityObjIdDrawer();

        public override bool DrawItem(Rect rect, int index, SceneHistoryItem item, out SceneHistoryItem newItem)
        {
            UnityObjId newObj = null;
            newItem = item;
            return drawer.DrawItem(rect, index, item.first, out newObj);
        }
    }
}

