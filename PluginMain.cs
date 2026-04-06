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
        //temp
        internal List<CustomCosmetic> tempcosmetics = new List<CustomCosmetic>();
        //temp
        void Awake()
        {
            InitializeMethods();
            Instance ??= this;
            Log = Logger;
            DataLocation = SaveLocation();
            ModConfig = new CaseConfig(Config);
            GetDummy();
            //temp
            for (int i = 0; i < tempcosmetics.Count; i++)
            {
                CaseUtils.RegisterCosmetic(Info, tempcosmetics[i]);
            }
            //temp
            HandleHarmony();
            Log.LogInfo($"{modName} successfully loaded");
        }
        static void InitializeMethods()
        {
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                MethodInfo[] typeMethods = assemblyTypes[i].GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                for (int j = 0; j < typeMethods.Length; j++)
                {
                    object[] methodAttributes;
                    try
                    {
                        methodAttributes = typeMethods[j].GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    }
                    catch
                    {
                        continue;
                    }
                    if (methodAttributes.Length > 0)
                    {
                        typeMethods[j].Invoke(null, null);
                    }
                }
            }
        }
        string SaveLocation()
        {
            return Path.Combine(Directory.GetParent(ConstData.PlayerZipSavePath).FullName, "CaseData");
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
                    //temp
                    case "assets/onthecase/cosmetics":
                        {
                            try
                            {
                                tempcosmetics.Add(bundle.LoadAsset<CustomCosmetic>(allAssetPaths[i]));
                            }
                            catch
                            {
                                continue;
                            }
                            break;
                        }
                    //temp
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