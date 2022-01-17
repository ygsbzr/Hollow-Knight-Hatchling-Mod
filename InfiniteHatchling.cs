using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HutongGames.PlayMaker.Actions;
using Modding;
using UnityEngine;
using Vasi;
namespace Hatchling
{

    public class InfiniteHatchling : Mod,ITogglableMod,IGlobalSettings<GlobalModSettings>
    {
        public static readonly string DATA_DIR = Path.GetFullPath(Application.dataPath + "/Managed/Mods/Hatchlings");
        private readonly Dictionary<string, Texture2D> hatchlingTex = new Dictionary<string, Texture2D>();
        private int _selector;
        public int hatchlingSelector
        {
            get
            {
                _selector = (_selector + 1) % Math.Abs(Settings.hatchlingType);
                return _selector + 1;
            }

        }
        public override void Initialize()
        {
            ModHooks.AfterSavegameLoadHook += ModifyHatchling;
            ModHooks.ObjectPoolSpawnHook += Instance_ObjectPoolSpawnHook;
            On.KnightHatchling.Start += KnightHatchling_Start;
            ModHooks.SetPlayerIntHook += ModifyCharmCost;

            _selector = -1;
            Log($"Init Done,atk:{Settings.attackOption},type:{Settings.hatchlingType},max:{Settings.maxCount},cost:{Settings.charmCost}");
        }

        private int ModifyCharmCost(string name, int orig)
        {
           if(name==nameof(PlayerData.instance.charmCost_22))
            {
                return Settings.charmCost;
            }
            else
            {
                return orig;
            }
        }

        private Texture2D LoadTex(string path)
        {
            if (!File.Exists(path))
            {
                LogDebug($"File:{path} is Not Found");
                return null;
            }
            if(hatchlingTex.ContainsKey(path))
            {
                return hatchlingTex[path];
            }
            else
            {
                byte[] texBytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(texBytes, true);
                hatchlingTex.Add(path, tex);
                return tex;
            }
            
        }

        private void KnightHatchling_Start(On.KnightHatchling.orig_Start orig, KnightHatchling self)
        {
            orig(self);
            if( !Settings.attackOption ) //remove this can let sommoned lose goal and prevent attacking
                self.enemyRange = null;

            //if you want to let them attack but not die, you can block the "Land" fsm action.
            //if you want to let them attack without damage, you can remove it's component "Damager"
            return;
        }

        private GameObject Instance_ObjectPoolSpawnHook(GameObject go) 
        {
            if (go.tag == "Knight Hatchling")
            {
                Texture2D tex = LoadTex((DATA_DIR + "/" + $"h{hatchlingSelector}.png").Replace("\\", "/"));
                if (tex)
                {
                    var materialProp = new MaterialPropertyBlock();
                    go.GetComponent<MeshRenderer>().GetPropertyBlock(materialProp);
                    materialProp.SetTexture("_MainTex", tex);
                    go.GetComponent<MeshRenderer>().SetPropertyBlock(materialProp);
                }
            }
            return go;
        }

        private void ModifyHatchling(SaveGameData data)
        {
            GameManager.instance.StartCoroutine(HeroFinder());
        }
        private IEnumerator HeroFinder() 
        {
            yield return new WaitWhile(()=>HeroController.instance == null);

            var ce = GameObject.Find("Charm Effects");
            //var ce = HeroController.instance.transform.FindChild("Charm Effects").gameObject;
            var hatchlingstate = ce.LocateMyFSM("Hatchling Spawn").GetState("Equipped");
 
            hatchlingstate.InsertAction(0, new SetIntValue { intVariable = hatchlingstate.Fsm.GetFsmInt("Hatchling Max"), intValue = Math.Abs(Settings.maxCount), everyFrame = false });
            hatchlingstate.InsertAction(1, new SetFloatValue { floatVariable = hatchlingstate.Fsm.GetFsmFloat("Hatch Time"), floatValue = Math.Abs(Settings.hatchTime), everyFrame = false });

            LogDebug("Modify Hatchling MaxCount and Time");
            
            
        }
        public void Unload()
        {
            ModHooks.AfterSavegameLoadHook -= ModifyHatchling;
            ModHooks.ObjectPoolSpawnHook -= Instance_ObjectPoolSpawnHook;
            On.KnightHatchling.Start -= KnightHatchling_Start;
            ModHooks.GetPlayerIntHook -= ModifyCharmCost;
        }
        public GlobalModSettings OnSaveGlobal()
        {
            return Settings;
        }
        public void OnLoadGlobal(GlobalModSettings s)
        {
            Settings = s;
        }
        public override string GetVersion()
        {
            return "1.0";
            /*
            Assembly asm = Assembly.GetExecutingAssembly();

            string ver = asm.GetName().Version.ToString();

            using SHA1 sha1 = SHA1.Create();
            using FileStream stream = File.OpenRead(asm.Location);

            byte[] hashBytes = sha1.ComputeHash(stream);

            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return $"{ver}-{hash.Substring(0, 6)}";*/
        }

        public GlobalModSettings Settings = new GlobalModSettings();
        
    }
}
