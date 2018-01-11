using Nez.Maths;

namespace Nez.AI.BehaviorTree.Conditionals
{
	/// <summary>
	///     returns success when the random probability is above the successProbability probability. It will otherwise return
	///     failure.
	///     successProbability should be between 0 and 1.
	/// </summary>
	public class RandomProbability<T> : Behavior<T>, IConditional<T>
    {
	    /// <summary>
	    ///     The chance that the task will return success
	    /// </summary>
	    private readonly int _successProbability;


        public RandomProbability(int successProbability)
        {
            _successProbability = successProbability;
        }


        public override TaskStatus Update(T context)
        {
            if (Random.NextFloat() > _successProbability)
                return TaskStatus.Success;
            return TaskStatus.Failure;
        }
    }
}