using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public struct SpawnInfo
        {
            public string name;
            public GameObject go;
        }

        public List<PoolSpawnId> spawnInfoes = new List<PoolSpawnId>();
        public Dictionary<string, Pool> dict = new Dictionary<string, Pool>();

        private static ObjectPool instance;
        public static ObjectPool Instance => instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < spawnInfoes.Count; i++)
            {
                var d = spawnInfoes[i];
                Pool pool = new Pool(d.gameObject);
                pool.OnCreate = Pool_OnCreate;
                pool.OnUnspawn = Pool_OnUnspawn;
                pool.OnSpawn = Pool_OnSpawn;
                pool.OnDestroy = Pool_OnDestroy;
                dict.Add(d.name, pool);
            }
        }

        public GameObject Spawn(string name)
        {
            GameObject res = null;
            if (dict.ContainsKey(name))
            {
                res = dict[name].Spawn();
            }
            return res;
        }

        public void UnSpawn(GameObject go)
        {
            var name = go.name;
            dict[name].UnSpawn(go);
        }

        private GameObject Pool_OnCreate(GameObject goTemplate)
        {
            var go = Instantiate(goTemplate);
            go.SetActive(true);
            go.name = goTemplate.name;
            return go;
        }

        private void Pool_OnUnspawn(GameObject goTemplate)
        {
            goTemplate.SetActive(false);
        }

        private void Pool_OnSpawn(GameObject goTemplate)
        {
            goTemplate.SetActive(true);
        }

        private void Pool_OnDestroy(GameObject go)
        {
            Destroy(go);
        }

        public void Flush()
        {
            foreach (var item in dict)
            {
                item.Value.Flush();
            }
        }
    }
}
