using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using UnityEngine.EventSystems;

namespace DevOpsGuy.GUI
{
    public class UIManager : M_Singleton<UIManager>
    {

        [SerializeField]
        private Panel defaultPanel;
        [SerializeField]
        private Modal defaultModal;

        private static Dictionary<Type, UIBehaviour> uiCache = new Dictionary<Type, UIBehaviour>();

        private SceneLoader loader;
        private static UIBehaviour currentUI;

        public SceneLoader Load => loader;
        public UIBehaviour CurrentUI => currentUI;

        protected override void Awake()
        {
            base.Awake();

            loader = GetComponentInChildren<SceneLoader>();

            // Find all UIBase components in the scene and register them
            UIBehaviour[] components = GetComponentsInChildren<UIBehaviour>();
            foreach (var component in components)
                RegisterUIComponent(component);

            HideAll();

            if (defaultPanel != null) ShowUI(defaultPanel);

            if (defaultModal != null) ShowUI(defaultModal);
        }

        private void Update()
        {
            UpdateShortcutInput();
        }

        private void UpdateShortcutInput()
        {
            foreach (var ui in uiCache)
            {
                if (ui.Value.ShortcutInput != KeyCode.None && Input.GetKeyDown(ui.Value.ShortcutInput))
                {
                    ui.Value.OnShortcutPressed();
                }
            }
        }

        public static void HideAll()
        {
            foreach (var ui in uiCache)
            {
                ui.Value.Hide();
            }
        }

        public void RegisterUIComponent(UIBehaviour component)
        {
            Type type = component.GetType();
            if (!uiCache.ContainsKey(type))
            {
                uiCache.Add(type, component);
                component.Setup(this);
            }
        }

        public static T GetUI<T>() where T : UIBehaviour
        {
            Type type = typeof(T);
            if (uiCache.TryGetValue(type, out UIBehaviour component))
            {
                return component as T;
            }
            return null;
        }

        public void ShowUI(UIBehaviour _ui)
        {
            foreach (var ui in uiCache)
            {
                if (ui.Value.Equals(_ui))
                {
                    currentUI = ui.Value;
                    currentUI.Show();
                }
            }
        }

        public void HideUI(UIBehaviour _ui)
        {
            foreach (var ui in uiCache)
            {
                if (ui.Value.Equals(_ui))
                {
                    currentUI.Hide();
                }
            }
        }

        public void HideAllPanels()
        {
            foreach (var ui in uiCache)
            {
                if (ui.Value is Panel)
                {
                    ui.Value.gameObject.SetActive(false);
                }
            }
        }

        public void HideAllPopups()
        {
            foreach (var ui in uiCache)
            {
                if (ui.Value is Modal)
                {
                    ui.Value.gameObject.SetActive(false);
                }
            }
        }

        public static T ShowUI<T>() where T : UIBehaviour
        {
            T ui = GetUI<T>();
            if (ui != null) {
                currentUI = ui;
                Cursor.lockState = currentUI.CursorMode;
                ui.Show();
            }
            else
            {
                Debug.Log($"<color=red>UI of type {typeof(T)} is null!</color>");
            }
            return ui;
        }

        public static void HideUI<T>() where T : UIBehaviour
        {
            T ui = GetUI<T>();
            if (ui != null) {
                ui.Hide();
            }
        }

        #region Miscs
        public bool IsPointerOverUIObject()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        #endregion
    }
}