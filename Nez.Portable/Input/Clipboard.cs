using System;
using System.Runtime.InteropServices;

namespace Nez.Input
{
	/// <summary>
	///     prep for a proper multi-platform clipboard system. For now it just mocks the clipboard and will only work in-app
	/// </summary>
	public class Clipboard : IClipboard
    {
        private static IClipboard _instance;
        private string _text;

        [DllImport("./x64/libSDL2-2.0.so.0",EntryPoint = "SDL_SetClipboardText")]
        private static extern int SDL_SetClipboardTextLinux(string text);
        [DllImport("./x64/libSDL2-2.0.so.0",EntryPoint = "SDL_GetClipboardText")]
        private static extern string SDL_GetClipboardTextLinux();
        [DllImport("./x64/SDL2.dll",EntryPoint = "SDL_SetClipboardText")]
        private static extern int SDL_SetClipboardTextWin(string text);
        [DllImport("./x64/SDL2.dll",EntryPoint = "SDL_GetClipboardText")]
        private static extern string SDL_GetClipboardTextWin();

        public static string GetContents()
        {
            if (_instance == null)
                _instance = new Clipboard();
           #if LINUX
            return SDL_GetClipboardTextLinux();
           #elif WIN
            return SDL_GetClipboardTextWin();
           #endif
            throw new Exception("Platform not defined? WIN/LINUX");
        }


        public static void SetContents(string text)
        {
            if (_instance == null)
                _instance = new Clipboard();
            #if LINUX
             SDL_SetClipboardTextLinux(text);
            return;
           #elif WIN
             SDL_SetClipboardTextWin(text);
            return;
           #endif
            throw new Exception("Platform not defined? WIN/LINUX");
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