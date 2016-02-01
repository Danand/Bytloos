using System;
using System.Collections.Generic;

namespace Bytloos
{
    /// <summary>
    /// Simple dispatcher pattern realization.
    /// </summary>
    /// <typeparam name="TKey">Type of event key.</typeparam>
    public static class Dispatcher<TKey>
    {
        private static readonly Dictionary<TKey, List<object>> handlersList = new Dictionary<TKey, List<object>>();

        /// <summary>
        /// Raises events for key.
        /// </summary>
        /// <param name="eventKey">Event key.</param>
        public static void Send(TKey eventKey)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action)handler).Invoke();
        }

        #region Senders

        public static void Send<T>(TKey eventKey, T eventInfo)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T>)handler).Invoke(eventInfo);
        }

        public static void Send<T1, T2>(TKey eventKey, T1 arg0, T2 arg1)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2>)handler).Invoke(arg0, arg1);
        }

        public static void Send<T1, T2, T3>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3>)handler).Invoke(arg0, arg1, arg2);
        }

        public static void Send<T1, T2, T3, T4>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4>)handler).Invoke(arg0, arg1, arg2, arg3);
        }
        public static void Send<T1, T2, T3, T4, T5>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5>)handler).Invoke(arg0, arg1, arg2, arg3, arg4);
        }

        public static void Send<T1, T2, T3, T4, T5, T6>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12, T14 arg13)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12, T14 arg13, T15 arg14)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }

        public static void Send<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(TKey eventKey, T1 arg0, T2 arg1, T3 arg2, T4 arg3, T5 arg4, T6 arg5, T7 arg6, T8 arg7, T9 arg8, T10 arg9, T11 arg10, T12 arg11, T13 arg12, T14 arg13, T15 arg14, T16 arg15)
        {
            foreach (var handler in handlersList[eventKey])
                ((Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>)handler).Invoke(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }

        #endregion

        /// <summary>
        /// Subscribes action with key.
        /// </summary>
        /// <param name="eventKey">Event key.</param>
        /// <param name="handler">Action.</param>
        public static void Subscribe(TKey eventKey, Action handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        #region Subscribers

        public static void Subscribe<T>(TKey eventKey, Action<T> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2>(TKey eventKey, Action<T1, T2> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3>(TKey eventKey, Action<T1, T2, T3> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4>(TKey eventKey, Action<T1, T2, T3, T4> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5>(TKey eventKey, Action<T1, T2, T3, T4, T5> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        public static void Subscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler)
        {
            if (!handlersList.ContainsKey(eventKey))
                handlersList.Add(eventKey, new List<object>());

            handlersList[eventKey].Add(handler);
        }

        #endregion

        /// <summary>
        /// Unsubscribes action with key.
        /// </summary>
        /// <param name="eventKey">Event key.</param>
        /// <param name="handler">Action.</param>
        public static void Unsubscribe(TKey eventKey, Action handler)
        {
            handlersList[eventKey].Remove(handler);
        }

        #region Unsubscribers

        public static void Unsubscribe<T>(TKey eventKey, Action<T> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2>(TKey eventKey, Action<T1, T2> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3>(TKey eventKey, Action<T1, T2, T3> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4>(TKey eventKey, Action<T1, T2, T3, T4> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5>(TKey eventKey, Action<T1, T2, T3, T4, T5> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> handler) { handlersList[eventKey].Remove(handler); }

        public static void Unsubscribe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(TKey eventKey, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> handler) { handlersList[eventKey].Remove(handler); }

        #endregion
    }
}
