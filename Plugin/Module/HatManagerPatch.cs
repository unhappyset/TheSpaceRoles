﻿using Cpp2IL.Core.Extensions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSpaceRoles
{
    [HarmonyPatch(typeof(HatManager))]
    internal static class HatManagerPatches
    {
        private static bool isRunning;
        private static bool isLoaded;
        private static List<HatData> allHats;

        [HarmonyPatch(nameof(HatManager.GetHatById))]
        [HarmonyPrefix]
        private static void GetHatByIdPrefix(HatManager __instance)
        {
            if (isRunning || isLoaded) return;
            isRunning = true;
            // Maybe we can use lock keyword to ensure simultaneous list manipulations ?
            allHats = __instance.allHats.ToList();
            var cache = CustomHatManager.UnregisteredHats.Clone();
            foreach (var hat in cache)
            {
                try
                {
                    allHats.Add(CustomHatManager.CreateHatBehaviour(hat.Item1));
                    CustomHatManager.UnregisteredHats.Remove(hat);
                }
                catch
                {
                    // This means the file has not been downloaded yet, do nothing...
                }
            }
            if (CustomHatManager.UnregisteredHats.Count == 0)
                isLoaded = true;
            cache.Clear();

            __instance.allHats = allHats.ToArray();
        }

        [HarmonyPatch(nameof(HatManager.GetHatById))]
        [HarmonyPostfix]
        private static void GetHatByIdPostfix()
        {
            isRunning = false;
        }
    }
}