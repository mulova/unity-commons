﻿using System;
using System.Ex;
using mulova.unicore;
using UnityEditor;
using UnityEngine;

namespace mulova.comunity
{
    public class StrEnumItemDrawer<T> : ItemDrawer<T> where T:class, new()
    {
        private readonly string strVarName;
        private readonly string enumVarName;

        public StrEnumItemDrawer(string strVarName, string enumVarName)
        {
            this.strVarName = strVarName;
            this.enumVarName = enumVarName;
        }

        public override bool DrawItem(Rect position, int index, T obj, out T changedObj)
        {
            Rect[] r = SplitHorizontally(position, 0.7f);
            if (obj == null)
            {
                obj = new T();
            }
            string str = obj.GetFieldValue<string>(strVarName);
            if (str == null)
            {
                str = string.Empty;
            }
            string newStr = GUI.TextField(r[0], str);
            changedObj = obj;
            if (newStr != str)
            {
                obj.SetFieldValue(strVarName, newStr);
                return true;
            }
            Enum e = obj.GetFieldValue<Enum>(enumVarName);
            Enum selE = EditorGUI.EnumPopup(r[1], e);
            if (e != selE)
            {
                obj.SetFieldValue(enumVarName, selE);
                return true;
            }
            return false;
        }

        protected Rect[] SplitHorizontally(Rect rect, float ratio)
        {
            Rect r1 = rect;
            Rect r2 = rect;
            r1.width = rect.width*ratio;
            r2.x = r1.x + r1.width;
            r2.width = rect.width - r1.width;
            return new Rect[] { r1, r2};
        }
    }
}

