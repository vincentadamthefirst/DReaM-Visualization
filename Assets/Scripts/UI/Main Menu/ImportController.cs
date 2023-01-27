using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Importer;
using Importer.XMLHandlers;
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

        private FolderImporter _folderImporter = new FolderImporter();
        private string _path;
        
        private List<Tuple<XmlType, XmlHandler>> _handlers;
        private Dictionary<XmlHandler, ImportEntry> _fileEntries = new Dictionary<XmlHandler, ImportEntry>();

        private void Start() {
            open.onClick.AddListener(OpenFileBrowser);
            reload.onClick.AddListener(Reload);
            pathInput.onValueChanged.AddListener(ValueChanged);

            if (PlayerPrefs.HasKey("configPath")) {
                _path = PlayerPrefs.GetString("configPath");
                Reload();

                if (PlayerPrefs.HasKey("selectedConfigs")) {
                    SetSelected();
                }
                
            } else {
                _path = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            }
            pathInput.SetTextWithoutNotify(_path);
        }

        private void SetSelected() {
            var selected = PlayerPrefs.GetString("selectedConfigs").Split(',');

            foreach (var path in selected) {
                try {
                    _fileEntries.First(x => x.Key.GetFilePath() == path).Value.SetSelected();
                } catch (Exception) { }
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

        private ImportType GetTypeByName(string name) {
            switch (name) {
                case "ped": return ImportType.PedestrianModels;
                case "veh": return ImportType.VehicleModels;
                case "out": return ImportType.Output;
                case "scene": return ImportType.Scenery;
                case "profiles": return ImportType.Profiles;
                case "dream": return ImportType.DReaM;
                default: return ImportType.Unsupported;
            }
        }

        private void Reload() {
            PlayerPrefs.SetString("configPath", _path);

            foreach (Transform t in content)
                Destroy(t.gameObject);
            
            _fileEntries = new Dictionary<XmlHandler, ImportEntry>();
            _handlers = _folderImporter.GetPossibleFiles(_path);
            
            var sorted = _handlers.OrderByDescending(x => x.Item1).ThenBy(x => x.Item2.GetName()).ToList();
            for (var index = 0; index < sorted.Count; index++) {
                var entry = sorted[index];
                var handler = entry.Item2;

                var type = GetTypeByName(handler.GetName());
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
                
                instantiated.name = handler.GetName();
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

        private void OnDestroy() {
            
        }
    }
}