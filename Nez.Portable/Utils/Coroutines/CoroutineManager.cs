using System.Collections;
using System.Collections.Generic;

namespace Nez.Utils.Coroutines
{
	/// <summary>
	///     basic CoroutineManager. Coroutines can do the following:
	///     - yield return null (tick again the next frame)
	///     - yield return Coroutine.waitForSeconds( 3 ) (tick again after a 3 second delay)
	///     - yield return Coroutine.waitForSeconds( 5.5f ) (tick again after a 5.5 second delay)
	///     - yield return startCoroutine( another() ) (wait for the other coroutine before getting ticked again)
	/// </summary>
	public class CoroutineManager : IUpdatableManager
    {
	    /// <summary>
	    ///     flag to keep track of when we are in our update loop. If a new coroutine is started during the update loop we have
	    ///     to stick
	    ///     it in the shouldRunNextFrame List to avoid modifying a List while we iterate.
	    /// </summary>
	    private bool _isInUpdate;

        private readonly List<CoroutineImpl> _shouldRunNextFrame = new List<CoroutineImpl>();
        private readonly List<CoroutineImpl> _unblockedCoroutines = new List<CoroutineImpl>();


        void IUpdatableManager.Update()
        {
            _isInUpdate = true;
            for (var i = 0; i < _unblockedCoroutines.Count; i++)
            {
                var coroutine = _unblockedCoroutines[i];

                // check for stopped coroutines
                if (coroutine.IsDone)
                {
                    Pool<CoroutineImpl>.Free(coroutine);
                    continue;
                }

                // are we waiting for any other coroutines to finish?
                if (coroutine.WaitForCoroutine != null)
                    if (coroutine.WaitForCoroutine.IsDone)
                    {
                        coroutine.WaitForCoroutine = null;
                    }
                    else
                    {
                        _shouldRunNextFrame.Add(coroutine);
                        continue;
                    }

                // deal with timers if we have them
                if (coroutine.WaitTimer > 0)
                {
                    // still has time left. decrement and run again next frame being sure to decrement with the appropriate deltaTime.
                    coroutine.WaitTimer -= coroutine.UseUnscaledDeltaTime ? Time.UnscaledDeltaTime : Time.DeltaTime;
                    _shouldRunNextFrame.Add(coroutine);
                    continue;
                }

                if (TickCoroutine(coroutine))
                    _shouldRunNextFrame.Add(coroutine);
            }

            _unblockedCoroutines.Clear();
            _unblockedCoroutines.AddRange(_shouldRunNextFrame);
            _shouldRunNextFrame.Clear();

            _isInUpdate = false;
        }


	    /// <summary>
	    ///     adds the IEnumerator to the CoroutineManager. Coroutines get ticked before Update is called each frame.
	    /// </summary>
	    /// <returns>The coroutine.</returns>
	    /// <param name="enumerator">Enumerator.</param>
	    public ICoroutine StartCoroutine(IEnumerator enumerator)
        {
            // find or create a CoroutineImpl
            var coroutine = Pool<CoroutineImpl>.Obtain();
            coroutine.PrepareForReuse();

            // setup the coroutine and add it
            coroutine.Enumerator = enumerator;
            var shouldContinueCoroutine = TickCoroutine(coroutine);

            // guard against empty coroutines
            if (!shouldContinueCoroutine)
                return null;

            if (_isInUpdate)
                _shouldRunNextFrame.Add(coroutine);
            else
                _unblockedCoroutines.Add(coroutine);

            return coroutine;
        }


	    /// <summary>
	    ///     ticks a coroutine. returns true if the coroutine should continue to run next frame. This method will put finished
	    ///     coroutines
	    ///     back in the Pool!
	    /// </summary>
	    /// <returns><c>true</c>, if coroutine was ticked, <c>false</c> otherwise.</returns>
	    /// <param name="coroutine">Coroutine.</param>
	    private bool TickCoroutine(CoroutineImpl coroutine)
        {
            // This coroutine has finished
            if (!coroutine.Enumerator.MoveNext() || coroutine.IsDone)
            {
                Pool<CoroutineImpl>.Free(coroutine);
                return false;
            }

            if (coroutine.Enumerator.Current == null)
                return true;

            if (coroutine.Enumerator.Current is WaitForSeconds)
            {
                coroutine.WaitTimer = (coroutine.Enumerator.Current as WaitForSeconds).WaitTime;
                return true;
            }

#if DEBUG
            // deprecation warning for yielding an int/float
            if (coroutine.Enumerator.Current is int)
            {
                Debug.Debug.Error(
                    "yield Coroutine.waitForSeconds instead of an int. Yielding an int will not work in a release build.");
                coroutine.WaitTimer = (int) coroutine.Enumerator.Current;
                return true;
            }

            if (coroutine.Enumerator.Current is float)
            {
                Debug.Debug.Error(
                    "yield Coroutine.waitForSeconds instead of a float. Yielding a float will not work in a release build.");
                coroutine.WaitTimer = (float) coroutine.Enumerator.Current;
                return true;
            }
#endif

            if (coroutine.Enumerator.Current is CoroutineImpl)
            {
                coroutine.WaitForCoroutine = coroutine.Enumerator.Current as CoroutineImpl;
                return true;
            }
            // This coroutine yielded some value we don't understand. run it next frame.
            return true;
        }

	    /// <summary>
	    ///     internal class used by the CoroutineManager to hide the data it requires for a Coroutine
	    /// </summary>
	    private class CoroutineImpl : ICoroutine, IPoolable
        {
            public IEnumerator Enumerator;
            public bool IsDone;
            public bool UseUnscaledDeltaTime;
            public CoroutineImpl WaitForCoroutine;

	        /// <summary>
	        ///     anytime a delay is yielded it is added to the waitTimer which tracks the delays
	        /// </summary>
	        public float WaitTimer;


            public void Stop()
            {
                IsDone = true;
            }


            public ICoroutine SetUseUnscaledDeltaTime(bool useUnscaledDeltaTime)
            {
                this.UseUnscaledDeltaTime = useUnscaledDeltaTime;
                return this;
            }


            void IPoolable.Reset()
            {
                IsDone = true;
                WaitTimer = 0;
                WaitForCoroutine = null;
                Enumerator = null;
                UseUnscaledDeltaTime = false;
            }


            internal void PrepareForReuse()
            {
                IsDone = false;
            }
        }
    }
}