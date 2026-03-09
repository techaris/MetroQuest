using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurvedTextUI : MonoBehaviour
{
    [SerializeField] private float radius = 300f;
    [SerializeField] private float arcDegrees = 120f;

    private TextMeshProUGUI tmp;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.OnPreRenderText += OnPreRenderText;
    }

    private void OnDestroy()
    {
        tmp.OnPreRenderText -= OnPreRenderText;
    }

    private void OnPreRenderText(TMP_TextInfo textInfo)
    {
        int charCount = textInfo.characterCount;
        if (charCount == 0) return;

        float totalArc = Mathf.Deg2Rad * arcDegrees;
        float anglePerChar = totalArc / Mathf.Max(1, charCount - 1);

        for (int i = 0; i < charCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3 charMidBaseline = (vertices[vertexIndex] +
                                       vertices[vertexIndex + 2]) / 2;

            float angle = -totalArc / 2 + anglePerChar * i;
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;

            Vector3 offset = new Vector3(x, y - radius, 0);

            for (int j = 0; j < 4; j++)
                vertices[vertexIndex + j] += offset;

            float charAngle = -angle * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, charAngle);

            for (int j = 0; j < 4; j++)
            {
                Vector3 diff = vertices[vertexIndex + j] - offset;
                vertices[vertexIndex + j] = offset + rotation * diff;
            }
        }
    }
}
