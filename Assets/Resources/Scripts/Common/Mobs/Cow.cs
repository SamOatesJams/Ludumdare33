using UnityEngine;
using System.Collections;

namespace Realms.Common
{
    public class Cow : Mob
    {
        void Awake()
        {
            this.MobType = "Cow";
        }
    }
}