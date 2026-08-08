using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlueWaterRiptide.UI.Navigation
{
    /// <summary>
    /// Owns the MainMenu screen stack (Plan 04 §4). Screens are registered at runtime by id
    /// rather than wired via [SerializeField] prefab arrays, so this class works with zero
    /// scene/prefab authoring — the concrete Home/Locker/ModeSelect/etc. screens are later,
    /// Editor-bound work that plugs into this via RegisterScreen.
    ///
    /// Stack semantics: screens below the top stay instantiated but inactive (SetActive(false))
    /// rather than being destroyed, so a screen's state (scroll position, entered lobby code,
    /// etc.) survives being covered by a pushed screen. A screen is only destroyed when it is
    /// itself popped, or when Reset() tears down the whole stack. Index 0 is the "base" screen
    /// (typically Home) and is never popped by Pop() — only Reset() replaces it.
    /// </summary>
    public class UIStateController : MonoBehaviour
    {
        /// <summary>A stack entry pairs the registered id back up with its instance — GameObject.name isn't reliable once Unity appends "(Clone)".</summary>
        readonly struct StackEntry
        {
            public readonly string ScreenId;
            public readonly UIScreen Screen;

            public StackEntry(string screenId, UIScreen screen)
            {
                ScreenId = screenId;
                Screen = screen;
            }
        }

        readonly Dictionary<string, Func<GameObject>> _factories = new Dictionary<string, Func<GameObject>>();
        readonly List<StackEntry> _stack = new List<StackEntry>();

        /// <summary>Fired after a screen finishes pushing (after OnPush/OnFocus have run).</summary>
        public event Action<string> OnScreenPushed;

        /// <summary>Fired after a screen finishes popping (after OnPop has run and the instance is destroyed).</summary>
        public event Action<string> OnScreenPopped;

        /// <summary>Fired when a back-request (hardware back / Escape) arrives while only the base screen is on the stack — callers decide what that means (e.g. an exit-app confirmation); this class never calls Application.Quit itself.</summary>
        public event Action OnBackRequestedAtRoot;

        public int Count => _stack.Count;
        public UIScreen Current => _stack.Count > 0 ? _stack[_stack.Count - 1].Screen : null;

        /// <summary>Registers a screenId against a prefab; Push() instantiates a fresh copy parented under this controller each time the id reaches the top of the stack for the first time.</summary>
        public void RegisterScreen(string screenId, GameObject prefab)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            RegisterScreen(screenId, () => Instantiate(prefab, transform));
        }

        /// <summary>Registers a screenId against a custom factory, for callers that need instantiation logic beyond a plain prefab (e.g. resolving an Addressable, or reusing a pooled instance).</summary>
        public void RegisterScreen(string screenId, Func<GameObject> factory)
        {
            if (string.IsNullOrEmpty(screenId)) throw new ArgumentException("screenId must not be null/empty.", nameof(screenId));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            _factories[screenId] = factory;
        }

        /// <summary>
        /// Instantiates/activates screenId's screen and pushes it on top of the stack. The
        /// previous top (if any) loses focus and is hidden but kept alive underneath.
        /// </summary>
        public void Push(string screenId)
        {
            if (!_factories.TryGetValue(screenId, out Func<GameObject> factory))
            {
                Debug.LogError($"UIStateController: no screen registered for id '{screenId}'.");
                return;
            }

            UIScreen previous = Current;
            if (previous != null)
            {
                previous.OnFocusLost();
                HideScreen(previous);
            }

            GameObject instance = factory();
            if (instance == null)
            {
                Debug.LogError($"UIStateController: factory for '{screenId}' returned null.");
                return;
            }

            UIScreen screen = instance.GetComponent<UIScreen>();
            if (screen == null)
            {
                Debug.LogError($"UIStateController: instantiated GameObject for '{screenId}' has no UIScreen component.");
                Destroy(instance);
                return;
            }

            _stack.Add(new StackEntry(screenId, screen));
            ShowScreen(screen);
            screen.OnPush();
            screen.OnFocus();

            OnScreenPushed?.Invoke(screenId);
        }

        /// <summary>
        /// Pops the top screen, destroys it, and restores focus to whatever is now on top.
        /// No-op if the stack is at (or below) its single base screen — Pop() never removes
        /// the last remaining screen, so there's always something to return to; use Reset()
        /// to replace the base screen itself.
        /// </summary>
        public void Pop()
        {
            if (_stack.Count <= 1) return;

            StackEntry top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);

            top.Screen.OnPop();
            Destroy(top.Screen.gameObject);

            UIScreen newTop = Current;
            if (newTop != null)
            {
                ShowScreen(newTop);
                newTop.OnFocus();
            }

            OnScreenPopped?.Invoke(top.ScreenId);
        }

        /// <summary>Tears the whole stack down (OnPop + destroy for every entry, top to bottom) and pushes homeScreenId as the new single base screen. Matches the plan's "Exit from Results returns to Home" nav-map behavior.</summary>
        public void Reset(string homeScreenId)
        {
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                UIScreen screen = _stack[i].Screen;
                screen.OnPop();
                Destroy(screen.gameObject);
            }
            _stack.Clear();

            Push(homeScreenId);
        }

        /// <summary>Transition-animation hook: called when a screen becomes visible (push, or restored after a pop above it). Default is an instant SetActive(true) — override for a fade/slide tween later; deliberately not a tweening system yet.</summary>
        protected virtual void ShowScreen(UIScreen screen)
        {
            screen.gameObject.SetActive(true);
        }

        /// <summary>Transition-animation hook: called when a screen stops being visible because another was pushed above it. Default is an instant SetActive(false); see ShowScreen.</summary>
        protected virtual void HideScreen(UIScreen screen)
        {
            screen.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hardware back on Android surfaces through the new Input System as Escape (per
        /// CLAUDE.md). Pop()s the current screen, or raises OnBackRequestedAtRoot when already
        /// at the base screen so a caller can decide whether "back at Home" should confirm-exit.
        /// </summary>
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                RequestBack();
            }
        }

        public void RequestBack()
        {
            if (_stack.Count > 1)
            {
                Pop();
            }
            else
            {
                OnBackRequestedAtRoot?.Invoke();
            }
        }
    }
}
