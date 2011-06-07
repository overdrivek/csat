#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Reflection;

namespace CSatEng
{
    public class Script
    {
        const string programName = "testi";

        static Assembly AssemblyInfo = Assembly.LoadFrom(programName + ".exe");
        Type loadedClass;
        object classInstance;

        public void LoadClass(string className)
        {
            loadedClass = AssemblyInfo.GetType(programName + className);
            classInstance = Activator.CreateInstance(loadedClass);
        }
        public object RunMethod(string methodName, object[] parameters)
        {
            MethodInfo method = loadedClass.GetMethod(methodName);
            return method.Invoke(classInstance, parameters);
        }
    }
}
