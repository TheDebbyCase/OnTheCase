using HarmonyLib;
using Newtonsoft.Json;
using OnTheCase.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
namespace OnTheCase.Patches
{
    [HarmonyPatch(typeof(DataManager))]
    public class DataManagerPatches
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static bool AddCustomCustomizations(DataManager __instance)
        {
            CustomizationSettings settings = ScriptableSingleton<CustomizationSettings>.I;
            List<AppearanceGroup> appearanceGroups = settings.AppearanceGroups;
            List<AppearanceGroupForSprite> appearanceSpriteGroups = settings.AppearanceSpriteGroups;
            List<OutfitGroup> outfitGroups = settings.OutfitGroups;
            for (int i = 0; i < appearanceGroups.Count; i++)
            {
                for (int j = 0; j < appearanceGroups[i].CustomizationOptions.Count; j++)
                {
                    CaseUtils.vanillaIDs.Add(appearanceGroups[i].CustomizationOptions[j].ID);
                }
            }
            for (int i = 0; i < appearanceSpriteGroups.Count; i++)
            {
                for (int j = 0; j < appearanceSpriteGroups[i].CustomizationOptions.Count; j++)
                {
                    CaseUtils.vanillaIDs.Add(appearanceSpriteGroups[i].CustomizationOptions[j].ID);
                }
            }
            for (int i = 0; i < outfitGroups.Count; i++)
            {
                for (int j = 0; j < outfitGroups[i].CustomizationOptions.Count; j++)
                {
                    CaseUtils.vanillaIDs.Add(outfitGroups[i].CustomizationOptions[j].ID);
                }
            }
            for (int i = 0; i < appearanceGroups.Count; i++)
            {
                AppearanceGroup group = appearanceGroups[i];
                List<CustomAppearance> appearances = CaseMod.Instance.customAppearances[group.AppearanceType];
                List<CustomizationOption> options = new List<CustomizationOption>();
                for (int j = 0; j < appearances.Count; j++)
                {
                    CustomAppearance appearance = appearances[j];
                    int id = -1;
                    if (ModDataController.moddedData.TryGetValue(appearance.cosmeticName, out ModdedCustomizationData cosmeticData))
                    {
                        id = cosmeticData.targetID;
                    }
                    int next = CaseUtils.NextAvailableID();
                    if (id < next)
                    {
                        id = next;
                    }
                    string stringID = string.Join('.', appearance.modGUID, appearance.cosmeticName);
                    ModDataController.NewCosmeticData(stringID, id, appearance.type);
                    CustomizationOption? option = CaseUtils.AppearanceToOption(appearance, id);
                    if (option.HasValue)
                    {
                        options.Add(option.Value);
                        CaseUtils.moddedIDs.Add(id, stringID);
                    }
                }
                group.CustomizationOptions.AddRange(options);
            }
            Traverse.Create(__instance).Field("appearanceGroups").SetValue(appearanceGroups);
            for (int i = 0; i < appearanceSpriteGroups.Count; i++)
            {
                AppearanceGroupForSprite group = appearanceSpriteGroups[i];
                List<CustomAppearance> sprites = CaseMod.Instance.customSpriteAppearances[group.AppearanceType];
                List<CustomizationOptionForSprite> options = new List<CustomizationOptionForSprite>();
                for (int j = 0; j < sprites.Count; j++)
                {
                    CustomAppearance sprite = sprites[j];
                    int id = -1;
                    if (ModDataController.moddedData.TryGetValue(sprite.cosmeticName, out ModdedCustomizationData cosmeticData))
                    {
                        id = cosmeticData.targetID;
                    }
                    int next = CaseUtils.NextAvailableID();
                    if (id < next)
                    {
                        id = next;
                    }
                    string stringID = string.Join('.', sprite.modGUID, sprite.cosmeticName);
                    ModDataController.NewCosmeticData(stringID, id, sprite.type);
                    CustomizationOptionForSprite? option = CaseUtils.SpriteToOption(sprite, id);
                    if (option.HasValue)
                    {
                        options.Add(option.Value);
                        CaseUtils.moddedIDs.Add(id, stringID);
                    }
                }
                group.CustomizationOptions.AddRange(options);
            }
            Traverse.Create(__instance).Field("appearanceSpriteGroups").SetValue(appearanceSpriteGroups);
            for (int i = 0; i < outfitGroups.Count; i++)
            {
                OutfitGroup group = outfitGroups[i];
                List<CustomOutfit> outfits = CaseMod.Instance.customOutfits[group.OutfitType];
                List<CustomizationOption> options = new List<CustomizationOption>();
                for (int j = 0; j < outfits.Count; j++)
                {
                    CustomOutfit outfit = outfits[j];
                    int id = -1;
                    if (ModDataController.moddedData.TryGetValue(outfit.cosmeticName, out ModdedCustomizationData cosmeticData))
                    {
                        id = cosmeticData.targetID;
                    }
                    int next = CaseUtils.NextAvailableID();
                    if (id < next)
                    {
                        id = next;
                    }
                    string stringID = string.Join('.', outfit.modGUID, outfit.cosmeticName);
                    ModDataController.NewCosmeticData(stringID, id, outfit.type);
                    CustomizationOption? option = CaseUtils.OutfitToOption(outfit, id);
                    if (option.HasValue)
                    {
                        options.Add(option.Value);
                        CaseUtils.moddedIDs.Add(id, stringID);
                    }
                }
                group.CustomizationOptions.AddRange(options);
            }
            Traverse.Create(__instance).Field("_outfitGroups").SetValue(outfitGroups);
            return true;
        }
        [HarmonyPatch(nameof(DataManager.SavePlayerZipData))]
        [HarmonyPrefix]
        public static bool SaveModdedData()
        {
            ModDataController.saving = true;
            if (!ModDataController.loaded)
            {
                return true;
            }
            if (!Directory.Exists(CaseMod.DataLocation))
            {
                Directory.CreateDirectory(CaseMod.DataLocation);
            }
            StreamWriter? writer = null;
            try
            {
                writer = File.CreateText(Path.Combine(CaseMod.DataLocation, "data.json"));
                writer.Write(JsonConvert.SerializeObject(ModDataController.moddedData));
            }
            catch (Exception exception)
            {
                CaseMod.Instance.Log.LogError("Failed to save modded data!");
                CaseMod.Instance.Log.LogError(exception);
            }
            finally
            {
                writer?.Dispose();
            }
            return true;
        }
        [HarmonyPatch(nameof(DataManager.SavePlayerZipData))]
        [HarmonyPostfix]
        public static void ResumeBuffering()
        {
            ModDataController.saving = false;
        }
        [HarmonyPatch("LoadPlayerData")]
        [HarmonyPostfix]
        public static void LoadModdedData(DataManager __instance)
        {
            ModDataController.FillModdedData(CaseUtils.LoadData());
            __instance.CustomizationData = new CustomizationData(__instance.PlayerDataZip.CurrentCustomizationDataIDs);
        }
    }
    [HarmonyPatch(typeof(DataManager))]
    public class DataManagerLoadPlayerDataPatch
    {
        [HarmonyPatch("LoadPlayerData")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching LoadPlayerData");
            int foundCount = 0;
            MethodInfo targetMethod = AccessTools.Method(typeof(PlayerDataZip), "get_CurrentCustomizationDataIDs");
            FieldInfo targetField = AccessTools.Field(typeof(PlayerDataZip), nameof(PlayerDataZip.IsCustomizationBought));
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(targetMethod))
                {
                    codes.RemoveRange(i - 3, 6);
                }
                if (codes[i].LoadsField(targetField))
                {
                    foundCount++;
                    codes[i + 10] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(CaseUtils), nameof(CaseUtils.StartBought), new Type[] { typeof(int) }));
                    codes.RemoveAt(i + 9);
                    codes.RemoveRange(i - 1, 2);
                    if (foundCount >= 3)
                    {
                        break;
                    }
                }
            }
            return codes.AsEnumerable();
        }
    }
}