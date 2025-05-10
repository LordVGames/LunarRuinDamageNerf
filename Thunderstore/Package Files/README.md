# LunarRuinDamageNerf

False son's lunar ruin from the start was almost too good when stacked a bunch, and now with the new + rebalanced skills it's much easier to spam lunar ruin and get what's practically a free win on any tanky enemy. To fix that (or at least make it not as absurd), this mod makes it so that each stack of lunar ruin adds less and less of a damage increase.

The following values are configurable (in-game if you have [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) installed):
- Damage increase per lunar ruin
- - Set to 10% by default like in vanilla, but with the other features of the mod it isn't really.
- Diminishing/hyperbolic scaling for stacking lunar ruin's damage
- - Enabled by default. How much the damage is reduced each stack depends on the lunar ruin damage increase cap.
- Lunar ruin damage increase cap
- - Set to 145 by default. <br> If hyperbolic scaling is enabled, it will never actually reach this value, but instead get closer and closer. The value of 145 makes it so that the first few stacks have a damage increase close to vanilla's, but gets lower and lower than vanilla the more it stacks. I.E. what would be 90% is down to 58%, and 300% is down to 100%.

There's also a setting to log the old and new damage increase from lunar ruin if you really want to fine tune the settings so that it scales how you want.