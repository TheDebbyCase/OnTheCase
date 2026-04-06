using HarmonyLib;
using OnTheCase.Utils;
using PurrNet;
using PurrNet.Packing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
namespace OnTheCase.Patches
{
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public class PlayerCustomizationControllerPatches
    {
        [HarmonyPatch(nameof(PlayerCustomizationController.NotifyOtherClients))]
        [HarmonyPrefix]
        public static bool NotifyOtherClientsPre(PlayerCustomizationController __instance)
        {
            CaseMod.Instance.Log.LogDebug($"Running {nameof(PlayerCustomizationController)}.{nameof(PlayerCustomizationController.NotifyOtherClients)}");
            return true;
        }
        [HarmonyPatch(nameof(PlayerCustomizationController.HandleRPCGenerated_6))]
        [HarmonyPrefix]
        public static bool HandleRPCGenerated_6Pre(PlayerCustomizationController __instance)
        {
            CaseMod.Instance.Log.LogDebug($"Running {nameof(PlayerCustomizationController)}.{nameof(PlayerCustomizationController.HandleRPCGenerated_6)}");
            return true;
        }
        [HarmonyPatch(nameof(PlayerCustomizationController.UpdateNewPlayer))]
        [HarmonyPrefix]
        public static bool UpdateNewPlayerPre(PlayerCustomizationController __instance)
        {
            CaseMod.Instance.Log.LogDebug($"Running {nameof(PlayerCustomizationController)}.{nameof(PlayerCustomizationController.UpdateNewPlayer)}");
            return true;
        }
        [HarmonyPatch(nameof(PlayerCustomizationController.HandleRPCGenerated_5))]
        [HarmonyPrefix]
        public static bool HandleRPCGenerated_5Pre(PlayerCustomizationController __instance)
        {
            CaseMod.Instance.Log.LogDebug($"Running {nameof(PlayerCustomizationController)}.{nameof(PlayerCustomizationController.HandleRPCGenerated_5)}");
            return true;
        }
        [HarmonyPatch(nameof(PlayerCustomizationController.NotifyOtherClientsUpdate))]
        [HarmonyPrefix]
        public static bool NotifyOtherClientsUpdatePre(PlayerCustomizationController __instance)
        {
            CaseMod.Instance.Log.LogDebug($"Running {nameof(PlayerCustomizationController)}.{nameof(PlayerCustomizationController.NotifyOtherClientsUpdate)}");
            return false;
        }
        [HarmonyPatch(nameof(PlayerCustomizationController.HandleRPCGenerated_7))]
        [HarmonyPrefix]
        public static bool HandleRPCGenerated_7Pre(PlayerCustomizationController __instance)
        {
            CaseMod.Instance.Log.LogDebug($"Running {nameof(PlayerCustomizationController)}.{nameof(PlayerCustomizationController.HandleRPCGenerated_7)}");
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public class PlayerCustomizationControllerNotifyOtherClientsPatch
    {
        [HarmonyPatch(nameof(PlayerCustomizationController.NotifyOtherClients))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(PlayerCustomizationController.NotifyOtherClients)}");
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            MethodInfo target = AccessTools.Method(typeof(Packer<byte[]>), nameof(Packer<byte[]>.Write), new Type[] { typeof(BitPacker), typeof(byte[]) });
            MethodInfo newMethod1 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.AddOrSetLocalIDKey));
            MethodInfo newMethod2 = AccessTools.Method(typeof(Packer<IDKeys>), nameof(Packer<IDKeys>.Write), new Type[] { typeof(BitPacker), typeof(IDKeys) });
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, (byte)0));
                    newCodes.Add(new CodeInstruction(OpCodes.Callvirt, newMethod1));
                    newCodes.Add(new CodeInstruction(OpCodes.Call, newMethod2));
                    codes.InsertRange(i + 1, newCodes);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public class PlayerCustomizationControllerHandleRPCGenerated_6Patch
    {
        [HarmonyPatch(nameof(PlayerCustomizationController.HandleRPCGenerated_6))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> newCodesPre = new List<CodeInstruction>();
            List<CodeInstruction> newCodesPost = new List<CodeInstruction>();
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(PlayerCustomizationController.HandleRPCGenerated_6)}");
            MethodInfo target = AccessTools.FirstMethod(typeof(Packer<byte[]>), (x) => x.Name == "Read" && x.GetParameters().Length > 1);
            MethodInfo newMethod1 = AccessTools.FirstMethod(typeof(Packer<IDKeys>), (x) => x.Name == "Read" && x.GetParameters().Length > 1);
            MethodInfo newMethod2 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.SetIDKeyRPC), new Type[] { typeof(IDKeys), typeof(RPCInfo) });
            MethodInfo newMethod3 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.ApplyIDKey));
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    LocalBuilder idkeysValue = generator.DeclareLocal(typeof(IDKeys));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Ldarg, (byte)1));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Ldloca_S, (byte)idkeysValue.LocalIndex));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Call, newMethod1));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloc_S, (byte)idkeysValue.LocalIndex));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloc_S, (byte)7));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Callvirt, newMethod2));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloca, (byte)0));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Callvirt, newMethod3));
                    codes.InsertRange(i + 3, newCodesPost);
                    codes.InsertRange(i + 1, newCodesPre);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public class PlayerCustomizationControllerUpdateNewPlayerPatch
    {
        [HarmonyPatch(nameof(PlayerCustomizationController.UpdateNewPlayer))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(PlayerCustomizationController.UpdateNewPlayer)}");
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            MethodInfo target = AccessTools.Method(typeof(Packer<bool>), nameof(Packer<bool>.Write), new Type[] { typeof(BitPacker), typeof(bool) });
            MethodInfo newMethod1 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.AddOrSetLocalIDKey));
            MethodInfo newMethod2 = AccessTools.Method(typeof(Packer<IDKeys>), nameof(Packer<IDKeys>.Write), new Type[] { typeof(BitPacker), typeof(IDKeys) });
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, (byte)0));
                    newCodes.Add(new CodeInstruction(OpCodes.Callvirt, newMethod1));
                    newCodes.Add(new CodeInstruction(OpCodes.Call, newMethod2));
                    codes.InsertRange(i + 1, newCodes);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public class PlayerCustomizationControllerHandleRPCGenerated_5Patch
    {
        [HarmonyPatch(nameof(PlayerCustomizationController.HandleRPCGenerated_5))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(PlayerCustomizationController.HandleRPCGenerated_5)}");
            List<CodeInstruction> newCodesPre = new List<CodeInstruction>();
            List<CodeInstruction> newCodesPost = new List<CodeInstruction>();
            MethodInfo target = AccessTools.FirstMethod(typeof(Packer<bool>), (x) => x.Name == "Read" && x.GetParameters().Length > 1);
            MethodInfo newMethod1 = AccessTools.FirstMethod(typeof(Packer<IDKeys>), (x) => x.Name == "Read" && x.GetParameters().Length > 1);
            MethodInfo newMethod2 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.SetIDKeyRPC), new Type[] { typeof(IDKeys), typeof(RPCInfo) });
            MethodInfo newMethod3 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.ApplyIDKey));
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    LocalBuilder idkeysValue = generator.DeclareLocal(typeof(IDKeys));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Ldarg, (byte)1));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Ldloca_S, (byte)idkeysValue.LocalIndex));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Call, newMethod1));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloc_S, (byte)idkeysValue.LocalIndex));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloc_S, (byte)30));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Callvirt, newMethod2));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloca, (byte)2));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Callvirt, newMethod3));
                    codes.InsertRange(i + 3, newCodesPost);
                    codes.InsertRange(i + 1, newCodesPre);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public class PlayerCustomizationControllerNotifyOtherClientsUpdatePatch
    {
        [HarmonyPatch(nameof(PlayerCustomizationController.NotifyOtherClientsUpdate))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(PlayerCustomizationController.NotifyOtherClientsUpdate)}");
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            MethodInfo target = AccessTools.Method(typeof(Packer<PetPosType>), nameof(Packer<PetPosType>.Write), new Type[] { typeof(BitPacker), typeof(PetPosType) });
            MethodInfo newMethod1 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.AddOrSetLocalIDKey));
            MethodInfo newMethod2 = AccessTools.Method(typeof(Packer<IDKeys>), nameof(Packer<IDKeys>.Write), new Type[] { typeof(BitPacker), typeof(IDKeys) });
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc, (byte)0));
                    newCodes.Add(new CodeInstruction(OpCodes.Callvirt, newMethod1));
                    newCodes.Add(new CodeInstruction(OpCodes.Call, newMethod2));
                    codes.InsertRange(i + 1, newCodes);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
    [HarmonyPatch(typeof(PlayerCustomizationController))]
    public class PlayerCustomizationControllerHandleRPCGenerated_7Patch
    {
        [HarmonyPatch(nameof(PlayerCustomizationController.HandleRPCGenerated_7))]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            CaseMod.Instance.Log.LogDebug($"Harmony Patching {nameof(PlayerCustomizationController.HandleRPCGenerated_7)}");
            List<CodeInstruction> newCodesPre = new List<CodeInstruction>();
            List<CodeInstruction> newCodesPost = new List<CodeInstruction>();
            MethodInfo target = AccessTools.FirstMethod(typeof(Packer<PetPosType>), (x) => x.Name == "Read" && x.GetParameters().Length > 1);
            MethodInfo newMethod1 = AccessTools.FirstMethod(typeof(Packer<IDKeys>), (x) => x.Name == "Read" && x.GetParameters().Length > 1);
            MethodInfo newMethod2 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.SetIDKeyRPC), new Type[] { typeof(IDKeys), typeof(RPCInfo) });
            MethodInfo newMethod3 = AccessTools.Method(typeof(ModDataController), nameof(ModDataController.ApplyIDKey));
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(target))
                {
                    LocalBuilder idkeysValue = generator.DeclareLocal(typeof(IDKeys));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Ldarg, (byte)1));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Ldloca_S, (byte)idkeysValue.LocalIndex));
                    newCodesPre.Add(new CodeInstruction(OpCodes.Call, newMethod1));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloc_S, (byte)idkeysValue.LocalIndex));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloc_S, (byte)4));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Callvirt, newMethod2));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Ldloca, (byte)0));
                    newCodesPost.Add(new CodeInstruction(OpCodes.Callvirt, newMethod3));
                    codes.InsertRange(i + 3, newCodesPost);
                    codes.InsertRange(i + 1, newCodesPre);
                    break;
                }
            }
            return codes.AsEnumerable();
        }
    }
}