using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFETribalsSearch
{
    public class Mod: Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "VFETribalsSearch.LoadingLabel", doAsynchronously: true, null);
        }
        private void Init()
        {
            new Harmony("rimworld.sk.vfetribalsearch").PatchAll();
        }
    }
}
