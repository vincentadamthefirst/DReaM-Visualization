using UnityEngine;
using Utils;

namespace UI.Main_Menu.Settings {

    public abstract class Setting : MonoBehaviour {
        public abstract void StoreData();

        public abstract void LoadData();
    }
    
    public abstract class Setting<T> : Setting {
        public Reference<T> Reference { get; set; }

        /// <summary>
        /// Setter for information on this setting. Accepts any number of parameters that are treated differently
        /// depending on the inheritor.
        /// </summary>
        /// <param name="infos">The infos to pass</param>
        public abstract void SetInfo(params string[] infos);
    }
}