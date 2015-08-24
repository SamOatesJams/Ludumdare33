using UnityEngine;
using System.Collections;

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
        void Start()
        {
            var spawnArea = this.GetComponent<BoxCollider>();

            int initialCount = UnityEngine.Random.Range(1, this.MaxMobCount);
            for (int mobIndex = 0; mobIndex < initialCount; ++mobIndex)
            {
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
        }
    }
}
