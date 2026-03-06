using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Shared
{
    public class RewardPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private Graphic[] tintTargets;
        [SerializeField] private float hideAfterSeconds = 1f;

        private Coroutine _hideCoroutine;

        public void Initialize(string message, Color color)
        {
            if (valueText != null)
            {
                valueText.text = message;
                valueText.color = color;
            }

            if (tintTargets != null)
            {
                foreach (var graphic in tintTargets)
                {
                    if (graphic != null)
                        graphic.color = color;
                }
            }

            if (_hideCoroutine != null)
                StopCoroutine(_hideCoroutine);

            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(hideAfterSeconds);
            gameObject.SetActive(false);
            _hideCoroutine = null;
        }
    }
}
