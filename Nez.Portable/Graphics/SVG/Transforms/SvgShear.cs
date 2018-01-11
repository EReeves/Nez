using System.Globalization;

namespace Nez.Graphics.SVG.Transforms
{
    public class SvgShear : SvgTransform
    {
        private readonly float _shearX;
        private readonly float _shearY;


        public SvgShear(float shearX, float shearY)
        {
            _shearX = shearX;
            _shearY = shearY;
            Debug.Debug.Warn("SvgSkew shear is not implemented");
        }


        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "shear({0}, {1})", _shearX, _shearY);
        }
    }
}