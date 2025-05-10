using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;

namespace LunarRuinDamageNerf
{
    public static class ConfigOptions
    {
        private const string _configSectionName = "Lunar Ruin Damage Nerf";


        public static ConfigEntry<float> DamageIncreasePerLunarRuin;
        private static void DamageIncreasePerLunarRuin_SettingChanged(object sender, EventArgs e)
        {
            Main.UpdateLunarRuinDescription();
        }


        public static ConfigEntry<bool> EnableDiminishingDamage;
        private static void EnableDiminishingDamage_SettingChanged(object sender, EventArgs e)
        {
            Main.UpdateLunarRuinDescription();
        }


        public static ConfigEntry<float> LunarRuinDamageCap;
        private static void LunarRuinDamageCap_SettingChanged(object sender, EventArgs e)
        {
            Main.UpdateLunarRuinDescription();
        }


        public static ConfigEntry<bool> EnableLoggingDamageIncrease;


        internal static void BindConfigOptions(ConfigFile config)
        {
            DamageIncreasePerLunarRuin = config.Bind<float>(
                _configSectionName,
                "Damage increase per lunar ruin stack", 10,
                "The increase to incoming damage for each stack of lunar ruin. Value is a percentage, so 5 means lunar ruin will increase all incoming damage by 5%. Vanilla's value is 10."
            );
            EnableDiminishingDamage = config.Bind<bool>(
                _configSectionName,
                "Enable diminishing/hyperbolic scaling for lunar ruin`s damage increase.", true,
                "If enabled, will make the damage increase for each stack of lunar ruin smaller and smaller as it approaches the cap. Vanilla does not use this."
            );
            LunarRuinDamageCap = config.Bind<float>(
                _configSectionName,
                "Lunar ruin damage cap", 145,
                "The maximum damage increase that stacking lunar ruin can reach. Set to -1 to not have a cap, which vanilla doesn't have."
            );
            EnableLoggingDamageIncrease = config.Bind<bool>(
                _configSectionName,
                "Enable logging the new damage increase from lunar ruin.", false,
                "If you're re-configuring the mod and want to know how much of a damage increase lunar ruin is giving, enable this option."
            );

            if (ModSupport.RiskOfOptionsMod.ModIsRunning)
            {
                DamageIncreasePerLunarRuin.SettingChanged += DamageIncreasePerLunarRuin_SettingChanged;
                EnableDiminishingDamage.SettingChanged += EnableDiminishingDamage_SettingChanged;
                LunarRuinDamageCap.SettingChanged += LunarRuinDamageCap_SettingChanged;
            }
        }
    }
}