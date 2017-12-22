namespace Nez
{
	/// <summary>
	///     prep for a proper multi-platform clipboard system. For now it just mocks the clipboard and will only work in-app
	/// </summary>
	public class Clipboard : IClipboard
    {
        private static IClipboard _instance;
        private string _text;


        public static string GetContents()
        {
            if (_instance == null)
                _instance = new Clipboard();
            return _instance.GetContents();
        }


        public static void SetContents(string text)
        {
            if (_instance == null)
                _instance = new Clipboard();
            _instance.SetContents(text);
        }


        #region IClipboard implementation

        string IClipboard.GetContents()
        {
            return _text;
        }


        void IClipboard.SetContents(string text)
        {
            _text = text;
        }

        #endregion
    }
}