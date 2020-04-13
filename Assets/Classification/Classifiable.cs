using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Classification
{
    public class Classifiable : MonoBehaviour
    {
        public abstract class TypeAsset : TypeFilter { }
        public abstract class TypeFilter : ScriptableObject
        {
            public abstract bool Filter(TypeAsset typeAsset);
        }

        [SerializeField]
        private List<TypeAsset> classes = new List<TypeAsset>();

        public IEnumerable<TypeAsset> AllClasses {
            get {
                return classes;
            }
        }

        public T GetClass<T>() where T: TypeAsset
        {
            var existing = Find<T>();
            return existing?.Item2;
        }

        public void SetClass<T>(T newClass) where T : TypeAsset
        {
            var existing = Find<T>();
            if (existing != null)
            {
                int existingIndex = existing.Item1;
                if (newClass != null)
                {
                    classes[existingIndex] = newClass;
                }
                else
                {
                    classes.RemoveAt(existingIndex);
                }
            }
            else if (newClass != null)
            {
                classes.Add(newClass);
            }
        }

        private Tuple<int, T> Find<T>() where T: TypeAsset
        {
            if (classes == null)
            {
                classes = new List<TypeAsset>();
                return null;
            }
            for (int i = 0; i < classes.Count; i++)
            {
                T found = classes[i] as T;
                if(found != null)
                {
                    return new Tuple<int, T>(i, found);
                }
            }
            return null;
        }

        public void Clear()
        {
            classes.Clear();
        }

        public void AddAllFrom(Classifiable other)
        {
            foreach(var nextClass in other.classes)
            {
                if (!classes.Contains(nextClass))
                {
                    classes.Add(nextClass);
                }
            }
        }
    }
}
