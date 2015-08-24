using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Realms.Server
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpawnArea : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public GameObject MobPrefab = null;

        /// <summary>
        /// 
        /// </summary>
        public int MaxMobCount = 3;

        /// <summary>
        /// 
        /// </summary>
        public float MinSpawnTime = 10.0f;

        /// <summary>
        /// 
        /// </summary>
        public float MaxSpawnTime = 30.0f;

        /// <summary>
        /// 
        /// </summary>
        private List<float> m_spawnRequests = new List<float>();

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            int initialCount = UnityEngine.Random.Range(1, this.MaxMobCount);
            for (int mobIndex = 0; mobIndex < initialCount; ++mobIndex)
            {
                SpawnMob();
            }
        }

        private void SpawnMob()
        {
            var spawnArea = this.GetComponent<BoxCollider>();
            var newMob = GameObject.Instantiate(this.MobPrefab);

            var spawnPoint = new Vector3(
                UnityEngine.Random.Range(0, spawnArea.bounds.size.x),
                0.0f,
                UnityEngine.Random.Range(0, spawnArea.bounds.size.z)
            );

            spawnPoint = spawnPoint - spawnArea.bounds.extents;

            newMob.transform.parent = this.transform;
            newMob.transform.localPosition = spawnPoint;
            newMob.transform.localEulerAngles = new Vector3(0.0f, UnityEngine.Random.Range(0.0f, 360.0f), 0.0f);

            var mobComponent = newMob.GetComponent<Common.Mob>();
            mobComponent.SpawnArea = spawnArea;
        }

        /// <summary>
        /// 
        /// </summary>
        void FixedUpdate()
        {
            var completed = new List<float>();
            foreach (var request in m_spawnRequests)
            {
                if (Time.time - request >= 0.0f)
                {
                    SpawnMob();
                    completed.Add(request);
                }
            }

            foreach (var request in completed)
            {
                m_spawnRequests.Remove(request);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instant"></param>
        public void RequestSpawn()
        {
            m_spawnRequests.Add(Time.time + UnityEngine.Random.Range(this.MinSpawnTime, this.MaxSpawnTime));
        }
    }
}
