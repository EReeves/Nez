using Nez.Textures;

namespace Nez.BitmapFonts
{
    public class BitmapFontRegion
    {
        public char Character;
        public Subtexture Subtexture;
        public int XAdvance;
        public int XOffset;
        public int YOffset;


        public BitmapFontRegion(Subtexture textureRegion, char character, int xOffset, int yOffset, int xAdvance)
        {
            Subtexture = textureRegion;
            this.Character = character;
            this.XOffset = xOffset;
            this.YOffset = yOffset;
            this.XAdvance = xAdvance;
        }

        public int Width => Subtexture.SourceRect.Width;

        public int Height => Subtexture.SourceRect.Height;
    }
}