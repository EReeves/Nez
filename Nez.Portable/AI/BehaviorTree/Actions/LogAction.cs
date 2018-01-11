namespace Nez.AI.BehaviorTree.Actions
{
	/// <summary>
	///     simple task which will output the specified text and return success. It can be used for debugging.
	/// </summary>
	public class LogAction<T> : Behavior<T>
    {
	    /// <summary>
	    ///     is this text an error
	    /// </summary>
	    public bool IsError;

	    /// <summary>
	    ///     text to log
	    /// </summary>
	    public string Text;


        public LogAction(string text)
        {
            this.Text = text;
        }


        public override TaskStatus Update(T context)
        {
            if (IsError)
                Debug.Debug.Error(Text);
            else
                Debug.Debug.Log(Text);

            return TaskStatus.Success;
        }
    }
}