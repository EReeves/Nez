namespace Nez.Input
{
    public interface IClipboard
    {
        string GetContents();
        void SetContents(string text);
    }
}