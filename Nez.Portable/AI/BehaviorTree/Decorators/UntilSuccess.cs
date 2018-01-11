using Nez.Debug;

namespace Nez.AI.BehaviorTree.Decorators
{
	/// <summary>
	///     will keep executing its child task until the child task returns success
	/// </summary>
	public class UntilSuccess<T> : Decorator<T>
    {
        public override TaskStatus Update(T context)
        {
            Assert.IsNotNull(Child, "child must not be null");

            var status = Child.Tick(context);

            if (status != TaskStatus.Success)
                return TaskStatus.Running;

            return TaskStatus.Success;
        }
    }
}