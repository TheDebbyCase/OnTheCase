using OnTheCase.Utils;
using System.Collections.Generic;
namespace OnTheCase.Extensions
{
    public static class CustomizationDataIDs2Extensions
    {
        public static CustomizationDataIDs2 ReplaceModded(this CustomizationDataIDs2 toReplace, CustomizationDataIDs2 replaceWith)
        {
            foreach (KeyValuePair<string, ModdedCustomizationData> pair in ModDataController.moddedCosmeticData)
            {
                if (CosmeticUtils.IsIDPresent(toReplace, pair.Value.targetID, out int target))
                {
                    CosmeticUtils.SetShapeInData(ref toReplace, target, CosmeticUtils.GetCustomizationsAsArray(replaceWith)[target]);
                }
            }
            return toReplace;
        }
        public static CustomizationDataIDs2 Copy(this CustomizationDataIDs2 toCopy)
        {
            return new CustomizationDataIDs2()
                {
                    IsFullBody = toCopy.IsFullBody,
                    HeadData = new ShapeAndColorsData(toCopy.HeadData.ShapeIndex, toCopy.HeadData.ColorIndexes),
                    HairData = new ShapeAndColorsData(toCopy.HairData.ShapeIndex, toCopy.HairData.ColorIndexes),
                    MustacheData = new ShapeAndColorsData(toCopy.MustacheData.ShapeIndex, toCopy.MustacheData.ColorIndexes),
                    EyeData = new ShapeAndColorData(toCopy.EyeData.ShapeIndex, toCopy.EyeData.ColorIndex),
                    EyebrowData = new ShapeAndColorData(toCopy.EyebrowData.ShapeIndex, toCopy.EyebrowData.ColorIndex),
                    MouthData = new ShapeAndColorData(toCopy.MouthData.ShapeIndex, toCopy.MouthData.ColorIndex),
                    FacialData = new ShapeAndColorData(toCopy.FacialData.ShapeIndex, toCopy.FacialData.ColorIndex),
                    HatsData = new ShapeAndColorsData(toCopy.HatsData.ShapeIndex, toCopy.HatsData.ColorIndexes),
                    Top = new ShapeAndColorsData(toCopy.Top.ShapeIndex, toCopy.Top.ColorIndexes),
                    Bottom = new ShapeAndColorsData(toCopy.Bottom.ShapeIndex, toCopy.Bottom.ColorIndexes),
                    Feet = new ShapeAndColorsData(toCopy.Feet.ShapeIndex, toCopy.Feet.ColorIndexes),
                    Shoes = new ShapeAndColorsData(toCopy.Shoes.ShapeIndex, toCopy.Shoes.ColorIndexes),
                    Hands = new ShapeAndColorsData(toCopy.Hands.ShapeIndex, toCopy.Hands.ColorIndexes),
                    Tail = new ShapeAndColorsData(toCopy.Tail.ShapeIndex, toCopy.Tail.ColorIndexes),
                    EyeGlass = new ShapeAndColorsData(toCopy.EyeGlass.ShapeIndex, toCopy.EyeGlass.ColorIndexes),
                    Backpack = new ShapeAndColorsData(toCopy.Backpack.ShapeIndex, toCopy.Backpack.ColorIndexes),
                    FullBody = new ShapeAndColorsData(toCopy.FullBody.ShapeIndex, toCopy.FullBody.ColorIndexes),
                    Umbrella = new ShapeAndColorsData(toCopy.Umbrella.ShapeIndex, toCopy.Umbrella.ColorIndexes),
                };
        }
    }
}