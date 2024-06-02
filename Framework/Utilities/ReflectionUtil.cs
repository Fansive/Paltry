using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
public class ReflectionUtil  
{
    public readonly static Assembly CurAssembly = Assembly.GetExecutingAssembly();
    public readonly static Type[] CurTypes = CurAssembly.GetTypes();

    public static Type[] GetSubTypes(Type baseType)
    {
        return CurTypes.Where(t=>t.IsSubclassOf(baseType)).ToArray();
    }
    //public static Type[] GetSubTypesOfGeneric(Type baseGenericType)
    //{
    //    return curTypes.Where(t=>baseGenericType.getgen)
    //}
}
 