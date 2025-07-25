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


        // from WellRoundedBalance's SharedBase.cs
        internal static float GetHyperbolic(float firstStack, float cap, float chance)
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