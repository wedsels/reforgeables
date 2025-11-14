using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.DataStructures;

namespace reforgeables.code;

internal class EndlessItem : GlobalItem {
    [ CloneByReference ]
    internal bool Endless = false;

    public override bool InstancePerEntity => true;

    public override bool CanStack( Item destination, Item source ) => !Endless;
    public override bool ConsumeItem( Item item, Player player ) => !Endless;
    public override bool CanBeConsumedAsAmmo( Item ammo, Item weapon, Player player ) => !Endless;

    internal void SetEndless( Item item ) {
        Endless = true;
        item.maxStack = 1;
        item.material = false;
        item.consumable = false;
        item.SetNameOverride( Language.GetTextValue( "RandomWorldName_Adjective.Infinite" ) + " " + Lang.GetItemNameValue( item.type ) );
    }

    public override void OnCreated( Item item, ItemCreationContext context ) {
        if ( context is RecipeItemCreationContext recipe && recipe.Recipe.Mod is Reforgeables )
            SetEndless( item );
    }

    public override void SetDefaults( Item item ) {
        if ( item.type > ItemID.None && item.damage > 0 && item.maxStack > 1 )
            item.maxStack = 9999;
    }

    public override bool PreDrawInInventory( Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale ) {
        if ( !Endless )
            return true;

        int count = 5;
        float radius = 8.0f;

        for ( int i = 0; i < count; i++ ) {
            float x = ( i - ( count - 1 ) / 2.0f ) * 4.5f;
            float y = -MathF.Sqrt( MathF.Max( 0, radius * radius - x * x ) ) + 4.5f;

            spriteBatch.Draw(
                TextureAssets.Item[ item.type ].Value,
                position + new Vector2( x, y ),
                frame,
                Color.Lerp( drawColor, Color.Black, count * 0.1f - i * ( 1.0f / count ) ),
                -0.5f + 0.2f * i,
                origin,
                scale,
                SpriteEffects.None,
                0.0f
            );
        }

        return false;
    }

    public override void NetSend( Item item, BinaryWriter writer ) => writer.Write( Endless );
    public override void NetReceive( Item item, BinaryReader reader ) {
        if ( reader.ReadBoolean() )
            SetEndless( item );
    }

    public override void SaveData( Item item, TagCompound tag ) {
        tag[ "Prefix" ] = item.prefix;
        tag[ "Endless" ] = Endless;
    }

    public override void LoadData( Item item, TagCompound tag ) {
        if ( Endless = tag.GetBool( "Endless" ) ) {
            SetEndless( item );
            item.Prefix( tag.GetInt( "Prefix" ) );
        }
    }
}

internal class RecipeSystem : ModSystem {
    public override void PostWorldLoad() => Main.recipe.ToList().ForEach( x => { if ( x.Mod is Reforgeables ) x.createItem.GetGlobalItem< EndlessItem >().SetEndless( x.createItem ); } );
    public override void AddRecipes() {
        foreach ( var i in ContentSamples.ItemsByType.Values ) {
            if ( !i.Endless( true ) )
                continue;

            Recipe r = Recipe.Create( i.type );
            r.AddIngredient( i.type, 3996 );
            r.AddTile( TileID.CrystalBall );
            r.Register();
        }
    }

    public override void PostSetupContent() {
		On_Recipe.CollectItems_IEnumerable1 += ( orig, items ) => {
            var l = items.ToList();

            for ( int i = l.Count - 1; i >= 0; i-- )
                if ( l[ i ].TryGetGlobalItem( out EndlessItem e ) && e.Endless )
                    l.RemoveAt( i );

            orig( l );
        };

        On_Recipe.ConsumeForCraft += ( On_Recipe.orig_ConsumeForCraft orig, Recipe self, Item item, Item requiredItem, ref int stackRequired ) => {
            if ( item.TryGetGlobalItem( out EndlessItem e ) && e.Endless )
                return false;

            return orig( self, item, requiredItem, ref stackRequired );
        };
    }
}