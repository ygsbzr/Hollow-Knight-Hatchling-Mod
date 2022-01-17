using System;

namespace Hatchling
{

    [Serializable]
    public class GlobalModSettings
    {
        public bool attackOption = true;
        public int hatchlingType = 3;
        public int maxCount = 3*2;
        public int charmCost = 2;
        public float hatchTime = 4f;
    }
}
