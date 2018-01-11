using Nez.Utils;

namespace Nez.AI.BehaviorTree
{
	/// <summary>
	///     root class used to control a BehaviorTree. Handles storing the context
	/// </summary>
	public class BehaviorTree<T>
    {
	    /// <summary>
	    ///     The context should contain all the data needed to run the tree
	    /// </summary>
	    private readonly T _context;

        private float _elapsedTime;

	    /// <summary>
	    ///     root node of the tree
	    /// </summary>
	    private readonly Behavior<T> _root;

	    /// <summary>
	    ///     how often the behavior tree should update. An updatePeriod of 0.2 will make the tree update 5 times a second.
	    /// </summary>
	    public float UpdatePeriod;


        public BehaviorTree(T context, Behavior<T> rootNode, float updatePeriod = 0.2f)
        {
            _context = context;
            _root = rootNode;

            this.UpdatePeriod = _elapsedTime = updatePeriod;
        }


        public void Tick()
        {
            // updatePeriod less than or equal to 0 will tick every frame
            if (UpdatePeriod > 0)
            {
                _elapsedTime -= Time.DeltaTime;
                if (_elapsedTime <= 0)
                {
                    // ensure we only tick once for long frames
                    while (_elapsedTime <= 0)
                        _elapsedTime += UpdatePeriod;

                    _root.Tick(_context);
                }
            }
            else
            {
                _root.Tick(_context);
            }
        }
    }
}