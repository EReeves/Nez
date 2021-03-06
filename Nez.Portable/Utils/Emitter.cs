﻿using System;
using System.Collections.Generic;
using Nez.Debug;

namespace Nez.Utils
{
	/// <summary>
	///     simple event emitter that is designed to have its generic contraint be either an int or an enum
	/// </summary>
	public class Emitter<T> where T : struct, IComparable, IFormattable
    {
        private readonly Dictionary<T, List<Action>> _messageTable;


        public Emitter()
        {
            _messageTable = new Dictionary<T, List<Action>>();
        }


	    /// <summary>
	    ///     if using an enum as the generic constraint you may want to pass in a custom comparer to avoid boxing/unboxing. See
	    ///     the CoreEventsComparer
	    ///     for an example implementation.
	    /// </summary>
	    /// <param name="customComparer">Custom comparer.</param>
	    public Emitter(IEqualityComparer<T> customComparer)
        {
            _messageTable = new Dictionary<T, List<Action>>(customComparer);
        }


        public void AddObserver(T eventType, Action handler)
        {
            List<Action> list = null;
            if (!_messageTable.TryGetValue(eventType, out list))
            {
                list = new List<Action>();
                _messageTable.Add(eventType, list);
            }

            Assert.IsFalse(list.Contains(handler), "You are trying to add the same observer twice");
            list.Add(handler);
        }


        public void RemoveObserver(T eventType, Action handler)
        {
            // we purposely do this in unsafe fashion so that it will throw an Exception if someone tries to remove a handler that
            // was never added
            _messageTable[eventType].Remove(handler);
        }


        public void Emit(T eventType)
        {
            List<Action> list = null;
            if (_messageTable.TryGetValue(eventType, out list))
                for (var i = list.Count - 1; i >= 0; i--)
                    list[i]();
        }
    }


	/// <summary>
	///     simple event emitter that is designed to have its generic contraint be either an int or an enum. this variant lets
	///     you pass around
	///     data with each event. See InputEvent for an example.
	/// </summary>
	public class Emitter<T, TU> where T : struct, IComparable, IFormattable
    {
        private readonly Dictionary<T, List<Action<TU>>> _messageTable;


        public Emitter()
        {
            _messageTable = new Dictionary<T, List<Action<TU>>>();
        }


	    /// <summary>
	    ///     if using an enum as the generic constraint you may want to pass in a custom comparer to avoid boxing/unboxing. See
	    ///     the CoreEventsComparer
	    ///     for an example implementation.
	    /// </summary>
	    /// <param name="customComparer">Custom comparer.</param>
	    public Emitter(IEqualityComparer<T> customComparer)
        {
            _messageTable = new Dictionary<T, List<Action<TU>>>(customComparer);
        }


        public void AddObserver(T eventType, Action<TU> handler)
        {
            List<Action<TU>> list = null;
            if (!_messageTable.TryGetValue(eventType, out list))
            {
                list = new List<Action<TU>>();
                _messageTable.Add(eventType, list);
            }

            Assert.IsFalse(list.Contains(handler), "You are trying to add the same observer twice");
            list.Add(handler);
        }


        public void RemoveObserver(T eventType, Action<TU> handler)
        {
            // we purposely do this in unsafe fashion so that it will throw an Exception if someone tries to remove a handler that
            // was never added
            _messageTable[eventType].Remove(handler);
        }


        public void Emit(T eventType, TU data)
        {
            List<Action<TU>> list = null;
            if (_messageTable.TryGetValue(eventType, out list))
                for (var i = list.Count - 1; i >= 0; i--)
                    list[i](data);
        }
    }
}