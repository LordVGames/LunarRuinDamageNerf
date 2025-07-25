using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using MiscFixes.Modules;

namespace LunarRuinDamageNerf
{
    public static class ConfigOptions
    {
        private const string _configSectionName = "Lunar Ruin Damage Nerf";


        public static ConfigEntry<bool> DamageIncreaseForSkillsOnly;
        public static ConfigEntry<float> DamageIncreasePerLunarRuin;
        public static ConfigEntry<bool> EnableDiminishingDamage;
        public static ConfigEntry<float> LunarRuinDamageCap;
        //public static ConfigEntry<bool> AllowHiddenDot;
        public static ConfigEntry<bool> EnableLoggingDamageIncrease;
        private static void OnConfigOptionChanged(object sender, EventArgs e)
        {
            Main.UpdateLunarRuinDescription();
        }


        internal static void BindConfigOptions(ConfigFile config)
        {
            DamageIncreaseForSkillsOnly = config.BindOption(
                _configSectionName,
                "Make lunar ruins damage increase only apply to skill damage",
                "If enabled, will make the damage increase from lunar ruin only apply to skill damage and not proc/item damage.",
                true
            );
            DamageIncreasePerLunarRuin = config.BindOptionSlider(
                _configSectionName,
                "Damage increase per lunar ruin stack",
                "The increase to incoming damage for each stack of lunar ruin. Value is a percentage, so 5 means lunar ruin will increase all incoming damage by 5%. Vanilla's value is 10.",
                 10f
            );
            EnableDiminishingDamage = config.BindOption(
                _configSectionName,
                "Enable diminishing/hyperbolic scaling for lunar ruin`s damage increase.",
                "If enabled, will make the damage increase for each stack of lunar ruin smaller and smaller as it approaches the cap. Vanilla does not use this.\nOld default: true",
                false
            );
            LunarRuinDamageCap = config.BindOptionSteppedSlider(
                _configSectionName,
                "Lunar ruin damage cap",
                "The maximum damage increase that stacking lunar ruin can reach. Vanilla does not have a maximum damage increase. Set to -1 for no maximum damage boost.\nOld default: 145",
                -1,
                1,
                -1, 1000
            );
            /*AllowHiddenDot = config.BindOption(
                _configSectionName,
                "Enable hidden lunar ruin damage-over-time debuff",
                "Toggles application of lunar ruin's hidden damage-over-time debuff. It deals little bit of damage every 3 seconds, up to 2 times, and contributes to death mark alongside lunar ruin itself.",
                false
            );*/
            EnableLoggingDamageIncrease = config.BindOption(
                _configSectionName,
                "Enable logging the new damage increase from lunar ruin.",
                "If you're re-configuring the mod and want to know how much of a damage increase lunar ruin is giving, enable this option.",
                false
            );

            DamageIncreaseForSkillsOnly.SettingChanged += OnConfigOptionChanged;
            DamageIncreasePerLunarRuin.SettingChanged += OnConfigOptionChanged;
            EnableDiminishingDamage.SettingChanged += OnConfigOptionChanged;
            LunarRuinDamageCap.SettingChanged += OnConfigOptionChanged;
        }
    }
}