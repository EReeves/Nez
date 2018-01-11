namespace Nez.AI.UtilityAI.Actions
{
	/// <summary>
	///     Action that logs text
	/// </summary>
	public class LogAction<T> : IAction<T>
    {
        private readonly string _text;


        public LogAction(string text)
        {
            _text = text;
        }


        void IAction<T>.Execute(T context)
        {
            Debug.Debug.Log(_text);
        }
    }
}