using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gemity.Common
{
    public class GemiPool<T> where T : new()
    {
        protected static Stack<T> Pool = new();
        public static T Pop()
        {
            if (Pool.Count == 0)
                Pool.Push(new T());

            return Pool.Pop();
        }

        public static void Push(T t)
        {
            Pool.Push(t);
        }
    }
}
