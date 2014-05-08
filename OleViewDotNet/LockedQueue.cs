//    This file is part of OleViewDotNet.
//    Copyright (C) James Forshaw 2014
//
//    OleViewDotNet is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    OleViewDotNet is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with OleViewDotNet.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Collections;

namespace OleViewDotNet
{
    /// <summary>
    /// Generic queue which can be used in a multi-threaded app and waits for a queue value
    /// </summary>
    /// <typeparam name="T">Type of object to queue, must be a reference type</typeparam>
    public sealed class LockedQueue<T> : IDisposable, IEnumerable<T> where T : class
    {
        private BlockingCollection<T> _queue;
        private CancellationToken _token; 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="limit">The bound of the queue, trying to add more than this number will block the queue</param>
        /// <param name="token">A cancellation token which can be used to cancel any pending operations</param>
        public LockedQueue(int limit, CancellationToken token)
        {
            _queue = limit >= 0 ? new BlockingCollection<T>(limit) : new BlockingCollection<T>();
            _token = token;
        }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="limit">The bound of the queue, trying to add more than this number will block the queue</param>
        public LockedQueue(int limit)
            : this(limit, CancellationToken.None)
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LockedQueue()
            : this(-1)
        {
        }

        /// <summary>
        /// Enqueue a new item (thread safe)
        /// </summary>
        /// <param name="item">The item to queue</param>
        /// <param name="milliSecondsTimeout">Timeout to wait for enqueue, only an issue if using a limited queue</param>
        /// <returns>Returns true if successfully queued</returns>
        /// <exception cref="InvalidOperationException">The timeout value is invalid</exception>
        /// <exception cref="ObjectDisposedException">The object was disposed</exception>
        /// <exception cref="OperationCanceledException">The operation was cancelled</exception>
        public bool Enqueue(T item, int milliSecondsTimeout)
        {
            return _queue.TryAdd(item, milliSecondsTimeout, _token);
        }

        /// <summary>
        /// Enqueue a new item (thread safe)
        /// </summary>
        /// <param name="item">The item to queue</param>      
        /// <exception cref="ObjectDisposedException">The object was disposed</exception>
        /// <exception cref="OperationCanceledException">The operation was cancelled</exception>
        public void Enqueue(T item)
        {
            Enqueue(item, Timeout.Infinite);
        }

        /// <summary>
        /// Dequeue an item, waiting for a specified time
        /// </summary>
        /// <param name="milliSecondsTimeout">The timeout if required</param>
        /// <param name="ret">The location of the return parameters</param>
        /// <returns>True if an item was successfully read from the queue, false otherwise</returns>
        /// <exception cref="ObjectDisposedException">The object was disposed</exception>
        /// <exception cref="OperationCanceledException">The opertion was cancelled</exception>
        public bool Dequeue(int milliSecondsTimeout, out T ret)
        {
            ret = null;

            if (_queue.TryTake(out ret, milliSecondsTimeout, _token))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Dequeue an item, waiting for ever
        /// </summary>        
        /// <returns>The item (null will be returned if the queue has been stopped)</returns>
        public T Dequeue()
        {
            T ret = null;

            if (!Dequeue(Timeout.Infinite, out ret))
            {
                return null;
            }
            else
            {
                return ret;
            }
        }

        /// <summary>
        /// Gets the count of items in the queue
        /// </summary>
        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        /// <summary>
        /// Stop the queue and try and unlock all waiting threads
        /// </summary>
        public void Stop()
        {
            try
            {
                _queue.CompleteAdding();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (NullReferenceException)
            {
                // Seen a few times this has been generated.
            }
        }

        /// <summary>
        /// Indicates whether this queue has been stopped and there is no more data
        /// left to read
        /// </summary>
        public bool IsStopped
        {
            get
            {
                try
                {
                    return _queue.IsCompleted;
                }
                catch (ObjectDisposedException)
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~LockedQueue()
        {
            _queue.Dispose();
        }

        /// <summary>
        /// Implemetation of Dispose
        /// </summary>
        void IDisposable.Dispose()
        {
            _queue.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            T obj = Dequeue();

            while (obj != null)
            {
                yield return obj;
                obj = Dequeue();
            }
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
