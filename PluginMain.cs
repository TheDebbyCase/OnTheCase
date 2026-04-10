using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using HarmonyLib;
using OnTheCase.Config;
using OnTheCase.Utils;
using System.Collections.Generic;
using System.IO;
using System;
namespace OnTheCase
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CaseMod : BaseUnityPlugin
    {
        internal const string modGUID = "deB.CaseMod";
        internal const string modName = "On-The Case";
        internal const string modVersion = "0.0.1";
        public static string DataLocation { get; private set; } = null!;
        internal static CaseMod Instance { get; private set; } = null!;
        internal readonly Harmony harmony = new Harmony(modGUID);
        internal ManualLogSource Log { get; private set; } = null!;
        internal CaseConfig ModConfig { get; private set; } = null!;
        internal readonly Dictionary<AppearanceType, List<CustomAppearance>> customAppearances = new Dictionary<AppearanceType, List<CustomAppearance>>()
        {
            [AppearanceType.Head] = new List<CustomAppearance>(),
            [AppearanceType.Hair] = new List<CustomAppearance>(),
            [AppearanceType.Mustache] = new List<CustomAppearance>()
        };
        internal readonly Dictionary<AppearanceType, List<CustomAppearance>> customSpriteAppearances = new Dictionary<AppearanceType, List<CustomAppearance>>()
        {
            [AppearanceType.Eye] = new List<CustomAppearance>(),
            [AppearanceType.Eyebrow] = new List<CustomAppearance>(),
            [AppearanceType.Facial] = new List<CustomAppearance>(),
            [AppearanceType.Mouth] = new List<CustomAppearance>()
        };
        internal readonly Dictionary<OutfitType, List<CustomOutfit>> customOutfits = new Dictionary<OutfitType, List<CustomOutfit>>()
        {
            [OutfitType.Backpack] = new List<CustomOutfit>(),
            [OutfitType.Bottom] = new List<CustomOutfit>(),
            [OutfitType.EyeGlass] = new List<CustomOutfit>(),
            [OutfitType.Feet] = new List<CustomOutfit>(),
            [OutfitType.FullBody] = new List<CustomOutfit>(),
            [OutfitType.Hands] = new List<CustomOutfit>(),
            [OutfitType.Hats] = new List<CustomOutfit>(),
            [OutfitType.Shoes] = new List<CustomOutfit>(),
            [OutfitType.Tail] = new List<CustomOutfit>(),
            [OutfitType.Top] = new List<CustomOutfit>(),
            [OutfitType.Umbrella] = new List<CustomOutfit>()
        };
        public GameObject Dummy { get; private set; } = null!;
        void Awake()
        {
            Instance ??= this;
            Log = Logger;
            DataLocation = SaveLocation();
            ModConfig = new CaseConfig(Config);
            GetDummy();
            RegisterFromBundles();
            HandleHarmony();
            Log.LogInfo($"{modName} successfully loaded");
        }
        string SaveLocation()
        {
            return Path.Combine(Directory.GetParent(ConstData.PlayerZipSavePath).FullName, "CaseData");
        }
        void RegisterFromBundles()
        {
            string[] filePaths = Directory.GetFiles(Paths.PluginPath, "*.case", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                string path = filePaths[i];
                try
                {
                    string modID = path[path.LastIndexOf('/')..];
                    AssetBundle bundle = AssetBundle.LoadFromFile(path);
                    CustomCosmetic[] customCosmetics = bundle.LoadAllAssets<CustomCosmetic>();
                    for (int j = 0; j < customCosmetics.Length; j++)
                    {
                        CosmeticUtils.RegisterCosmetic(modID, customCosmetics[j]);
                    }
                }
                catch (Exception exception)
                {
                    Log.LogError($"Failed to load asset bundle at path \"{path}\"");
                    Log.LogError(exception);
                    continue;
                }
            }
        }
        void GetDummy()
        {
            string? assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (assemblyPath is null)
            {
                Log.LogError($"Could not find assembly directory!");
                return;
            }
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(assemblyPath, "onthecase"));
            string[] allAssetPaths = bundle.GetAllAssetNames();
            for (int i = 0; i < allAssetPaths.Length; i++)
            {
                string assetType = allAssetPaths[i][..allAssetPaths[i].LastIndexOf('/')];
                switch (assetType)
                {
                    case "assets/onthecase/dummy":
                        {
                            try
                            {
                                Dummy = bundle.LoadAsset<GameObject>(allAssetPaths[i]);
                            }
                            catch
                            {
                                continue;
                            }
                            break;
                        }
                    default:
                        {
                            Log.LogWarning($"\"{assetType}\" is not a known asset path, skipping.");
                            break;
                        }
                }
            }
        }
        void HandleHarmony()
        {
            harmony.PatchAll();
        }
    }
}