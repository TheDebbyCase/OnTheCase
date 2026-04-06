using BepInEx;
using Newtonsoft.Json;
using PurrNet;
using PurrNet.Packing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
namespace OnTheCase.Utils
{
    public static class CaseUtils
    {
        internal static readonly HashSet<int> vanillaIDs = new HashSet<int>();
        public static HashSet<int> VanillaIDs
        {
            get
            {
                return new HashSet<int>(vanillaIDs);
            }
        }
        internal static readonly Dictionary<int, string> moddedIDs = new Dictionary<int, string>();
        public static Dictionary<int, string> ModdedIDs
        {
            get
            {
                return new Dictionary<int, string>(moddedIDs);
            }
        }
        static Dictionary<string, Shader> _onToShaders = null!;
        public static Dictionary<string, Shader> OnToShaders
        {
            get
            {
                if (_onToShaders != null)
                {
                    return new Dictionary<string, Shader>(_onToShaders);
                }
                _onToShaders = new Dictionary<string, Shader>();
                List<string> knownShaders = new List<string>() { "Toon", "Shader Graphs/TransparentShader" };
                for (int i = 0; i < knownShaders.Count; i++)
                {
                    Shader shader = Shader.Find(knownShaders[i]);
                    if (shader != null)
                    {
                        _onToShaders.Add(knownShaders[i], shader);
                    }
                    else
                    {
                        CaseMod.Instance.Log.LogWarning($"Shader with name \"{knownShaders[i]}\" could not be found!");
                    }
                }
                return new Dictionary<string, Shader>(_onToShaders);
            }
        }
        static readonly Dictionary<string, string> fromToShader = new Dictionary<string, string>()
        {
            ["Toon/Toon"] = "Toon"
        };
        public static void FixShader(Material material)
        {
            string shaderName = material.shader.name;
            if (fromToShader.TryGetValue(shaderName, out string newName))
            {
                shaderName = newName;
            }
            if (!OnToShaders.TryGetValue(shaderName, out Shader? shader))
            {
                shader = Shader.Find(material.shader.name);
            }
            if (shader != null)
            {
                material.shader = shader;
                _onToShaders?.TryAdd(material.shader.name, shader);
            }
        }
        public static bool RegisterCosmetic(PluginInfo plugin, CustomCosmetic cosmetic)
        {
            CaseMod.Instance.Log.LogInfo($"Registering cosmetic with name {cosmetic.cosmeticName}!");
            bool success = true;
            try
            {
                if (cosmetic.cosmeticName is null || cosmetic.cosmeticName == string.Empty)
                {
                    CaseMod.Instance.Log.LogWarning($"Cosmetic from mod: \"{plugin.Metadata.Name}\" did not have a name!");
                    return false;
                }
                cosmetic.modGUID = plugin.Metadata.GUID;
                cosmetic.FixShaders();
                if (cosmetic.autoWeight)
                {
                    cosmetic.AutoWeight();
                }
                switch (cosmetic)
                {
                    case CustomAppearance appearance:
                        {
                            if (appearance.type is AppearanceType.Head || appearance.type is AppearanceType.Hair || appearance.type is AppearanceType.Mustache)
                            {
                                CaseMod.Instance.customAppearances[appearance.type].Add(appearance);
                            }
                            else
                            {
                                CaseMod.Instance.customSpriteAppearances[appearance.type].Add(appearance);
                            }
                            break;
                        }
                    case CustomOutfit outfit:
                        {
                            CaseMod.Instance.customOutfits[outfit.type].Add(outfit);
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"Custom cosmetic type \"{cosmetic.GetType()}\" from plugin \"{plugin.Metadata.Name}\" was unknown!");
                            success = false;
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                CaseMod.Instance.Log.LogError($"Failed to register cosmetic with name {cosmetic.cosmeticName}!");
                CaseMod.Instance.Log.LogError(exception);
                success = false;
            }
            return success;
        }
        public static bool IsIDPresent(CustomizationData data, int id, out int index)
        {
            index = -1;
            bool result = false;
            object[] shapeColourData = GetCustomizationsAsArray(data);
            for (int i = 1; i < shapeColourData.Length; i++)
            {
                result = (shapeColourData[i] is ShapeAndColorData colourData && colourData.ShapeIndex == id) || (shapeColourData[i] is ShapeAndColorsData coloursData && coloursData.ShapeIndex == id);
                if (result)
                {
                    index = i;
                    break;
                }
            }
            return result;
        }
        public static bool IsIDPresent(CustomizationDataIDs2 data, int id, out int index)
        {
            index = -1;
            bool result = false;
            object[] shapeColourData = GetCustomizationsAsArray(data);
            for (int i = 1; i < shapeColourData.Length; i++)
            {
                result = (shapeColourData[i] is ShapeAndColorData colourData && colourData.ShapeIndex == id) || (shapeColourData[i] is ShapeAndColorsData coloursData && coloursData.ShapeIndex == id);
                if (result)
                {
                    index = i;
                    break;
                }
            }
            return result;
        }
        public static List<int> GetColoursByIndex(CustomizationData data, int index)
        {
            List<int> result = new List<int>();
            object[] shapeColourData = GetCustomizationsAsArray(data);
            if (index < 0 || index >= shapeColourData.Length)
            {
                return result;
            }
            if (shapeColourData[index] is ShapeAndColorData colourData)
            {
                result = new List<int>() { colourData.ColorIndex };
            }
            else if (shapeColourData[index] is ShapeAndColorsData coloursData)
            {
                result = coloursData.ColorIndexes;
            }
            return result;
        }
        public static List<int> GetColoursByIndex(CustomizationDataIDs2 data, int index)
        {
            List<int> result = new List<int>();
            object[] shapeColourData = GetCustomizationsAsArray(data);
            if (index < 0 || index >= shapeColourData.Length)
            {
                return result;
            }
            if (shapeColourData[index] is ShapeAndColorData colourData)
            {
                result = new List<int>() { colourData.ColorIndex };
            }
            else if (shapeColourData[index] is ShapeAndColorsData coloursData)
            {
                result = coloursData.ColorIndexes;
            }
            return result;
        }
        public static object[] GetCustomizationsAsArray(CustomizationData data)
        {
            return new object[] { null!, data.HeadData, data.HairData, data.MustacheData, data.EyeData, data.EyebrowData, data.MouthData, data.FacialData, data.HatsData, data.EyeGlass, data.Top, data.Bottom, data.Hands, data.Feet, data.Shoes, data.Backpack, data.Tail, data.FullBody, data.Umbrella };
        }
        public static object[] GetCustomizationsAsArray(CustomizationDataIDs2 data)
        {
            return new object[] { null!, data.HeadData, data.HairData, data.MustacheData, data.EyeData, data.EyebrowData, data.MouthData, data.FacialData, data.HatsData, data.EyeGlass, data.Top, data.Bottom, data.Hands, data.Feet, data.Shoes, data.Backpack, data.Tail, data.FullBody, data.Umbrella };
        }
        public static void SetColoursInData(ref CustomizationData data, int index, List<int>? colours)
        {
            object? shapeColour = index switch
            {
                1 => data.HeadData,
                2 => data.HairData,
                3 => data.MustacheData,
                4 => data.EyeData,
                5 => data.EyebrowData,
                6 => data.MouthData,
                7 => data.FacialData,
                8 => data.HatsData,
                9 => data.EyeGlass,
                10 => data.Top,
                11 => data.Bottom,
                12 => data.Hands,
                13 => data.Feet,
                14 => data.Shoes,
                15 => data.Backpack,
                16 => data.Tail,
                17 => data.FullBody,
                18 => data.Umbrella,
                _ => null
            };
            if (shapeColour is null)
            {
                CaseMod.Instance.Log.LogWarning($"While setting colours, the shape index of {index} was out of the allowed range (1 - 18)!");
                return;
            }
            else if (shapeColour is ShapeAndColorData colourData)
            {
                if (colours != null && colours.Count > 0)
                {
                    colourData.ColorIndex = colours[0];
                }
                else
                {
                    colourData.ColorIndex = 0;
                }
                switch (index)
                {
                    case 4:
                        {
                            data.EyeData = colourData;
                            break;
                        }
                    case 5:
                        {
                            data.EyebrowData = colourData;
                            break;
                        }
                    case 6:
                        {
                            data.MouthData = colourData;
                            break;
                        }
                    case 7:
                        {
                            data.FacialData = colourData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting colours, the non-plural shape index of {index} was out of the allowed range (4 - 7)!");
                            break;
                        }
                };
            }
            else if (shapeColour is ShapeAndColorsData coloursData)
            {
                colours ??= new List<int>();
                coloursData.ColorIndexes = colours;
                switch (index)
                {
                    case 1:
                        {
                            data.HeadData = coloursData;
                            break;
                        }
                    case 2:
                        {
                            data.HairData = coloursData;
                            break;
                        }
                    case 3:
                        {
                            data.MustacheData = coloursData;
                            break;
                        }
                    case 8:
                        {
                            data.HatsData = coloursData;
                            break;
                        }
                    case 9:
                        {
                            data.EyeGlass = coloursData;
                            break;
                        }
                    case 10:
                        {
                            data.Top = coloursData;
                            break;
                        }
                    case 11:
                        {
                            data.Bottom = coloursData;
                            break;
                        }
                    case 12:
                        {
                            data.Hands = coloursData;
                            break;
                        }
                    case 13:
                        {
                            data.Feet = coloursData;
                            break;
                        }
                    case 14:
                        {
                            data.Shoes = coloursData;
                            break;
                        }
                    case 15:
                        {
                            data.Backpack = coloursData;
                            break;
                        }
                    case 16:
                        {
                            data.Tail = coloursData;
                            break;
                        }
                    case 17:
                        {
                            data.FullBody = coloursData;
                            break;
                        }
                    case 18:
                        {
                            data.Umbrella = coloursData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting colours, the plural shape index of {index} was out of the allowed range (1 - 3 & 8 - 18)!");
                            break;
                        }
                };
            }
        }
        public static void SetColoursInData(ref CustomizationDataIDs2 data, int index, List<int>? colours)
        {
            object? shapeColour = index switch
            {
                1 => data.HeadData,
                2 => data.HairData,
                3 => data.MustacheData,
                4 => data.EyeData,
                5 => data.EyebrowData,
                6 => data.MouthData,
                7 => data.FacialData,
                8 => data.HatsData,
                9 => data.EyeGlass,
                10 => data.Top,
                11 => data.Bottom,
                12 => data.Hands,
                13 => data.Feet,
                14 => data.Shoes,
                15 => data.Backpack,
                16 => data.Tail,
                17 => data.FullBody,
                18 => data.Umbrella,
                _ => null
            };
            if (shapeColour is null)
            {
                CaseMod.Instance.Log.LogWarning($"While setting colours, the shape index of {index} was out of the allowed range (1 - 18)!");
                return;
            }
            else if (shapeColour is ShapeAndColorData colourData)
            {
                if (colours != null && colours.Count > 0)
                {
                    colourData.ColorIndex = colours[0];
                }
                else
                {
                    colourData.ColorIndex = 0;
                }
                switch (index)
                {
                    case 4:
                        {
                            data.EyeData = colourData;
                            break;
                        }
                    case 5:
                        {
                            data.EyebrowData = colourData;
                            break;
                        }
                    case 6:
                        {
                            data.MouthData = colourData;
                            break;
                        }
                    case 7:
                        {
                            data.FacialData = colourData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting colours, the non-plural shape index of {index} was out of the allowed range (4 - 7)!");
                            break;
                        }
                };
            }
            else if (shapeColour is ShapeAndColorsData coloursData)
            {
                colours ??= new List<int>();
                coloursData.ColorIndexes = colours;
                switch (index)
                {
                    case 1:
                        {
                            data.HeadData = coloursData;
                            break;
                        }
                    case 2:
                        {
                            data.HairData = coloursData;
                            break;
                        }
                    case 3:
                        {
                            data.MustacheData = coloursData;
                            break;
                        }
                    case 8:
                        {
                            data.HatsData = coloursData;
                            break;
                        }
                    case 9:
                        {
                            data.EyeGlass = coloursData;
                            break;
                        }
                    case 10:
                        {
                            data.Top = coloursData;
                            break;
                        }
                    case 11:
                        {
                            data.Bottom = coloursData;
                            break;
                        }
                    case 12:
                        {
                            data.Hands = coloursData;
                            break;
                        }
                    case 13:
                        {
                            data.Feet = coloursData;
                            break;
                        }
                    case 14:
                        {
                            data.Shoes = coloursData;
                            break;
                        }
                    case 15:
                        {
                            data.Backpack = coloursData;
                            break;
                        }
                    case 16:
                        {
                            data.Tail = coloursData;
                            break;
                        }
                    case 17:
                        {
                            data.FullBody = coloursData;
                            break;
                        }
                    case 18:
                        {
                            data.Umbrella = coloursData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting colours, the plural shape index of {index} was out of the allowed range (1 - 3 & 8 - 18)!");
                            break;
                        }
                };
            }
        }
        public static void SetShapeInData(ref CustomizationDataIDs2 data, int index, object shapeData)
        {
            if (!(shapeData is ShapeAndColorData || shapeData is ShapeAndColorsData) || shapeData is null)
            {
                return;
            }
            object? target = index switch
            {
                1 => data.HeadData,
                2 => data.HairData,
                3 => data.MustacheData,
                4 => data.EyeData,
                5 => data.EyebrowData,
                6 => data.MouthData,
                7 => data.FacialData,
                8 => data.HatsData,
                9 => data.EyeGlass,
                10 => data.Top,
                11 => data.Bottom,
                12 => data.Hands,
                13 => data.Feet,
                14 => data.Shoes,
                15 => data.Backpack,
                16 => data.Tail,
                17 => data.FullBody,
                18 => data.Umbrella,
                _ => null
            };
            if (target is null)
            {
                CaseMod.Instance.Log.LogWarning($"While setting shapes, the shape index of {index} was out of the allowed range (1 - 18)!");
                return;
            }
            else if (target is ShapeAndColorData && shapeData is ShapeAndColorData newShapeData)
            {
                switch (index)
                {
                    case 4:
                        {
                            data.EyeData = newShapeData;
                            break;
                        }
                    case 5:
                        {
                            data.EyebrowData = newShapeData;
                            break;
                        }
                    case 6:
                        {
                            data.MouthData = newShapeData;
                            break;
                        }
                    case 7:
                        {
                            data.FacialData = newShapeData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting shapes, the non-plural shape index of {index} was out of the allowed range (4 - 7)!");
                            break;
                        }
                };
            }
            else if (target is ShapeAndColorsData && shapeData is ShapeAndColorsData newShapesData)
            {
                switch (index)
                {
                    case 1:
                        {
                            data.HeadData = newShapesData;
                            break;
                        }
                    case 2:
                        {
                            data.HairData = newShapesData;
                            break;
                        }
                    case 3:
                        {
                            data.MustacheData = newShapesData;
                            break;
                        }
                    case 8:
                        {
                            data.HatsData = newShapesData;
                            break;
                        }
                    case 9:
                        {
                            data.EyeGlass = newShapesData;
                            break;
                        }
                    case 10:
                        {
                            data.Top = newShapesData;
                            break;
                        }
                    case 11:
                        {
                            data.Bottom = newShapesData;
                            break;
                        }
                    case 12:
                        {
                            data.Hands = newShapesData;
                            break;
                        }
                    case 13:
                        {
                            data.Feet = newShapesData;
                            break;
                        }
                    case 14:
                        {
                            data.Shoes = newShapesData;
                            break;
                        }
                    case 15:
                        {
                            data.Backpack = newShapesData;
                            break;
                        }
                    case 16:
                        {
                            data.Tail = newShapesData;
                            break;
                        }
                    case 17:
                        {
                            data.FullBody = newShapesData;
                            break;
                        }
                    case 18:
                        {
                            data.Umbrella = newShapesData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting shapes, the plural shape index of {index} was out of the allowed range (1 - 3 & 8 - 18)!");
                            break;
                        }
                };
            }
        }
        public static void SetShapeInData(ref CustomizationDataIDs2 data, int index, int id, List<int> colours)
        {
            int[] nonPlurals = new int[] { 4, 5, 6, 7 };
            if (nonPlurals.Contains(index))
            {
                int colourValue = 0;
                if (colours != null && colours.Count > 0)
                {
                    colourValue = colours[0];
                }
                SetShapeInData(ref data, index, new ShapeAndColorData(id, colourValue));
                return;
            }
            SetShapeInData(ref data, index, new ShapeAndColorsData(id, colours));
        }
        public static void SetShapeInData(ref CustomizationData data, int index, object shapeData)
        {
            if (!(shapeData is ShapeAndColorData || shapeData is ShapeAndColorsData) || shapeData is null)
            {
                return;
            }
            object? target = index switch
            {
                1 => data.HeadData,
                2 => data.HairData,
                3 => data.MustacheData,
                4 => data.EyeData,
                5 => data.EyebrowData,
                6 => data.MouthData,
                7 => data.FacialData,
                8 => data.HatsData,
                9 => data.EyeGlass,
                10 => data.Top,
                11 => data.Bottom,
                12 => data.Hands,
                13 => data.Feet,
                14 => data.Shoes,
                15 => data.Backpack,
                16 => data.Tail,
                17 => data.FullBody,
                18 => data.Umbrella,
                _ => null
            };
            if (target is null)
            {
                CaseMod.Instance.Log.LogWarning($"While setting shapes, the shape index of {index} was out of the allowed range (1 - 18)!");
                return;
            }
            else if (target is ShapeAndColorData && shapeData is ShapeAndColorData newShapeData)
            {
                switch (index)
                {
                    case 4:
                        {
                            data.EyeData = newShapeData;
                            break;
                        }
                    case 5:
                        {
                            data.EyebrowData = newShapeData;
                            break;
                        }
                    case 6:
                        {
                            data.MouthData = newShapeData;
                            break;
                        }
                    case 7:
                        {
                            data.FacialData = newShapeData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting shapes, the non-plural shape index of {index} was out of the allowed range (4 - 7)!");
                            break;
                        }
                };
            }
            else if (target is ShapeAndColorsData && shapeData is ShapeAndColorsData newShapesData)
            {
                switch (index)
                {
                    case 1:
                        {
                            data.HeadData = newShapesData;
                            break;
                        }
                    case 2:
                        {
                            data.HairData = newShapesData;
                            break;
                        }
                    case 3:
                        {
                            data.MustacheData = newShapesData;
                            break;
                        }
                    case 8:
                        {
                            data.HatsData = newShapesData;
                            break;
                        }
                    case 9:
                        {
                            data.EyeGlass = newShapesData;
                            break;
                        }
                    case 10:
                        {
                            data.Top = newShapesData;
                            break;
                        }
                    case 11:
                        {
                            data.Bottom = newShapesData;
                            break;
                        }
                    case 12:
                        {
                            data.Hands = newShapesData;
                            break;
                        }
                    case 13:
                        {
                            data.Feet = newShapesData;
                            break;
                        }
                    case 14:
                        {
                            data.Shoes = newShapesData;
                            break;
                        }
                    case 15:
                        {
                            data.Backpack = newShapesData;
                            break;
                        }
                    case 16:
                        {
                            data.Tail = newShapesData;
                            break;
                        }
                    case 17:
                        {
                            data.FullBody = newShapesData;
                            break;
                        }
                    case 18:
                        {
                            data.Umbrella = newShapesData;
                            break;
                        }
                    default:
                        {
                            CaseMod.Instance.Log.LogWarning($"While setting shapes, the plural shape index of {index} was out of the allowed range (1 - 3 & 8 - 18)!");
                            break;
                        }
                };
            }
        }
        public static void SetShapeInData(ref CustomizationData data, int index, int id, List<int> colours)
        {
            int[] nonPlurals = new int[] { 4, 5, 6, 7 };
            if (nonPlurals.Contains(index))
            {
                int colourValue = 0;
                if (colours != null && colours.Count > 0)
                {
                    colourValue = colours[0];
                }
                SetShapeInData(ref data, index, new ShapeAndColorData(id, colourValue));
                return;
            }
            SetShapeInData(ref data, index, new ShapeAndColorsData(id, colours));
        }
        public static AppearanceType? CosmeticToAppearanceType(CosmeticType cosmetic)
        {
            if (Enum.TryParse(cosmetic.ToString(), true, out AppearanceType result))
            {
                return result;
            }
            CaseMod.Instance.Log.LogWarning($"Could not convert CosmeticType.{cosmetic} to AppearanceType!");
            return null;
        }
        public static OutfitType? CosmeticToOutfitType(CosmeticType cosmetic)
        {
            if (Enum.TryParse(cosmetic.ToString(), true, out OutfitType result))
            {
                return result;
            }
            CaseMod.Instance.Log.LogWarning($"Could not convert CosmeticType.{cosmetic} to OutfitType!");
            return null;
        }
        public static CosmeticType GetCosmeticType(AppearanceType type)
        {
            if (Enum.TryParse(type.ToString(), true, out CosmeticType result))
            {
                return result;
            }
            CaseMod.Instance.Log.LogWarning($"Could not convert AppearanceType.{type} to CosmeticType!");
            return CosmeticType.None;
        }
        public static CosmeticType GetCosmeticType(OutfitType type)
        {
            if (Enum.TryParse(type.ToString(), true, out CosmeticType result))
            {
                return result;
            }
            CaseMod.Instance.Log.LogWarning($"Could not convert OutfitType.{type} to CosmeticType!");
            return CosmeticType.None;
        }
        public static ulong SteamIDFromPlayerID(PlayerID player)
        {
            PlayerPanelController playerPanel = NetworkSingleton<PlayerPanelController>.I;
            int playerIndex = playerPanel.PlayerIDs.IndexOf(player);
            int steamIDsCount = playerPanel.PlayerSteamIDs.Count;
            if (playerPanel is null || playerIndex == -1 || playerIndex >= steamIDsCount)
            {
                CaseMod.Instance.Log.LogWarning($"Could not get Steam ID for player with Player ID {player}!");
                return 0;
            }
            if (!ulong.TryParse(playerPanel.PlayerSteamIDs[playerPanel.PlayerIDs.IndexOf(player)], out ulong steamID))
            {
                CaseMod.Instance.Log.LogWarning($"Could not get Steam ID for player with Player ID {player}!");
                return 0;
            }
            return steamID;
        }
        public static PlayerID? PlayerIDFromSteamID(ulong steamID)
        {
            PlayerPanelController playerPanel = NetworkSingleton<PlayerPanelController>.I;
            int steamIndex = playerPanel.PlayerSteamIDs.IndexOf(steamID.ToString());
            int playerIDsCount = playerPanel.PlayerIDs.Count;
            if (playerPanel is null || steamIndex == -1 || steamIndex >= playerIDsCount)
            {
                CaseMod.Instance.Log.LogWarning($"Could not get Player ID for player with Steam ID {steamID}!");
                return PlayerID.GetDefaultNullable();
            }
            return playerPanel.PlayerIDs[playerPanel.PlayerSteamIDs.IndexOf(steamID.ToString())];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBought(int id)
        {
            bool result = false;
            if (MonoSingleton<DataManager>.I is null || MonoSingleton<DataManager>.I.PlayerDataZip is null || MonoSingleton<DataManager>.I.PlayerDataZip.IsCustomizationBought is null)
            {
                return result;
            }
            if (!MonoSingleton<DataManager>.I.PlayerDataZip.IsCustomizationBought.TryGetValue(id, out result))
            {
                result = ModDataController.GetCosmeticBought(NameFromID(id));
            }
            return result;
        }
        public static string NameFromID(int id)
        {
            ModdedIDs.TryGetValue(id, out string result);
            if (result is null)
            {
                return "NOTFOUND";
            }
            return result;
        }
        public static int IDFromName(string name)
        {
            if (!ModDataController.moddedData.TryGetValue(name, out ModdedCustomizationData data))
            {
                return -1;
            }
            return data.targetID;
        }
        public static int NextAvailableID()
        {
            return VanillaIDs.Concat(ModdedIDs.Keys).Max() + 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetBought(int id, bool bought)
        {
            if (MonoSingleton<DataManager>.I is null || MonoSingleton<DataManager>.I.PlayerDataZip is null || MonoSingleton<DataManager>.I.PlayerDataZip.IsCustomizationBought is null)
            {
                return;
            }
            if (VanillaIDs.Contains(id))
            {
                if (!MonoSingleton<DataManager>.I.PlayerDataZip.IsCustomizationBought.TryAdd(id, bought))
                {
                    MonoSingleton<DataManager>.I.PlayerDataZip.IsCustomizationBought[id] = bought;
                }
            }
            else
            {
                ModDataController.EditCosmeticBought(NameFromID(id), bought);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void StartBought(int id)
        {
            if (MonoSingleton<DataManager>.I is null || MonoSingleton<DataManager>.I.PlayerDataZip is null || MonoSingleton<DataManager>.I.PlayerDataZip.IsCustomizationBought is null)
            {
                return;
            }
            if (VanillaIDs.Contains(id))
            {
                MonoSingleton<DataManager>.I.PlayerDataZip.IsCustomizationBought.TryAdd(id, false);
            }
        }
        internal static CustomizationOption? OutfitToOption(CustomOutfit custom, int id = -1)
        {
            try
            {
                return new CustomizationOption()
                {
                    ID = id,
                    Price = custom.price,
                    UISprite = custom.uiSprite,
                    IsDynamicBone = custom.dynamicBone,
                    Mesh = custom.mesh,
                    Model = custom.model,
                    Materials = custom.materials.Select(x => x.material).ToArray(),
                    isColorEditable = custom.materials.Select(x => x.isColourEditable).ToArray(),
                    IsHairCancel = custom.replacesHair,
                    LevelCap = custom.levelCap,
                    IsBlendShape = custom.isBlendShape,
                    IsHatCancelForPet = custom.replaceHatForPet
                };
            }
            catch (Exception exception)
            {
                CaseMod.Instance.Log.LogWarning($"Failed to create Outfit CustomizationOption with ID {id}!");
                CaseMod.Instance.Log.LogWarning(exception);
                return null;
            }
        }
        internal static CustomizationOption? AppearanceToOption(CustomAppearance custom, int id = -1)
        {
            try
            {
                return new CustomizationOption()
                {
                    ID = id,
                    Price = custom.price,
                    UISprite = custom.uiSprite,
                    IsDynamicBone = custom.dynamicBone,
                    Mesh = custom.mesh,
                    Model = custom.model,
                    Materials = custom.materials.Select(x => x.material).ToArray(),
                    isColorEditable = custom.materials.Select(x => x.isColourEditable).ToArray(),
                    IsHairCancel = custom.replacesHair,
                    LevelCap = custom.levelCap,
                    IsBlendShape = custom.isBlendShape,
                    IsAnimalHead = custom.isAnimalHead
                };
            }
            catch (Exception exception)
            {
                CaseMod.Instance.Log.LogWarning($"Failed to create Appearance CustomizationOption with ID {id}!");
                CaseMod.Instance.Log.LogWarning(exception);
                return null;
            }
        }
        internal static CustomizationOptionForSprite? SpriteToOption(CustomAppearance custom, int id = -1)
        {
            try
            {
                return new CustomizationOptionForSprite()
                {
                    ID = id,
                    Price = custom.price,
                    UISprite = custom.uiSprite,
                    Material = custom.materials.Single().material,
                    LevelCap = custom.levelCap,
                };
            }
            catch (Exception exception)
            {
                CaseMod.Instance.Log.LogWarning($"Failed to create CustomizationOptionForSprite with ID {id}!");
                CaseMod.Instance.Log.LogWarning(exception);
                return null;
            }
        }
        internal static Dictionary<string, ModdedCustomizationData>? LoadData()
        {
            Dictionary<string, ModdedCustomizationData>? loadedData = null;
            if (!Directory.Exists(CaseMod.DataLocation))
            {
                Directory.CreateDirectory(CaseMod.DataLocation);
            }
            StreamReader? reader = null;
            try
            {
                if (!File.Exists(Path.Combine(CaseMod.DataLocation, "data.json")))
                {
                    return loadedData;
                }
                reader = File.OpenText(Path.Combine(CaseMod.DataLocation, "data.json"));
                loadedData = JsonConvert.DeserializeObject<Dictionary<string, ModdedCustomizationData>>(reader.ReadToEnd());
            }
            catch (Exception exception)
            {
                CaseMod.Instance.Log.LogError("Failed to load modded data!");
                CaseMod.Instance.Log.LogError(exception);
            }
            finally
            {
                reader?.Dispose();
            }
            return loadedData;
        }
    }
    [CreateAssetMenu(menuName = "OnTheCase/CustomOutfit")]
    public class CustomOutfit : CustomCosmetic
    {
        public OutfitType type;
        public bool replaceHatForPet;
        public override void OnValidate()
        {
            base.OnValidate();
            if (type != OutfitType.Hats)
            {
                replaceHatForPet = false;
            }
            while (isBlendShape.Count > 6)
            {
                isBlendShape.RemoveAt(isBlendShape.Count - 1);
            }
            while (isBlendShape.Count < 6)
            {
                isBlendShape.Add(false);
            }
        }
    }
    [Serializable]
    public class CustomMatData
    {
        public Material? material;
        public bool isColourEditable;
    }
    [CreateAssetMenu(menuName = "OnTheCase/CustomAppearance")]
    public class CustomAppearance : CustomCosmetic
    {
        public AppearanceType type;
        public bool isAnimalHead;
        public override void OnValidate()
        {
            base.OnValidate();
            if (type != AppearanceType.Head)
            {
                isAnimalHead = false;
            }
            if (!(type == AppearanceType.Head || type == AppearanceType.Hair))
            {
                dynamicBone = false;
                mesh = null;
                model = null;
                replacesHair = false;
                isBlendShape = new List<bool>();
                materials ??= new List<CustomMatData>();
                if (materials.Count > 1)
                {
                    materials.Add(materials[0]);
                }
                else if (materials.Count == 0)
                {
                    materials.Add(new CustomMatData());
                }
            }
            else
            {
                while (isBlendShape.Count > 6)
                {
                    isBlendShape.RemoveAt(isBlendShape.Count - 1);
                }
                while (isBlendShape.Count < 6)
                {
                    isBlendShape.Add(false);
                }
            }
        }
    }
    public class CustomCosmetic : ScriptableObject
    {
        internal string modGUID = string.Empty;
        public string cosmeticName = string.Empty;
        public int price;
        public Sprite? uiSprite;
        public bool dynamicBone;
        public Mesh? mesh;
        public GameObject? model;
        public List<CustomMatData> materials = new List<CustomMatData>();
        public bool replacesHair;
        public int levelCap;
        public List<bool> isBlendShape = new List<bool>();
        public bool autoWeight = false;
        public int autoWeightSplits = 1;
        public virtual void OnValidate()
        {
            if (cosmeticName == string.Empty || cosmeticName is null)
            {
                cosmeticName = name;
            }
        }
        internal void FixShaders()
        {
            for (int i = 0; i < materials.Count; i++)
            {
                Material? mat = materials[i].material;
                if (mat is null)
                {
                    continue;
                }
                CaseUtils.FixShader(mat);
            }
            if (model != null)
            {
                List<Material> toFix = new List<Material>();
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    toFix.AddRange(renderers[i].materials);
                }
                ParticleSystem[] particleSystems = model.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < particleSystems.Length; i++)
                {
                    ParticleSystemRenderer particleRenderer = particleSystems[i].GetComponent<ParticleSystemRenderer>();
                    toFix.AddRange(particleRenderer.materials);
                }
                for (int i = 0; i < toFix.Count; i++)
                {
                    CaseUtils.FixShader(toFix[i]);
                }
            }
        }
        internal void AutoWeight()
        {
            if (CaseMod.Instance.Dummy is null || (mesh is null && model is null))
            {
                return;
            }
            List<(Vector3, Vector3)> allBones = GetBones(CaseMod.Instance.Dummy.transform.GetChild(1));
            List<Mesh> meshes = new List<Mesh>();
            if (mesh != null && mesh.isReadable)
            {
                meshes.Add(mesh);
            }
            if (model != null)
            {
                SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    Mesh newMesh = renderers[i].sharedMesh;
                    if (newMesh != null && newMesh.isReadable)
                    {
                        meshes.Add(newMesh);
                    }
                }
            }
            for (int i = 0; i < meshes.Count; i++)
            {
                bool abort = false;
                Mesh toEdit = meshes[i];
                Vector3[] vertices = toEdit.vertices;
                BoneWeight[] weights = new BoneWeight[toEdit.boneWeights.Length];
                for (int j = 0; j < vertices.Length; j++)
                {
                    if (abort)
                    {
                        break;
                    }
                    Vector3 vertex = vertices[j];
                    int closestBone = -1;
                    float closestDist = float.MaxValue;
                    for (int k = 0; k < allBones.Count; k++)
                    {
                        (Vector3, Vector3) bone = allBones[k];
                        for (int l = 0; l < 2 + autoWeightSplits; l++)
                        {
                            Vector3 checkingVector;
                            if (l == 0)
                            {
                                checkingVector = bone.Item1;
                            }
                            else if (l == 1)
                            {
                                checkingVector = bone.Item2;
                            }
                            else
                            {
                                checkingVector = (bone.Item2 - bone.Item1) * ((l - 1) / (autoWeightSplits + 1));
                            }
                            float checkDist = Vector3.Distance(checkingVector, vertex);
                            if (checkDist < closestDist)
                            {
                                closestBone = k;
                                closestDist = checkDist;
                                break;
                            }
                        }
                    }
                    if (closestBone > -1)
                    {
                        weights[j].boneIndex0 = closestBone;
                        weights[j].weight0 = 1f;
                    }
                    else
                    {
                        CaseMod.Instance.Log.LogError($"Vertex {j} of mesh \"{toEdit.name}\" could not find any bones to weight to! Aborting Auto-Weight");
                        abort = true;
                    }
                }
                if (!abort)
                {
                    toEdit.boneWeights = weights;
                }
            }
        }
        private static List<(Vector3, Vector3)> GetBones(Transform parent)
        {
            List<(Vector3, Vector3)> toReturn = new List<(Vector3, Vector3)>();
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                toReturn.Add((parent.position, child.position));
                if (child.childCount > 0)
                {
                    toReturn.AddRange(GetBones(child));
                }
            }
            return toReturn;
        }
    }
    [Serializable]
    public class ModdedCustomizationData
    {
        [JsonConstructor]
        public ModdedCustomizationData(int id, CosmeticType type, bool boolean, Dictionary<int, List<int>> dict)
        {
            targetID = id;
            cosmeticType = type;
            bought = boolean;
            colourIndices = dict;
            colourIndices ??= new Dictionary<int, List<int>>();
        }
        public ModdedCustomizationData(int id, CosmeticType type)
        {
            targetID = id;
            cosmeticType = type;
            bought = false;
            colourIndices = new Dictionary<int, List<int>>();
        }
        public int targetID;
        public CosmeticType cosmeticType;
        public bool bought;
        public Dictionary<int, List<int>> colourIndices;
        internal void SetID(int id)
        {
            targetID = id;
        }
        internal void SetType(int enumValue)
        {
            if (Enum.IsDefined(typeof(CosmeticType), enumValue))
            {
                SetType((CosmeticType)enumValue);
            }
        }
        internal void SetType(string enumName)
        {
            if (Enum.IsDefined(typeof(CosmeticType), enumName))
            {
                SetType(Enum.Parse<CosmeticType>(enumName));
            }
        }
        internal void SetType(CosmeticType enumValue)
        {
            cosmeticType = enumValue;
        }
        internal void SetBought(bool boolean)
        {
            bought = boolean;
        }
        internal bool AddPreset(int preset)
        {
            return colourIndices.TryAdd(preset, new List<int>());
        }
        internal void SetPresetColours(int preset, List<int> colours)
        {
            if (AddPreset(preset))
            {
                colourIndices[preset] = colours;
            }
        }
        internal void RemovePreset(int preset)
        {
            colourIndices.Remove(preset);
        }
    }
    public struct IDKeys : IPackedAuto
    {
        public IDKeys(Dictionary<int, string> ids)
        {
            pairs = (Dictionary<string, int>)ids.Select((x) => new KeyValuePair<string, int>(x.Value, x.Key));
        }
        public IDKeys(Dictionary<string, int> ids)
        {
            pairs = ids;
        }
        public Dictionary<string, int> pairs;
    }
    public enum CosmeticType
    {
        None,
        Head,
        Hair,
        Mustache,
        Eye,
        Eyebrow,
        Mouth,
        Facial,
        Hats,
        EyeGlass,
        Top,
        Bottom,
        Hands,
        Feet,
        Shoes,
        Backpack,
        Tail,
        FullBody,
        Umbrella
    }
    public static class PurrSerializers
    {
        [RegisterPackers(-1)]
        static void RegisterIDKeysDict()
        {
            PackCollections.RegisterDictionary<string, int>();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IDKeysWrite(BitPacker stream, IDKeys value)
        {
            Packer<Dictionary<string, int>>.Write(stream, value.pairs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IDKeysRead(BitPacker stream, ref IDKeys value)
        {
            Packer<Dictionary<string, int>>.Read(stream, ref value.pairs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IDKeysWriteDelta(BitPacker stream, IDKeys oldValue, IDKeys value)
        {
            int num = stream.AdvanceBits(1);
            bool flag = DeltaPacker<Dictionary<string, int>>.Write(stream, oldValue.pairs, value.pairs);
            stream.WriteAt(num, flag);
            if (!flag)
            {
                stream.SetBitPosition(num + 1);
            }
            return flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IDKeysReadDelta(BitPacker stream, IDKeys oldValue, ref IDKeys value)
        {
            bool value2 = default;
            Packer<bool>.Read(stream, ref value2);
            if (value2)
            {
                DeltaPacker<Dictionary<string, int>>.Read(stream, oldValue.pairs, ref value.pairs);
            }
            else
            {
                value = Packer.Copy(oldValue);
            }
        }

        [RegisterPackers(-1)]
        static void RegisterIDKeysType()
        {
            Packer<IDKeys>.RegisterWriter(IDKeysWrite);
            DeltaPacker<IDKeys>.RegisterWriter(IDKeysWriteDelta);
            Packer<IDKeys>.RegisterReader(IDKeysRead);
            DeltaPacker<IDKeys>.RegisterReader(IDKeysReadDelta);
        }
    }
}