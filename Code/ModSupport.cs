using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using UnityEngine;
using RoR2;
using RiskOfOptions;
using RiskOfOptions.Options;

namespace LunarRuinDamageNerf
{
    internal static class ModSupport
    {
        internal static class RiskOfOptionsMod
        {
            private static bool? _modexists;
            public static bool ModIsRunning
            {
                get
                {
                    _modexists ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RiskOfOptions.PluginInfo.PLUGIN_GUID);
                    return (bool)_modexists;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal static void AddOptions()
            {
                ModSettingsManager.SetModDescription("Nerfs the damage bonus from lunar ruin, makes stacking it give diminishing damage increases, and sets a maximum value for it. All configurable!");

                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.DamageIncreasePerLunarRuin
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.EnableDiminishingDamage
                    )
                );
                ModSettingsManager.AddOption(
                    new FloatFieldOption(
                        ConfigOptions.LunarRuinDamageCap
                    )
                );
                ModSettingsManager.AddOption(
                    new CheckBoxOption(
                        ConfigOptions.EnableLoggingDamageIncrease
                    )
                );
            }
        }
    }
}