using System.Collections;
using Animas.Core;
using Animas.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Animas.UI
{
    // The defeat screen (GDD §17, required by the rubric): when the Padre's life reaches 0, after a short
    // moment it fades in with Reintentar (back to the last safe room) and Salir al menú.
    public class DeathScreen : MonoBehaviour
    {
        [SerializeField] InstanceCatalog catalog;
        [SerializeField] Health playerHealth;
        [SerializeField] PlayerRespawn respawn;
        [SerializeField] CanvasGroup panel;
        [SerializeField] Button retryButton;
        [SerializeField] Button quitButton;
        [Tooltip("Seconds between dying and the screen appearing (the fall and the red flash play first).")]
        [SerializeField, Min(0f)] float delay = 1.5f;
        [SerializeField, Min(0.05f)] float fadeSeconds = 0.8f;

        void Awake()
        {
            retryButton.onClick.AddListener(Retry);
            quitButton.onClick.AddListener(() => SceneFlow.LoadMenu(catalog));
            Hide();
        }

        void OnEnable() => playerHealth.Died += OnDied;
        void OnDisable() => playerHealth.Died -= OnDied;

        void OnDied() => StartCoroutine(ShowLater());

        IEnumerator ShowLater()
        {
            yield return new WaitForSeconds(delay);
            panel.gameObject.SetActive(true);
            panel.interactable = true;
            panel.blocksRaycasts = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / fadeSeconds)
            {
                panel.alpha = t;
                yield return null;
            }
            panel.alpha = 1f;
        }

        void Retry()
        {
            Hide();
            // No safe room reached yet: start the instance over.
            if (respawn == null || !respawn.Respawn())
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void Hide()
        {
            StopAllCoroutines();
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
            panel.gameObject.SetActive(false);
        }
    }
}
