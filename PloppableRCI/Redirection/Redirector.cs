using System;
using System.Reflection;

namespace PloppableRICO.Redirection
{
    public class Redirector
    {
        private RedirectCallsState state;
        private readonly IntPtr site;
        private readonly IntPtr target;

        public Redirector(MethodInfo from, MethodInfo to)
        {
            site = from.MethodHandle.GetFunctionPointer();
            target = to.MethodHandle.GetFunctionPointer();
        }

        public void Apply()
        {
            if (Deployed) return;
            state = RedirectionHelper.PatchJumpTo(site, target);
            Deployed = true;
        }

        public void Revert()
        {
            if (!Deployed) return;
            RedirectionHelper.RevertJumpTo(site, state);
            Deployed = false;
        }

        public bool Deployed { get; private set; }
    }
}
