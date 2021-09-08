using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Runtime.Remoting.Messaging
{
    public static class CallContext2
    {
        static ConcurrentDictionary<string, AsyncLocal<object>> State = new ConcurrentDictionary<string, AsyncLocal<object>>();

        public static void SetData(string name, object data) =>
            State.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;

        public static object GetData(string name) =>
           State.TryGetValue(name, out var data) ? data.Value : null;
    }
}
