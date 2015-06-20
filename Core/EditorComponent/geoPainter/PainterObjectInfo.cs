using UnityEngine;
using System.Collections.Generic;

namespace PainterObjectRegisterInfo
{
    [System.Serializable]
    public class PainterObjectInfo
    {
        [SerializeField]
        public float distance;
        [SerializeField]
        public int perSent;

        public void Init()
        {
            distance = 5;
            perSent = 30;
        }
    }

    [System.Serializable]
    public class PainterObjectGroupInfo
    {
        [SerializeField]
        public int selectMaterials;
        [SerializeField]
        public int objectCounts;

        public void Init()
        {
            selectMaterials = 0;
            objectCounts = 1000;
        }
    }
}

