using HarmonyLib;
using OnTheCase.Extensions;
namespace OnTheCase.Patches
{
    [HarmonyPatch(typeof(PlayerDataZip))]
    public class PlayerDataZipPatches
    {
        [HarmonyPatch(nameof(PlayerDataZip.CurrentCustomizationDataIDs), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool BufferData(PlayerDataZip __instance, ref CustomizationDataIDs2 value)
        {
            if (!ModDataController.loaded)
            {
                return true;
            }
            ModDataController.SetBufferData(__instance.SelectedStyleIndex, value.Copy());
            CustomizationDataIDs2? lastVanilla = __instance.SelectedStyleIndex switch
            {
                0 => __instance.CustomizationDataIDsNew,
                1 => __instance.CustomizationDataIDs1New,
                2 => __instance.CustomizationDataIDs2New,
                _ => null,
            };
            if (lastVanilla.HasValue)
            {
                value = value.ReplaceModded(lastVanilla.Value);
            }
            return true;
        }
        [HarmonyPatch(nameof(PlayerDataZip.CurrentCustomizationDataIDs), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool GetBufferData(PlayerDataZip __instance, ref CustomizationDataIDs2 __result)
        {
            if (!ModDataController.loaded || ModDataController.saving)
            {
                return true;
            }
            CustomizationDataIDs2? buffer = ModDataController.GetBufferData(__instance.SelectedStyleIndex);
            if (buffer.HasValue)
            {
                __result = buffer.Value;
                return false;
            }
            return true;
        }
    }
}