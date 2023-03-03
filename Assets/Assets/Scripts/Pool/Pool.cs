using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class Pool
    {
        private List<GameObject> activeObject;
        private Stack<GameObject> unActiveObject;

        public System.Action<GameObject> OnSpawn;
        public System.Func<GameObject, GameObject> OnCreate;
        public System.Action<GameObject> OnUnspawn;
        public System.Action<GameObject> OnDestroy;

        public GameObject go;
        public Pool(GameObject go)
        {
            this.go = go;
            activeObject = new List<GameObject>();
            unActiveObject = new Stack<GameObject>();
        }

        public GameObject Spawn()
        {
            GameObject activeGo;
            if (unActiveObject.Count > 0)
            {
                var disableObject = unActiveObject.Pop();
                OnSpawn?.Invoke(disableObject);
                activeObject.Add(disableObject);
                activeGo = disableObject;
            }
            else
            {
                var newGO = OnCreate?.Invoke(go);
                activeObject.Add(newGO);
                activeGo = newGO;
            }
            return activeGo;
        }

        public void UnSpawn(GameObject inActiveGO)
        {
            OnUnspawn?.Invoke(inActiveGO);
            unActiveObject.Push(inActiveGO);
            activeObject.Remove(inActiveGO);
        }

        public void Flush()
        {
            var length = activeObject.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                var go = activeObject[i].gameObject;

                if (go)
                    OnDestroy.Invoke(go);
                activeObject.RemoveAt(i);
            }

            activeObject.Clear();

            length = unActiveObject.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                var go = unActiveObject.Pop();

                if (go)
                    OnDestroy.Invoke(go);
            }

            unActiveObject.Clear();
        }
    }
}
