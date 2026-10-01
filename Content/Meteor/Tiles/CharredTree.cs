using Everware.Content.Base.Tiles;
using Everware.Content.Meteor.Tiles;
using Terraria.ID;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Everware.Content.Meteor.Tiles
{
    public class CharredTree : ModTree
    {
        private Asset<Texture2D> texture;
        private Asset<Texture2D> branchesTexture;
        private Asset<Texture2D> topsTexture;


        public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
        {
            UseSpecialGroups = true,
            SpecialGroupMinimalHueValue = 11f / 72f,
            SpecialGroupMaximumHueValue = 0.25f,
            SpecialGroupMinimumSaturationValue = 0.88f,
            SpecialGroupMaximumSaturationValue = 1f

        };

        public override void SetStaticDefaults()
        {
            GrowsOnTileId = [ModContent.TileType<CharredSoilTile>()];
            texture = ModContent.Request<Texture2D>("Everware/Assets/Textures/Meteor/Tiles/CharredTreeTrunks");
            branchesTexture = ModContent.Request<Texture2D>("Everware/Assets/Textures/Meteor/Tiles/CharredTreeBranches");
            topsTexture = ModContent.Request<Texture2D>("Everware/Assets/Textures/Meteor/Tiles/CharredTreeTops");
        }
        public override Asset<Texture2D> GetTexture()
        {
            return texture;
        }

        public override void SetTreeFoliageSettings(int i, int j, Tile tile, int xoffset, ref int treeFrame, int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
        {
           
        }

        public override Asset<Texture2D> GetBranchTextures() => branchesTexture;


        public override Asset<Texture2D> GetTopTextures() => topsTexture;

        public override int DropWood() //apparently i need to keep this here otherwise it wont build?? 
        {
            return 0;
        }

    }
}