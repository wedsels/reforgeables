using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace reforgeables.code;

internal class PrefixItem : GlobalItem {
	internal static Dictionary< DamageClass, HashSet< PrefixCategory > > Categories = [];

    public override void SetDefaults( Item item ) {
		if ( !Categories.ContainsKey( item.DamageType ) )
			Categories[ item.DamageType ] = [];

		foreach ( var i in item.GetPrefixCategories() )
			Categories[ item.DamageType ].Add( i );

		if ( item.ammo > AmmoID.None )
        	item.useTime = 100;
    }

    public override float UseSpeedMultiplier( Item item, Player player ) {
		if ( item.useAmmo > AmmoID.None && player.ChooseAmmo( item ) is Item ammo )
			return 1.0f + 1.0f - ammo.useTime / 100.0f;
		return 1.0f;
	}
}

internal class System : ModSystem {
    public override void PostSetupContent() {
		On_Item.CanHavePrefixes += ( orig, item ) => item.type > ItemID.None && item.CanPrefix();
		On_Item.GetPrefixCategories += ( orig, item ) => item.CanPrefix() ? [ .. item.Category() ] : orig( item );
    }
}