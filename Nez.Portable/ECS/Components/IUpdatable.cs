using System.Collections.Generic;

namespace Nez.ECS.Components
{
	/// <summary>
	///     interface that when added to a Component lets Nez know that it wants the update method called each frame as long as
	///     the Component
	///     and Entity are enabled.
	/// </summary>
	public interface IUpdatable
    {
        bool Enabled { get; }
        int UpdateOrder { get; }

        void Update();
    }

	public interface IUpdatableFixed
	{
		bool Enabled { get; }
		void FixedUpdate();
	}


	/// <summary>
	///     Comparer for sorting IUpdatables
	/// </summary>
	public class UpdatableComparer : IComparer<IUpdatable>
    {
        public int Compare(IUpdatable a, IUpdatable b)
        {
            return a.UpdateOrder.CompareTo(b.UpdateOrder);
        }
    }
}