using System.Globalization;

namespace Nez.Graphics.SVG.Transforms
{
    public class SvgSkew : SvgTransform
    {
        private readonly float _angleX;
        private readonly float _angleY;


        public SvgSkew(float angleX, float angleY)
        {
            _angleX = angleX;
            _angleY = angleY;

            Debug.Debug.Warn("SvgSkew matrix is not implemented");
            //matrix = Matrix2D.Shear(
            //	(float)System.Math.Tan( _angleX / 180 * MathHelper.Pi ),
            //	(float)System.Math.Tan( _angleY / 180 * MathHelper.Pi ) );
        }


        public override string ToString()
        {
            if (_angleY == 0)
                return string.Format(CultureInfo.InvariantCulture, "skewX({0})", _angleX);

            return string.Format(CultureInfo.InvariantCulture, "skewY({0})", _angleY);
        }
    }
}