using HarmonyLib;
using OnTheCase.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
namespace OnTheCase.Patches
{
    [HarmonyPatch(typeof(CustomizationUIController))]
    public class CustomizationUIControllerButtonBuyPatch
    {
        [HarmonyPatch(nameof(CustomizationUIController.ButtonBuy))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(CustomizationUIController.ButtonBuy)}");
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            MethodInfo target = AccessTools.Method(typeof(Dictionary<int, bool>), "set_Item", new Type[] { typeof(int), typeof(bool) });
            MethodInfo newMethod = AccessTools.Method(typeof(CosmeticUtils), nameof(CosmeticUtils.SetBought), new Type[] { typeof(int), typeof(bool) });
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    codes[i] = new CodeInstruction(OpCodes.Callvirt, newMethod);
                    codes.RemoveRange(i - 11, 3);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(CustomizationUIController))]
    public class CustomizationUIControllerButtonItemPatch
    {
        [HarmonyPatch(nameof(CustomizationUIController.ButtonItem))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(CustomizationUIController.ButtonItem)}");
            MethodInfo target = AccessTools.Method(typeof(Dictionary<int, bool>), "get_Item", new Type[] { typeof(int) });
            MethodInfo newMethod = AccessTools.Method(typeof(CosmeticUtils), nameof(CosmeticUtils.IsBought), new Type[] { typeof(int) });
            bool firstDone = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    codes[i] = new CodeInstruction(OpCodes.Callvirt, newMethod);
                    codes.RemoveRange(i - 5, 3);
                    if (firstDone)
                    {
                        break;
                    }
                    firstDone = true;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(CustomizationUIController))]
    public class CustomizationUIControllerSetItemButtonsUIPatch
    {
        [HarmonyPatch("SetItemButtonsUI")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching SetItemButtonsUI");
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            MethodInfo target = AccessTools.Method(typeof(Dictionary<int, bool>), "get_Item", new Type[] { typeof(int) });
            MethodInfo newMethod = AccessTools.Method(typeof(CosmeticUtils), nameof(CosmeticUtils.IsBought), new Type[] { typeof(int) });
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    codes[i] = new CodeInstruction(OpCodes.Callvirt, newMethod);
                    codes.RemoveRange(i - 7, 3);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(CustomizationUIController))]
    public class CustomizationUIControllerSetItemForSpriteButtonsUIPatch
    {
        [HarmonyPatch("SetItemForSpriteButtonsUI")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching SetItemForSpriteButtonsUI");
            MethodInfo target = AccessTools.Method(typeof(Dictionary<int, bool>), "get_Item", new Type[] { typeof(int) });
            MethodInfo newMethod = AccessTools.Method(typeof(CosmeticUtils), nameof(CosmeticUtils.IsBought), new Type[] { typeof(int) });
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    codes[i] = new CodeInstruction(OpCodes.Callvirt, newMethod);
                    codes.RemoveRange(i - 7, 3);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}