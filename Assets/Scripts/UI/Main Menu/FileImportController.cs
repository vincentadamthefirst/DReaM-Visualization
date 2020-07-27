using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Importer;
using Importer.XMLHandlers;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using TMPro;

namespace UI.Main_Menu {
    public class FileImportController : MonoBehaviour {

        public FileEntry fileEntryPrefab;

        public TMP_InputField fileInputField;
        public Button fileOpenButton;
        public Button startImportButton;

        private RectTransform _content;

        private string _basePath = "C:/OpenPass/visualizationConfigs";

        private Coroutine _currentOpen;

        private FolderImporter _folderImporter;

        private List<Tuple<XmlType, XmlHandler>> _handlers;
        
        private Dictionary<XmlHandler, FileEntry> _fileEntries = new Dictionary<XmlHandler, FileEntry>();

        private void Start() {
            // adding necessary listeners
            fileOpenButton.onClick.AddListener(OpenFileBrowser);
            startImportButton.onClick.AddListener(StartImport);
            fileInputField.onValueChanged.AddListener(ValueChanged);
            
            // finding the content panel
            _content = transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<RectTransform>();
            
            // preparing the file import
            _folderImporter = new FolderImporter();
            FileBrowser.AddQuickLink( "Users", "C:\\Users" );
        }

        private IEnumerator ShowLoadDialogCoroutine() {
            yield return FileBrowser.WaitForLoadDialog(true, false, _basePath.Replace(" ", "") == "" ? null : _basePath,
                "Select Base Folder");

            if (!FileBrowser.Success) yield break;
            _basePath = FileBrowser.Result.Length > 0 ? FileBrowser.Result[0] : "";
            fileInputField.SetTextWithoutNotify(_basePath);
        }

        private void OpenFileBrowser() {
            _currentOpen = StartCoroutine(ShowLoadDialogCoroutine());
        }

        private void StartImport() {
            // clearing current content
            foreach (Transform obj in _content) {
                Destroy(obj.gameObject);
            }
            
            _fileEntries = new Dictionary<XmlHandler, FileEntry>();
            _handlers = _folderImporter.GetPossibleFiles(_basePath);

            var sorted = _handlers.OrderByDescending(x => x.Item1).ThenBy(x => x.Item2.GetName());
            foreach (var entry in sorted) {
                var handler = entry.Item2;
                var fileEntry = Instantiate(fileEntryPrefab, _content);
                fileEntry.name = handler.GetName();
                fileEntry.SetType(handler.GetName());
                fileEntry.SetName(handler.GetFilePath().Remove(0, _basePath.Length).Replace("\\", "/"));
                fileEntry.SetDetails(handler.GetDetails());
                _fileEntries[handler] = fileEntry;
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        }

        private void ValueChanged(string value) {
            _basePath = value;
        }
        
        public T GetXmlHandler<T>() where T : XmlHandler {
            var allOfType = _handlers.Where(x => x.Item2.GetType() == typeof(T) && _fileEntries[x.Item2].IsSelected)
                .Select(x => x.Item2).ToList();
            
            if (allOfType.Count == 0) return null;
            return (T) allOfType[0];
        }
    }
}