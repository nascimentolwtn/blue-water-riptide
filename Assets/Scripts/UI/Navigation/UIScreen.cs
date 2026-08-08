using UnityEngine;

namespace BlueWaterRiptide.UI.Navigation
{
    /// <summary>
    /// Base for every menu/HUD-overlay screen UIStateController can push. Screens are plain
    /// MonoBehaviours (they live on instantiated UI prefabs) that only ever read Core services
    /// and call intent methods on them per Plan 04 §4's one-way data flow — never edit that
    /// contract from a concrete screen.
    /// OnPush/OnPop fire exactly once per stack entry/exit; OnFocus/OnFocusLost fire every time
    /// this screen becomes/stops being the top of the stack, which can happen more than once for
    /// the same instance (e.g. a screen pushed on top of it is later popped, restoring focus).
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        /// <summary>Called once, immediately after this screen is instantiated and added to the stack.</summary>
        public virtual void OnPush()
        {
        }

        /// <summary>Called once, immediately before this screen is removed from the stack and destroyed.</summary>
        public virtual void OnPop()
        {
        }

        /// <summary>Called whenever this screen becomes the top of the stack (on push, and when a screen above it is popped).</summary>
        public virtual void OnFocus()
        {
        }

        /// <summary>Called whenever this screen stops being the top of the stack because another screen was pushed above it.</summary>
        public virtual void OnFocusLost()
        {
        }
    }
}
