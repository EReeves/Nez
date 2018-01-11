using Nez.Utils.Extensions;

namespace Nez.AI.BehaviorTree.Composites
{
	/// <summary>
	///     Same as Sequence except it shuffles the children when started
	/// </summary>
	public class RandomSequence<T> : Sequence<T>
    {
        public override void OnStart()
        {
            Children.Shuffle();
        }
    }
}