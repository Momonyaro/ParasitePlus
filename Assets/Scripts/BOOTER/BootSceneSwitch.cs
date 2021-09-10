using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace BOOTER
{
    public class BootSceneSwitch : MonoBehaviour
    {
        public VideoPlayer vPlayer;
        public bool init = false;
        public int sceneIndex = 2;
        private bool videoInitialized = false;

        private void Start()
        {
            init = true;
        }

        private void Update()
        {
            if (!init) return;
            if (!videoInitialized) { videoInitialized = vPlayer.isPlaying; return; }

            if (!vPlayer.isPlaying)
            {
                init = false;
                SceneManager.LoadScene(sceneIndex);
            }
        }
    }
}
