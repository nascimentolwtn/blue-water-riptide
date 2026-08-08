using BlueWaterRiptide.Core;
using BlueWaterRiptide.Networking;
using BlueWaterRiptide.Networking.Session;
using BlueWaterRiptide.UI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BlueWaterRiptide.UI
{
    /// <summary>
    /// Minimal Host/Join lobby for Plan 10's M1 wire-up spike — Host button, Join button + IP:port
    /// field, a Ready toggle, a player-count/ready readout, and a host-only Start Match button.
    /// Entirely procedural (this project has zero hand-authored UI prefabs — see CLAUDE.md), built
    /// in OnPush() the same way M1PrototypeBootstrap builds the 3D scene in code. Deliberately not a
    /// full lobby (no team assignment/Sailor picks/discovery list — those are Plan 10 M2/M3): this is
    /// the minimum needed to get a host and one guest into LanMatchBootstrap.HostStartMatch.
    /// </summary>
    public sealed class LanLobbyScreen : UIScreen
    {
        const int PlayersMax = 2;
        static readonly Font s_Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        LanSessionLauncher _launcher;
        bool _isHost;
        bool _hostReady;
        bool _matchStarted;
        ulong _hostClientId;
        ulong _remoteClientId;
        bool _remoteConnected;

        Text _statusText;
        Text _rosterText;
        InputField _ipField;
        Button _hostButton;
        Button _joinButton;
        Button _startButton;

        public override void OnPush()
        {
            _launcher = new LanSessionLauncher();
            BuildUi();
            RefreshStatus();
        }

        public override void OnPop()
        {
            _launcher?.Leave();
            UnsubscribeConnectionCallbacks();
        }

        void Update()
        {
            _launcher?.Poll(Time.deltaTime);
        }

        // --- Button/toggle handlers ---------------------------------------------------------

        void OnHostClicked()
        {
            if (_isHost || _matchStarted) return;

            LanMatchBootstrap.EnsureNetworkPrefabsRegistered();
            _launcher.Create(new SessionCreateOptions("Host", SessionMode.Pvp, PlayersMax));

            _isHost = true;
            _hostClientId = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : 0UL;

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }

            _hostButton.interactable = false;
            _joinButton.interactable = false;
            _startButton.gameObject.SetActive(true);

            RefreshStatus();
        }

        void OnJoinClicked()
        {
            if (_isHost || _matchStarted) return;

            LanMatchBootstrap.EnsureNetworkPrefabsRegistered();
            string endpoint = string.IsNullOrWhiteSpace(_ipField.text) ? "127.0.0.1:7777" : _ipField.text.Trim();

            _hostButton.interactable = false;
            _joinButton.interactable = false;
            _statusText.text = $"Connecting to {endpoint}...";

            _launcher.JoinCompleted += OnJoinCompleted;
            _launcher.Join(endpoint, endpoint);
        }

        void OnJoinCompleted(bool success)
        {
            _launcher.JoinCompleted -= OnJoinCompleted;

            if (success)
            {
                _statusText.text = "Connected — waiting for the host to start the match.";
                LanMatchBootstrap.ClientPreparePresentation();
            }
            else
            {
                _statusText.text = "Join failed — check the IP:port and try again.";
                _hostButton.interactable = true;
                _joinButton.interactable = true;
            }
        }

        void OnReadyToggled(bool value)
        {
            _hostReady = value;
            RefreshStatus();
        }

        void OnClientConnected(ulong clientId)
        {
            if (!_isHost || clientId == _hostClientId) return;

            _remoteClientId = clientId;
            _remoteConnected = true;
            RefreshStatus();
        }

        void OnClientDisconnected(ulong clientId)
        {
            if (!_isHost || clientId != _remoteClientId) return;

            _remoteConnected = false;
            RefreshStatus();
        }

        void OnStartClicked()
        {
            if (!_isHost || !_remoteConnected || !_hostReady || _matchStarted) return;

            _matchStarted = true;
            LanMatchBootstrap.HostStartMatch(_launcher, _hostClientId, _remoteClientId);

            _startButton.interactable = false;
            _statusText.text = "Match started.";
        }

        void RefreshStatus()
        {
            if (!_isHost)
            {
                _rosterText.text = "";
                return;
            }

            _statusText.text = _matchStarted ? "Match started." : "Hosting — advertising on the LAN.";
            _rosterText.text = _remoteConnected
                ? $"Players: Host (You){(_hostReady ? " [ready]" : "")}, Guest [connected]"
                : "Players: Host (You) — waiting for a guest to join...";

            _startButton.interactable = _remoteConnected && _hostReady && !_matchStarted;
        }

        void UnsubscribeConnectionCallbacks()
        {
            if (NetworkManager.Singleton == null) return;

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        // --- Procedural UI construction -----------------------------------------------------

        void BuildUi()
        {
            EnsureEventSystem();

            gameObject.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            // Top-right corner overlay so it doesn't cover M1PrototypeBootstrap's top-left IMGUI
            // HUD box — both stay reachable at once (see LanModeEntryPoint's doc comment).
            RectTransform panel = CreatePanel(transform, "Panel", new Vector2(0.62f, 0.5f), new Vector2(0.99f, 0.98f));

            CreateText(panel, "Title", "LAN PLAY", 22, TextAnchor.UpperLeft, new Vector2(10, -10), new Vector2(300, 30));

            _hostButton = CreateButton(panel, "HostButton", "Host", new Vector2(10, -50), new Vector2(110, 34), OnHostClicked);
            _joinButton = CreateButton(panel, "JoinButton", "Join", new Vector2(130, -50), new Vector2(70, 34), OnJoinClicked);
            _ipField = CreateInputField(panel, "IpField", new Vector2(210, -50), new Vector2(180, 34), "127.0.0.1:7777");

            CreateToggle(panel, "ReadyToggle", "Ready", new Vector2(10, -100), OnReadyToggled);

            _startButton = CreateButton(panel, "StartButton", "Start Match", new Vector2(10, -140), new Vector2(160, 40), OnStartClicked);
            _startButton.gameObject.SetActive(false);

            _statusText = CreateText(panel, "StatusText", "Not connected.", 15, TextAnchor.UpperLeft, new Vector2(10, -195), new Vector2(380, 50));
            _rosterText = CreateText(panel, "RosterText", "", 15, TextAnchor.UpperLeft, new Vector2(10, -250), new Vector2(380, 60));
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            // InputSystemUIInputModule, not the legacy StandaloneInputModule — this project runs the
            // new Input System as its sole active handler (CLAUDE.md), which the legacy UI module
            // can't read from when Active Input Handling is set to "Input System Package (New)".
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            return rt;
        }

        static void AnchorTopLeft(RectTransform rt, Vector2 topLeftOffset, Vector2 size)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = topLeftOffset;
            rt.sizeDelta = size;
        }

        static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Vector2 topLeftOffset, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            AnchorTopLeft((RectTransform)go.transform, topLeftOffset, size);

            var text = go.GetComponent<Text>();
            text.font = s_Font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 topLeftOffset, Vector2 size, UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            AnchorTopLeft((RectTransform)go.transform, topLeftOffset, size);

            go.GetComponent<Image>().color = new Color(0.2f, 0.35f, 0.5f, 0.9f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var labelText = labelGo.GetComponent<Text>();
            labelText.font = s_Font;
            labelText.fontSize = 15;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.text = label;

            return button;
        }

        static InputField CreateInputField(Transform parent, string name, Vector2 topLeftOffset, Vector2 size, string defaultValue)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            AnchorTopLeft((RectTransform)go.transform, topLeftOffset, size);

            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8, 4);
            textRt.offsetMax = new Vector2(-8, -4);

            var text = textGo.GetComponent<Text>();
            text.font = s_Font;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;

            var inputField = go.GetComponent<InputField>();
            inputField.textComponent = text;
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.text = defaultValue; // sensible default for a same-machine/LAN smoke test, editable

            return inputField;
        }

        static Toggle CreateToggle(Transform parent, string name, string label, Vector2 topLeftOffset, UnityAction<bool> onValueChanged)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent, false);
            AnchorTopLeft((RectTransform)go.transform, topLeftOffset, new Vector2(200, 28));

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(go.transform, false);
            var bgRt = (RectTransform)bgGo.transform;
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(0f, 0.5f);
            bgRt.pivot = new Vector2(0f, 0.5f);
            bgRt.anchoredPosition = Vector2.zero;
            bgRt.sizeDelta = new Vector2(24, 24);
            var bgImage = bgGo.GetComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.2f);

            var checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkGo.transform.SetParent(bgGo.transform, false);
            var checkRt = (RectTransform)checkGo.transform;
            checkRt.anchorMin = new Vector2(0.15f, 0.15f);
            checkRt.anchorMax = new Vector2(0.85f, 0.85f);
            checkRt.offsetMin = Vector2.zero;
            checkRt.offsetMax = Vector2.zero;
            var checkImage = checkGo.GetComponent<Image>();
            checkImage.color = new Color(0.3f, 0.9f, 0.4f, 1f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = new Vector2(0f, 0.5f);
            labelRt.anchorMax = new Vector2(0f, 0.5f);
            labelRt.pivot = new Vector2(0f, 0.5f);
            labelRt.anchoredPosition = new Vector2(32, 0);
            labelRt.sizeDelta = new Vector2(160, 24);
            var labelText = labelGo.GetComponent<Text>();
            labelText.font = s_Font;
            labelText.fontSize = 14;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.text = label;

            var toggle = go.GetComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(onValueChanged);

            return toggle;
        }
    }
}
