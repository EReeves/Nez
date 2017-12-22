using System.Globalization;

namespace Nez.Svg
{
    public class SvgScale : SvgTransform
    {
        private readonly float _scaleX;
        private readonly float _scaleY;


        public SvgScale(float x, float y)
        {
            _scaleX = x;
            _scaleY = y;

            Matrix = Matrix2D.CreateScale(_scaleX, _scaleY);
        }


        public SvgScale(float x) : this(x, x)
        {
        }


        public override string ToString()
        {
            if (_scaleX == _scaleY)
                return string.Format(CultureInfo.InvariantCulture, "scale({0})", _scaleX);
            return string.Format(CultureInfo.InvariantCulture, "scale({0}, {1})", _scaleX, _scaleY);
        }
    }
}