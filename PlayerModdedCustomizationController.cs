using OnTheCase.Extensions;
using OnTheCase.Utils;
using PurrNet;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace OnTheCase
{
    internal static class ModDataController
    {
        internal static bool saving = false;
        internal static bool loaded = false;
        internal static Dictionary<int, CustomizationDataIDs2> presetBuffer = new Dictionary<int, CustomizationDataIDs2>();
        internal static Dictionary<ulong, IDKeys> playerIDKeys = new Dictionary<ulong, IDKeys>();
        internal static Dictionary<string, ModdedCustomizationData> moddedData = new Dictionary<string, ModdedCustomizationData>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong SetIDKeyRPC(IDKeys keys, RPCInfo rpcInfo = default)
        {
            ulong steamID = CaseUtils.SteamIDFromPlayerID(rpcInfo.sender);
            AddOrSetIDKey(steamID, keys);
            return steamID;
        }
        internal static void AddOrSetIDKey(ulong steamID, IDKeys keys)
        {
            CaseMod.Instance.Log.LogDebug($"Setting IDKeys for player {steamID}");
            if (!playerIDKeys.TryAdd(steamID, keys))
            {
                playerIDKeys[steamID] = keys;
            }
        }
        internal static IDKeys AddOrSetLocalIDKey()
        {
            CaseMod.Instance.Log.LogDebug("Running AddOrSetLocalIDKey");
            ulong steamID = SteamUser.GetSteamID().m_SteamID;
            CaseMod.Instance.Log.LogDebug("Got Steam ID");
            IDKeys keys = new IDKeys { pairs = new Dictionary<string, int>() };
            foreach (KeyValuePair<string, ModdedCustomizationData> pair in moddedData)
            {
                keys.pairs.Add(pair.Key, pair.Value.targetID);
            }
            AddOrSetIDKey(steamID, keys);
            return keys;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyIDKey(ulong steamID, ref CustomizationData data)
        {
            ulong localSteamID = SteamUser.GetSteamID().m_SteamID;
            Dictionary<string, int> localKey = playerIDKeys[localSteamID].pairs;
            Dictionary<string, int> foreignKey = playerIDKeys[steamID].pairs;
            foreach (KeyValuePair<string, int> cosmetic in foreignKey)
            {
                if (!localKey.ContainsKey(cosmetic.Key))
                {
                    continue;
                }
                int localID = localKey[cosmetic.Key];
                int foreignID = foreignKey[cosmetic.Key];
                if (localID != foreignID)
                {
                    ShapeFromTo(foreignID, localID, ref data);
                }
                else
                {
                    CaseMod.Instance.Log.LogDebug($"IDs for Cosmetic \"{cosmetic.Key}\" were the same! ({localID})");
                }
            }
        }
        internal static void ShapeFromTo(int from, int to, ref CustomizationData data)
        {
            AppearanceType[] appearanceTypes = (AppearanceType[])Enum.GetValues(typeof(AppearanceType));
            for (int i = 0; i < appearanceTypes.Length; i++)
            {
                AppearanceType type = appearanceTypes[i];
                if (data.GetShapeIndex(CustomizationType.Appearance, (int)type) == from)
                {
                    CaseMod.Instance.Log.LogDebug($"Changing {type} from {from} to {to}");
                    data.SetShapeIndex(CustomizationType.Appearance, (int)type, to);
                }
            }
            OutfitType[] outfitTypes = (OutfitType[])Enum.GetValues(typeof(OutfitType));
            for (int i = 0; i < outfitTypes.Length; i++)
            {
                OutfitType type = outfitTypes[i];
                if (data.GetShapeIndex(CustomizationType.Outfit, (int)type) == from)
                {
                    CaseMod.Instance.Log.LogDebug($"Changing {type} from {from} to {to}");
                    data.SetShapeIndex(CustomizationType.Outfit, (int)type, to);
                }
            }
        }
        internal static void NewCosmeticData(string name, int id, AppearanceType type)
        {
            if (!moddedData.TryGetValue(name, out ModdedCustomizationData data))
            {
                data = new ModdedCustomizationData(id, CaseUtils.GetCosmeticType(type));
            }
            else
            {
                data.targetID = id;
            }
            ReplaceOrAddCosmeticData(name, data);
        }
        internal static void NewCosmeticData(string name, int id, OutfitType type)
        {
            if (!moddedData.TryGetValue(name, out ModdedCustomizationData data))
            {
                data = new ModdedCustomizationData(id, CaseUtils.GetCosmeticType(type));
            }
            else
            {
                data.targetID = id;
            }
            ReplaceOrAddCosmeticData(name, data);
        }
        internal static void ReplaceOrAddCosmeticData(string name, ModdedCustomizationData data)
        {
            if (!moddedData.TryAdd(name, data))
            {
                moddedData[name] = data;
            }
        }
        internal static void EditCosmeticID(string name, int id)
        {
            if (!moddedData.ContainsKey(name))
            {
                return;
            }
            moddedData[name].SetID(id);
        }
        internal static void EditCosmeticBought(string name, bool bought)
        {
            if (!moddedData.ContainsKey(name))
            {
                return;
            }
            moddedData[name].SetBought(bought);
        }
        internal static bool GetCosmeticBought(string name)
        {
            if (!moddedData.ContainsKey(name))
            {
                return false;
            }
            return moddedData[name].bought;
        }
        internal static void AddCosmeticPreset(string name, int preset, List<int> colours)
        {
            if (!moddedData.ContainsKey(name))
            {
                return;
            }
            moddedData[name].SetPresetColours(preset, colours);
        }
        internal static void RemoveCosmeticPreset(string name, int preset)
        {
            if (!moddedData.ContainsKey(name))
            {
                return;
            }
            moddedData[name].RemovePreset(preset);
        }
        internal static void AddOrReplaceBuffer(int preset, CustomizationDataIDs2 data)
        {
            if (!presetBuffer.TryAdd(preset, data))
            {
                presetBuffer[preset] = data;
            }
        }
        internal static void SetBufferData(int preset, CustomizationDataIDs2 data)
        {
            if (MonoSingleton<DataManager>.I.PlayerDataZip is null)
            {
                return;
            }
            AddOrReplaceBuffer(preset, data);
            Dictionary<string, ModdedCustomizationData> newData = new Dictionary<string, ModdedCustomizationData>(moddedData);
            foreach (KeyValuePair<string, ModdedCustomizationData> pair in newData)
            {
                if (CaseUtils.IsIDPresent(data, pair.Value.targetID, out int index))
                {
                    AddCosmeticPreset(pair.Key, preset, CaseUtils.GetColoursByIndex(data, index));
                }
                else
                {
                    RemoveCosmeticPreset(pair.Key, preset);
                }
            }
        }
        internal static CustomizationDataIDs2? GetBufferData(int preset)
        {
            if (presetBuffer.TryGetValue(preset, out CustomizationDataIDs2 result))
            {
                return result;
            }
            return null;
        }
        internal static void FillModdedData(Dictionary<string, ModdedCustomizationData>? loadedData = null)
        {
            if (MonoSingleton<DataManager>.I.PlayerDataZip is null)
            {
                CaseMod.Instance.Log.LogError("Failed to fill modded data! PlayerDataZip was null!");
                return;
            }
            if (loadedData != null)
            {
                foreach (KeyValuePair<string, ModdedCustomizationData> pair in loadedData)
                {
                    ReplaceOrAddCosmeticData(pair.Key, pair.Value);
                }
            }
            foreach (KeyValuePair<string, ModdedCustomizationData> pair in moddedData)
            {
                if (pair.Value.colourIndices.TryGetValue(0, out List<int> colours0))
                {
                    if (!presetBuffer.ContainsKey(0))
                    {
                        AddOrReplaceBuffer(0, MonoSingleton<DataManager>.I.PlayerDataZip.CustomizationDataIDsNew.Copy());
                    }
                    CustomizationDataIDs2 presetData = presetBuffer[0];
                    CaseUtils.SetShapeInData(ref presetData, (int)pair.Value.cosmeticType, pair.Value.targetID, colours0);
                    AddOrReplaceBuffer(0, presetData);
                }
                if (pair.Value.colourIndices.TryGetValue(1, out List<int> colours1))
                {
                    if (!presetBuffer.ContainsKey(1))
                    {
                        AddOrReplaceBuffer(1, MonoSingleton<DataManager>.I.PlayerDataZip.CustomizationDataIDs1New.Copy());
                    }
                    CustomizationDataIDs2 presetData = presetBuffer[1];
                    CaseUtils.SetShapeInData(ref presetData, (int)pair.Value.cosmeticType, pair.Value.targetID, colours1);
                    AddOrReplaceBuffer(1, presetData);
                }
                if (pair.Value.colourIndices.TryGetValue(2, out List<int> colours2))
                {
                    if (!presetBuffer.ContainsKey(2))
                    {
                        AddOrReplaceBuffer(2, MonoSingleton<DataManager>.I.PlayerDataZip.CustomizationDataIDs2New.Copy());
                    }
                    CustomizationDataIDs2 presetData = presetBuffer[2];
                    CaseUtils.SetShapeInData(ref presetData, (int)pair.Value.cosmeticType, pair.Value.targetID, colours2);
                    AddOrReplaceBuffer(2, presetData);
                }
            }
            loaded = true;
        }
    }
}