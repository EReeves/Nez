using System;

namespace Nez.AI.UtilityAI.Actions
{
	/// <summary>
	///     wraps an Action for use as an IAction without having to create a new class
	/// </summary>
	public class ActionExecutor<T> : IAction<T>
    {
        private readonly Action<T> _action;


        public ActionExecutor(Action<T> action)
        {
            _action = action;
        }


        void IAction<T>.Execute(T context)
        {
            _action(context);
        }
    }
}