using System;
using BepInEx;
using RoR2;
using R2API;
using MiscFixes.Modules;

namespace LunarRuinDamageNerf
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "LunarRuinDamageNerf";
        public const string PluginVersion = "1.1.1";
        public void Awake()
        {
            Log.Init(Logger);
            ConfigOptions.BindConfigOptions(Config);
            Config.WipeConfig();
            ILHooks.Setup();
            On.RoR2.UI.CharacterSelectController.Awake += Main.CharacterSelectController_Awake;
            On.RoR2.UI.CharacterSelectController.OnDisable += Main.CharacterSelectController_OnDisable;
            RoR2Application.onLoad += () =>
            {
                Main.UpdateLunarRuinDescription();
            };
        }
    }
}