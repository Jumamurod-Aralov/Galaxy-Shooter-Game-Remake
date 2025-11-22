using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 _originalPosition;

    public IEnumerator Shake(float duration, float magnitude)
    {
        _originalPosition = transform.position;

        float elapsed = 0f;

        float seedX = Random.Range(0f, 100f);
        float seedY = Random.Range(0f, 100f);

        while (elapsed < duration)
        {
            float x = (Mathf.PerlinNoise(seedX, elapsed * 10f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(seedY, elapsed * 10f) - 0.5f) * 2f;

            transform.localPosition = _originalPosition + new Vector3(x, y, 0) * magnitude;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPosition;
    }
}