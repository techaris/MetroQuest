using System.Collections;
using UnityEngine;
using TMPro;
using Core.Managers;
using Core.Enums;

namespace UI.Shared
{
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private CurrencyType currencyType;
        [SerializeField] private Animator effectAnim;
        [SerializeField] private string prefix = "";
        [SerializeField] private float waitTimeout = 2f;

        private int _lastDisplayedValue = -1;
        private Coroutine _waitCoroutine;

        private void OnEnable()
        {
            _waitCoroutine = StartCoroutine(WaitForEconomyManagerAndSubscribe());
        }

        private void OnDisable()
        {
            if (_waitCoroutine != null)
            {
                StopCoroutine(_waitCoroutine);
                _waitCoroutine = null;
            }

            if (EconomyManager.Instance != null)
                EconomyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        }

        private IEnumerator WaitForEconomyManagerAndSubscribe()
        {
            float elapsed = 0f;
            while (EconomyManager.Instance == null && elapsed < waitTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            _waitCoroutine = null;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
                RefreshDisplay();
            }
        }

        private void OnCurrencyChanged(int soft, int hard)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (valueText == null)
            {
                Debug.LogError("[CurrencyDisplay] valueText is not assigned.");
                return;
            }

            if (EconomyManager.Instance == null)
                return;

            int value = currencyType == CurrencyType.Soft
                ? EconomyManager.Instance.SoftCurrency
                : EconomyManager.Instance.HardCurrency;

            bool hasChanged = value != _lastDisplayedValue;
            bool isFirstLoad = _lastDisplayedValue < 0;
            _lastDisplayedValue = value;

            valueText.text = string.IsNullOrEmpty(prefix)
                ? value.ToString()
                : prefix + value.ToString();

            if (effectAnim != null && hasChanged && !isFirstLoad)
                effectAnim.SetTrigger("Jiggle");
        }
    }
}
