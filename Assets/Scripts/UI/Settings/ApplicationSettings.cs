using UnityEngine;

namespace UI.Settings {
    public class ApplicationSettings {

        public int resolutionWidth;
        public int resolutionHeight;
        public int resolutionFps;
        public bool fullscreen = true;

        public bool checkOcclusion = true;
        public float minimumAgentOpacity = .7f;
        public float minimumObjectOpacity = .3f;

        public void LoadFromPrefs() {
            if (PlayerPrefs.HasKey("res_width")) resolutionWidth = PlayerPrefs.GetInt("res_width");
            if (PlayerPrefs.HasKey("res_height")) resolutionHeight = PlayerPrefs.GetInt("res_height");
            if (PlayerPrefs.HasKey("res_fps")) resolutionFps = PlayerPrefs.GetInt("res_fps");
            if (PlayerPrefs.HasKey("res_fs")) fullscreen = PlayerPrefs.GetInt("res_fs") > 0;

            if (PlayerPrefs.HasKey("occ_toogle")) checkOcclusion = PlayerPrefs.GetInt("occ_toggle") > 0;
            if (PlayerPrefs.HasKey("occ_minagop")) minimumAgentOpacity = PlayerPrefs.GetFloat("occ_minagop");
            if (PlayerPrefs.HasKey("occ_minobop")) minimumObjectOpacity = PlayerPrefs.GetFloat("occ_minobop");
        }

        public void StoreToPrefs() {
            PlayerPrefs.SetInt("res_width", resolutionWidth);
            PlayerPrefs.SetInt("res_height", resolutionHeight);
            PlayerPrefs.SetInt("res_fps", resolutionFps);
            PlayerPrefs.SetInt("res_fs", fullscreen ? 1 : 0);
            
            PlayerPrefs.SetInt("occ_toggle", checkOcclusion ? 1 : 0);
            PlayerPrefs.SetFloat("occ_minagop", minimumAgentOpacity);
            PlayerPrefs.SetFloat("occ_minobop", minimumObjectOpacity);
            
            PlayerPrefs.Save();
        }
    }
}