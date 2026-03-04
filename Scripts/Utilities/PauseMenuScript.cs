using UnityEngine;

namespace FabioSenoDev
{ 
public class PauseMenuScript : MonoBehaviour
{
        [SerializeField]  private GameObject pauseMenuCanvas;

        public static bool gameIsPaused {  get; private set; }

        private void Start()
        {
            gameIsPaused = false;

            if (pauseMenuCanvas != null )
            {
                pauseMenuCanvas.SetActive( false );
            }
        }

        public void ResumeGame()
        {
            if (pauseMenuCanvas != null)
            {
                pauseMenuCanvas.SetActive( false );
            }

            Time.timeScale = 1f;
            gameIsPaused = false;
        }

        public void PauseGame()
        {
            if (pauseMenuCanvas != null)
            {
                pauseMenuCanvas.SetActive( true );
            }
            Time.timeScale = 0f;
            gameIsPaused= true;
        }

        private void OnDestroy()
        {
            if (gameIsPaused)
            {
                Time.timeScale = 1f;
                gameIsPaused = false;
            }
        }
    }
}
