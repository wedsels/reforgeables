using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace reforgeables.code;

internal static class ItemExtensions {
    internal static bool Armor( this Item item ) => item.defense > 0 && !item.accessory;
    internal static bool Endless( this Item item, bool potential = false ) => item.type > ItemID.None && ( item.damage > 0 || item.ammo > AmmoID.None ) && ( potential && item.maxStack > 1 || item.maxStack == 1 );
    internal static bool CanPrefix( this Item item ) => item.Armor() || item.Endless() || item.accessory || item.damage > 0 && item.maxStack == 1;
	internal static HashSet< PrefixCategory > Category( this Item item ) => PrefixItem.Categories[ item.DamageType ];
}