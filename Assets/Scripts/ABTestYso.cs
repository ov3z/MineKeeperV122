using System;
using UnityEngine;
using YsoCorp.GameUtils;

namespace DefaultNamespace
{
    public class ABTestYso : MonoBehaviour
    {
    
        public static ABTestYso Instance;
        public bool withIntro = false;
   
        public void GetScene() {
            
            if (YCManager.instance.abTestingManager.IsPlayerSample("withIntro"))
            {
                withIntro = true;
            }
            if (YCManager.instance.abTestingManager.IsPlayerSample("withoutIntro"))
            {
                withIntro = false;
            }
        }
    }
}