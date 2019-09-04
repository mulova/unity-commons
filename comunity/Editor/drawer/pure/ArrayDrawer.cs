﻿using System.Collections.Generic;
using Object = UnityEngine.Object;
using UnityEngine;
using mulova.commons;
using System;

namespace comunity
{
    public class ArrayDrawer<T> : ListDrawer<T> where T:class
    {
        private object target;
        private string fieldName;

        public ArrayDrawer(Object target, string fieldName) : this(target, fieldName, new ItemDrawer<T>())
        {
        }

        public ArrayDrawer(object target, string fieldName, IItemDrawer<T> itemDrawer)
            : base(new List<T>(ReflectionUtil.GetFieldValue<T[]>(target, fieldName)), itemDrawer)
        {
            this.target = target;
            this.fieldName = fieldName;
            onDuplicate += Refresh;
            onInsert += Refresh;
            onMove += Refresh;
            onRemove += Refresh;
            onChange += SetDirty;
        }

        void SetDirty()
        {
            changed = true;
        }

        void SetDirty(int arg1, T arg2)
        {
            SetDirty();
            Refresh();
        }

        void Refresh(int arg1, T arg2)
        {
            Refresh();
        }

        void Refresh(int arg1, int arg2, T arg3)
        {
            Refresh();
        }

        private void Refresh()
        {
            ReflectionUtil.SetFieldValue(target, fieldName, list.ToArray());
            SetDirty();
        }

        public override void Add(T item)
        {
            base.Add(item);
            Refresh();
        }
    }
}