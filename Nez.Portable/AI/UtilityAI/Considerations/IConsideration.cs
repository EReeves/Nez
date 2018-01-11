using Nez.AI.UtilityAI.Actions;

namespace Nez.AI.UtilityAI.Considerations
{
	/// <summary>
	///     encapsulates an Action and generates a score that a Reasoner can use to decide which Consideration to use
	/// </summary>
	public interface IConsideration<T>
    {
        IAction<T> Action { get; set; }

        float GetScore(T context);
    }
}