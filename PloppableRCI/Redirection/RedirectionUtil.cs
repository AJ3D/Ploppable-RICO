using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PloppableRICO.Redirection
{
    public static class RedirectionUtil
    {
        private const BindingFlags MethodFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags RedirectorFieldFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;

        public static Dictionary<MethodInfo, Redirector> RedirectType(Type type, bool onCreated = false)
        {
            var redirects = new Dictionary<MethodInfo, Redirector>();

            var customAttributes = type.GetCustomAttributes(typeof(TargetTypeAttribute), false);
            if (customAttributes.Length != 1)
            {
                return null;
            }
            var targetType = ((TargetTypeAttribute)customAttributes[0]).Type;
            RedirectMethods(type, targetType, redirects, onCreated);
            RedirectReverse(type, targetType, redirects, onCreated);
            return redirects;
        }

        private static void RedirectMethods(Type type, Type targetType, Dictionary<MethodInfo, Redirector> redirects, bool onCreated)
        {
            foreach (
                var method in
                    type.GetMethods(MethodFlags)
                        .Where(method =>
                        {
                            var redirectAttributes = method.GetCustomAttributes(typeof(RedirectMethodAttribute), false);
                            if (redirectAttributes.Length != 1)
                            {
                                return false;
                            }
                            return ((RedirectMethodAttribute)redirectAttributes[0]).OnCreated == onCreated;
                        }))
            {
                UnityEngine.Debug.Log($"Redirecting {targetType.Name}#{method.Name}...");
                var redirector = RedirectMethod(targetType, method, redirects);

                var redirectorField = type.GetField($"{method.Name}Redirector", RedirectorFieldFlags);
                if (redirectorField != null && redirectorField.FieldType == typeof (Redirector))
                {
                    UnityEngine.Debug.Log("Redirector field found!");
                    redirectorField.SetValue(null, redirector);
                }
            }
        }

        private static void RedirectReverse(Type type, Type targetType, Dictionary<MethodInfo, Redirector> redirects, bool onCreated)
        {
            foreach (
                var method in
                    type.GetMethods(MethodFlags)
                        .Where(method =>
                        {
                            var redirectAttributes = method.GetCustomAttributes(typeof(RedirectReverseAttribute), false);
                            if (redirectAttributes.Length == 1)
                            {
                                return ((RedirectReverseAttribute)redirectAttributes[0]).OnCreated == onCreated;
                            }
                            return false;
                        }))
            {
                UnityEngine.Debug.Log($"Redirecting reverse {targetType.Name}#{method.Name}...");
                RedirectMethod(targetType, method, redirects, true);
            }
        }

        private static Redirector RedirectMethod(Type targetType, MethodInfo method, Dictionary<MethodInfo, Redirector> redirects, bool reverse = false)
        {
            var tuple = RedirectMethod(targetType, method, reverse);
            redirects.Add(tuple.First, tuple.Second);

            return tuple.Second;
        }


        private static Tuple<MethodInfo, Redirector> RedirectMethod(Type targetType, MethodInfo detour, bool reverse)
        {
            var parameters = detour.GetParameters();
            Type[] types;
            if (parameters.Length > 0 && (
                (!targetType.IsValueType && parameters[0].ParameterType == targetType) ||
                (targetType.IsValueType && parameters[0].ParameterType == targetType.MakeByRefType())))
            {
                types = parameters.Skip(1).Select(p => p.ParameterType).ToArray();
            }
            else {
                types = parameters.Select(p => p.ParameterType).ToArray();
            }

            MethodInfo originalMethod = originalMethod = targetType.GetMethod(detour.Name, MethodFlags, null, types, null);
            if (originalMethod == null && detour.Name.EndsWith("Alt"))
            {
                originalMethod = targetType.GetMethod(detour.Name.Substring(0, detour.Name.Length - 3), MethodFlags, null, types, null);
            }

            var redirector = reverse ? new Redirector(detour, originalMethod) : new Redirector(originalMethod, detour);

            redirector.Apply();

            return Tuple.New(reverse ? detour : originalMethod, redirector);
        }
    }
}