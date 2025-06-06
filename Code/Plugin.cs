using System;
using BepInEx;
using RoR2;
using R2API;

namespace LunarRuinDamageNerf
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "LunarRuinDamageNerf";
        public const string PluginVersion = "1.0.1";
        public void Awake()
        {
            Log.Init(Logger);
            ConfigOptions.BindConfigOptions(Config);
            if (ModSupport.RiskOfOptionsMod.ModIsRunning)
            {
                ModSupport.RiskOfOptionsMod.AddOptions();
            }
            IL.RoR2.HealthComponent.TakeDamageProcess += Main.HealthComponent_TakeDamageProcess;
            On.RoR2.UI.CharacterSelectController.Awake += Main.CharacterSelectController_Awake;
            On.RoR2.UI.CharacterSelectController.OnDisable += Main.CharacterSelectController_OnDisable;
            RoR2Application.onLoad += () =>
            {
                Main.UpdateLunarRuinDescription();
            };
        }
    }
}