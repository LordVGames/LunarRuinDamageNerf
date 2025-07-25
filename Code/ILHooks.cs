using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using R2API;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.DetourTypes;
using MonoDetour.Cil;

namespace LunarRuinDamageNerf
{
    internal static class ILHooks
    {
        internal static void Setup()
        {
            LunarRuinEdit.Setup();
            // TODO find where the hidden dot actually comes from
            // because rn i just hooked where lunar ruin is applied
            //LunarRuinHiddenDotEdit.Setup();
        }



        [MonoDetourTargets(typeof(HealthComponent))]
        private static class LunarRuinEdit
        {
            private const int LunarRuinDamageIncreaseILVariableNumber = 28;



            [MonoDetourHookInitialize]
            internal static void Setup()
            {
                MonoDetourHooks.RoR2.HealthComponent.TakeDamageProcess.ILHook(DoEdit);
            }



            private static void DoEdit(ILManipulationInfo info)
            {
                ILWeaver w = new(info);
                ILLabel skipOriginalDamageCalc = w.DefineLabel();


                // going before line:
                // float num2 = (float)this.body.GetBuffCount(DLC2Content.Buffs.lunarruin) * 0.1f;
                w.MatchRelaxed(
                    x => x.MatchLdarg(0) && w.SetCurrentTo(x),
                    x => x.MatchLdfld<HealthComponent>("body"),
                    x => x.MatchLdsfld("RoR2.DLC2Content/Buffs", "lunarruin"),
                    x => x.MatchCallvirt<CharacterBody>("GetBuffCount")
                ).ThrowIfFailure()
                .InsertBeforeCurrent(
                    w.Create(OpCodes.Ldarg_0),
                    w.Create(OpCodes.Ldarg_1),
                    w.CreateCall(ChangeDamageIncrease),
                    w.Create(OpCodes.Stloc, LunarRuinDamageIncreaseILVariableNumber),
                    w.Create(OpCodes.Br, skipOriginalDamageCalc)
                );


                // going after line:
                // float num2 = (float)this.body.GetBuffCount(DLC2Content.Buffs.lunarruin) * 0.1f;
                w.MatchRelaxed(
                    x => x.MatchLdcR4(0.1f),
                    x => x.MatchMul(),
                    x => x.MatchStloc(LunarRuinDamageIncreaseILVariableNumber) && w.SetCurrentTo(x)
                ).ThrowIfFailure()
                .MarkLabelToCurrentNext(skipOriginalDamageCalc);
            }

            private static float ChangeDamageIncrease(HealthComponent healthComponent, DamageInfo damageInfo)
            {
                float finalDamageIncrease;
                int lunarRuinCount = healthComponent.body.GetBuffCount(DLC2Content.Buffs.lunarruin);

                // could make this better but idc rn
                if (ConfigOptions.DamageIncreaseForSkillsOnly.Value && !damageInfo.damageType.IsDamageSourceSkillBased)
                {
                    finalDamageIncrease = 0;
                }
                else
                {
                    float lunarRuinDamageBuff = ConfigOptions.DamageIncreasePerLunarRuin.Value * lunarRuinCount;
                    if (ConfigOptions.EnableDiminishingDamage.Value)
                    {
                        finalDamageIncrease = Main.GetHyperbolic(ConfigOptions.DamageIncreasePerLunarRuin.Value, ConfigOptions.LunarRuinDamageCap.Value, lunarRuinDamageBuff);
                    }
                    else if (ConfigOptions.LunarRuinDamageCap.Value != -1)
                    {
                        finalDamageIncrease = MathF.Min(lunarRuinDamageBuff, ConfigOptions.LunarRuinDamageCap.Value);
                    }
                    else
                    {
                        finalDamageIncrease = lunarRuinDamageBuff;
                    }
                }


                if (ConfigOptions.EnableLoggingDamageIncrease.Value)
                {
                    Log.Info($"Lunar ruin's damage increase went from +{(lunarRuinCount * 0.1f) * 100}% to +{finalDamageIncrease}%");
                }
                // returned value needs to be a percentage of 1
                return finalDamageIncrease *= 0.01f;
            }
        }


        /*[MonoDetourTargets(typeof(GlobalEventManager))]
        private static class LunarRuinHiddenDotEdit
        {
            [MonoDetourHookInitialize]
            internal static void Setup()
            {
                MonoDetourHooks.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(DoEdit);
            }



            private static void DoEdit(ILManipulationInfo info)
            {
                ILWeaver w = new(info);
                ILLabel skipInflictStupidDot = w.DefineLabel();


                // going before line:
                // DotController.InflictDot(characterBody2.gameObject, damageInfo.attacker, DotController.DotIndex.LunarRuin, 5f, 1f, null);
                w.MatchRelaxed(
                    x => x.MatchBr(out skipInflictStupidDot),
                    x => x.MatchLdloc(1) && w.SetCurrentTo(x),
                    x => x.MatchCallvirt<Component>("get_gameObject"),
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>("attacker"),
                    x => x.MatchLdcI4(9),
                    x => x.MatchLdcR4(5),
                    x => x.MatchLdcR4(1)
                ).ThrowIfFailure()
                .InsertBeforeCurrentStealLabels(
                    w.CreateCall(PutConfigOptionOnStack),
                    w.Create(OpCodes.Brfalse, skipInflictStupidDot)
                );
            }

            private static bool PutConfigOptionOnStack()
            {
                return ConfigOptions.AllowHiddenDot.Value;
            }
        }*/
    }
}