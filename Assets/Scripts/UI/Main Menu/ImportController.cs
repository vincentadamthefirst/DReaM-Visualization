using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Importer;
using Importer.XMLHandlers;
using Settings;
using SimpleFileBrowser;
using TMPro;
using UI.Main_Menu.Import;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Main_Menu {
    public class ImportController : MonoBehaviour {
        [Header("File input")]
        [Tooltip("The input field for the path.")]
        public TMP_InputField pathInput;
        public Button open;
        public Button reload;

        [Header("File Listing")] public RectTransform content;

        private readonly FolderImporter _folderImporter = new();
        private string _path;
        
        private List<Tuple<XmlType, XmlHandler>> _handlers;
        private Dictionary<XmlHandler, ImportEntry> _fileEntries = new();

        private void Start() {
            open.onClick.AddListener(OpenFileBrowser);
            reload.onClick.AddListener(Reload);
            pathInput.onValueChanged.AddListener(ValueChanged);

            var parentFolder = SettingsManager.Instance.Settings.parentFolder;
            if (!string.IsNullOrEmpty(parentFolder)) {
                _path = SettingsManager.Instance.Settings.parentFolder;
                Reload();
                
                if (SettingsManager.Instance.Settings.defaultConfiguration != null) 
                    SetSelected();
            } else {
                _path = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            }
            
            pathInput.SetTextWithoutNotify(_path);
        }

        private void SetSelected() {
            var selected = SettingsManager.Instance.Settings.defaultConfiguration;
            if (selected.filePaths == null)
                return;

            foreach (var (_, path) in selected.filePaths) {
                try {
                    _fileEntries.First(x => x.Key.GetFilePath() == path).Value.SetSelected();
                }
                catch (Exception) {
                    // ignored
                }
            }
        }
        
        private void OpenFileBrowser() {
            StartCoroutine(ShowLoadDialogCoroutine());
        }
        
        private IEnumerator ShowLoadDialogCoroutine() {
            yield return FileBrowser.WaitForLoadDialog(true, false, _path, "Select Folder");

            if (!FileBrowser.Success) yield break;
            _path = FileBrowser.Result.Length > 0 ? FileBrowser.Result[0] : "";
            pathInput.SetTextWithoutNotify(_path);
            Reload();
        }
        
        private void ValueChanged(string value) {
            _path = value;
        }

        private void Reload() {
            SettingsManager.Instance.Settings.parentFolder = _path;

            foreach (Transform t in content)
                Destroy(t.gameObject);
            
            _fileEntries = new Dictionary<XmlHandler, ImportEntry>();
            _handlers = _folderImporter.GetPossibleFiles(_path);
            
            var sorted = _handlers.OrderByDescending(x => x.Item1).ThenBy(x => x.Item2.GetXmlType()).ToList();
            for (var index = 0; index < sorted.Count; index++) {
                var entry = sorted[index];
                var handler = entry.Item2;

                var type = handler.GetXmlType();
                var entryName = handler.GetFilePath().Remove(0, _path.Length).Replace("\\", "/");
                ImportEntry instantiated;
                
                if (handler is SimulationOutputXmlHandler xmlHandler) {
                    var prefab = Resources.Load<SimulationOutputImportEntry>("Prefabs/UI/MainMenu/SimulationOutputEntry");
                    instantiated = Instantiate(prefab, content);
                    ((SimulationOutputImportEntry)instantiated)?.SetRunIds(xmlHandler.GetRuns());
                } else {
                    var prefab = Resources.Load<ImportEntry>("Prefabs/UI/MainMenu/ImportEntry");
                    instantiated = Instantiate(prefab, content);
                }

                if (instantiated == null) continue;
                instantiated.name = type.ToString();
                instantiated.SetType(type);
                instantiated.SetName(entryName);
                instantiated.SetIndex(index);
                _fileEntries[handler] = instantiated;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        public T GetXmlHandler<T>() where T : XmlHandler {
            var allOfType = _handlers.Where(x => x.Item2.GetType() == typeof(T) && _fileEntries[x.Item2].IsSelected)
                .Select(x => x.Item2).ToList();

            if (allOfType.Count == 0) return null;
            return (T)allOfType[0];
        }

        public string GetRunId() {
            var handler = GetXmlHandler<SimulationOutputXmlHandler>();
            return (_fileEntries[handler] as SimulationOutputImportEntry)?.CurrentlySelectedRunId;
        }
    }
}