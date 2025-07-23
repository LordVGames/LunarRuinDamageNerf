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
    internal static class Main
    {
        private const int LunarRuinDamageIncreaseILVariableNumber = 28;

        private static CharacterSelectController _characterSelectController = null;
        private static bool IsCharacterSelectControllerValid
        {
            get
            {
                if (_characterSelectController == null)
                {
                    return false;
                }
                if (_characterSelectController.characterSelectBarController == null)
                {
                    return false;
                }
                return true;
            }
        }

        [MonoDetourTargets(typeof(HealthComponent))]
        internal static class ILHooks
        {
            [MonoDetourHookInitialize]
            internal static void Setup()
            {
                // i'm using monodetour's class, shut up
#pragma warning disable CS0435 // Namespace conflicts with imported type
                On.RoR2.HealthComponent.TakeDamageProcess.ILHook(SetupIL);
#pragma warning restore CS0435 // Namespace conflicts with imported type
            }

            private static void SetupIL(ILManipulationInfo info)
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
                        finalDamageIncrease = GetHyperbolic(ConfigOptions.DamageIncreasePerLunarRuin.Value, ConfigOptions.LunarRuinDamageCap.Value, lunarRuinDamageBuff);
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


        // from WellRoundedBalance's SharedBase.cs
        private static float GetHyperbolic(float firstStack, float cap, float chance)
        {
            if (firstStack >= cap) return cap * (chance / firstStack);
            float count = chance / firstStack;
            float coeff = 100 * firstStack / (cap - firstStack);
            return cap * (1 - (100 / ((count * coeff) + 100)));
        }


        internal static void UpdateLunarRuinDescription()
        {
            Log.Debug("UpdateLunarRuinDescription");
            string newLunarRuinDescription;
            string extraNotes = "";
            bool extraNotesAdded = false;

            if (ConfigOptions.LunarRuinDamageCap.Value != -1)
            {
                extraNotes = Language.GetStringFormatted("KEYWORD_LUNARRUIN_DAMAGECAPPED", ConfigOptions.LunarRuinDamageCap.Value);
                extraNotesAdded = true;
            }
            if (ConfigOptions.EnableDiminishingDamage.Value)
            {
                if (ConfigOptions.LunarRuinDamageCap.Value != -1)
                {
                    extraNotes += " ";
                }
                extraNotes += Language.GetString("KEYWORD_LUNARRUIN_HYPERBOLICSTACKING");
                extraNotesAdded = true;
            }
            if (extraNotesAdded)
            {
                extraNotes = " " + extraNotes;
            }
            Log.Debug(extraNotes);

            if (ConfigOptions.DamageIncreaseForSkillsOnly.Value)
            {
                newLunarRuinDescription = Language.GetStringFormatted("KEYWORD_LUNARRUIN_NERFED_SKILLSONLY", ConfigOptions.DamageIncreasePerLunarRuin.Value, extraNotes);
            }
            else
            {
                newLunarRuinDescription = Language.GetStringFormatted("KEYWORD_LUNARRUIN_NERFED", ConfigOptions.DamageIncreasePerLunarRuin.Value, extraNotes);
            }
            Log.Debug(newLunarRuinDescription);
            LanguageAPI.AddOverlay("KEYWORD_LUNARRUIN", newLunarRuinDescription);
            // bandaid fix for the keyword text not updating after changing the token's text
            ReselectFalseSonIfSelected();
        }
        private static void ReselectFalseSonIfSelected()
        {
            if (!IsCharacterSelectControllerValid)
            {
                return;
            }
            if (_characterSelectController.currentSurvivorDef.displayNameToken != "FALSESON_BODY_NAME")
            {
                return;
            }
            SurvivorDef falseSonDef = _characterSelectController.currentSurvivorDef;
            // edited from RandomCharacterSelection mod's code because i don't wanna load the survivordef asset
            SurvivorDef commandoDef = SurvivorCatalog.orderedSurvivorDefs.Where((SurvivorDef survivorDef) => (survivorDef.displayNameToken == "COMMANDO_BODY_NAME")).ElementAt(0);
            _characterSelectController.characterSelectBarController.PickIconBySurvivorDef(commandoDef);
            _characterSelectController.StartCoroutine(DelaySelectFalseSon(falseSonDef));
        }
        private static IEnumerator DelaySelectFalseSon(SurvivorDef falseSonDef)
        {
            yield return null;
            if (!IsCharacterSelectControllerValid)
            {
                yield break;
            }
            _characterSelectController.characterSelectBarController.PickIconBySurvivorDef(falseSonDef);
        }


        internal static void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, CharacterSelectController self)
        {
            orig(self);
            _characterSelectController = self;
        }
        internal static void CharacterSelectController_OnDisable(On.RoR2.UI.CharacterSelectController.orig_OnDisable orig, CharacterSelectController self)
        {
            orig(self);
            _characterSelectController = null;
        }
    }
}