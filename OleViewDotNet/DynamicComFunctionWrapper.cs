using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace OleViewDotNet
{
    class DynamicComFunctionWrapper : DynamicObject
    {
        private MethodInfo _mi;
        private object _target;
        private COMRegistry _registry;

        public DynamicComFunctionWrapper(COMRegistry registry, MethodInfo mi, object target)
        {
            _mi = mi;
            _target = target;
            _registry = registry;
        }

        private static object[] UnwrapArray(object[] args, int outSize)
        {
            if (args != null)
            {
                object[] ret = new object[args.Length + outSize];

                for (int i = 0; i < args.Length; ++i)
                {
                    // Convert lists to object arrays (should do more here?)
                    if (args[i] is IList)
                    {
                        ret[i] = ((IList)args[i]).Cast<object>().ToArray();
                    }

                    ret[i] = DynamicComObjectWrapper.Unwrap(args[i]);
                }

                return ret;
            }

            return args;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {            
            List<Type> outReturnTypes = new List<Type>();
            List<object> returns = new List<object>();
            int startIndex = 0;

            ParameterInfo[] pis = _mi.GetParameters();

            for (startIndex = pis.Length; startIndex > 0; --startIndex)
            {
                ParameterInfo pi = pis[startIndex - 1];

                if ((pi.Attributes & ParameterAttributes.Out) == ParameterAttributes.Out)
                {
                    outReturnTypes.Insert(0, pi.ParameterType);
                }
                else
                {
                    break;
                }
            }

            object[] uargs = UnwrapArray(args, outReturnTypes.Count);

            // Possible varargs
            if ((outReturnTypes.Count == 0) && (pis.Length > 0) && (uargs.Length >= pis.Length) && (pis[pis.Length - 1].ParameterType == typeof(object[])))
            {                
                // Copy up to last argument (pis.Length - 1)
                object[] xargs = new object[pis.Length];
                Array.Copy(uargs, xargs, pis.Length - 1);

                // Copy remaining parameters into a trailing array
                object[] targs = new object[uargs.Length - pis.Length + 1];

                Array.Copy(uargs, pis.Length - 1, targs, 0, targs.Length);

                xargs[pis.Length - 1] = targs;

                uargs = xargs;
            }

            object ret = _mi.Invoke(_target, uargs);

            if (_mi.ReturnType != typeof(void))
            {
                returns.Add(DynamicComObjectWrapper.Wrap(_registry, ret, _mi.ReturnType));
            }

            // Promote out parameters
            if (outReturnTypes.Count > 0)
            {
                for (int i = 0; i < outReturnTypes.Count; ++i)
                {
                    returns.Add(DynamicComObjectWrapper.Wrap(_registry, uargs[startIndex + i], outReturnTypes[i]));
                }
            }

            if (returns.Count == 0)
            {
                ret = null;
            }
            else if (returns.Count == 1)
            {
                ret = returns[0];
            }
            else
            {
                ret = Activator.CreateInstance(typeof(Tuple<>).MakeGenericType(returns.Select(o => o != null ? o.GetType() : typeof(object)).ToArray()), returns.ToArray());
            }

            result = ret;

            return true;
        }
    }
}
