using System;
using Modding;

namespace Hatchling
{
    [Serializable]
    public class SaveModSettings : ModSettings
    {

    }

    [Serializable]
    public class GlobalModSettings : ModSettings
    {
        public bool attackOption = true;
        public int hatchlingType = 3;
        public int maxCount = 3*2;
        public int charmCost = 2;
    }
}
