using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Hooker 池，防止重复 Hook
/// </summary>
public static class HookerPool
{
    private static Dictionary<MethodBase, MethodHooker> _hookers = new Dictionary<MethodBase, MethodHooker>();

    public static void AddHooker(MethodBase method, MethodHooker hooker)
    {
        MethodHooker preHooker;
        if (_hookers.TryGetValue(method, out preHooker))
        {
            preHooker.Uninstall();
            _hookers[method] = hooker;
        }
        else
            _hookers.Add(method, hooker);
    }

    public static void RemoveHooker(MethodBase method)
    {
        _hookers.Remove(method);
    }
}
