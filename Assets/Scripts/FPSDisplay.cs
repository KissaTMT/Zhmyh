using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsText;
    [SerializeField] private float _updateInterval = 0.5f;

    private float _accum = 0f;
    private int _frames = 0;
    private float _timeLeft;

    private void Start()
    {
        if (_fpsText == null)
        {
            Debug.LogError("FPSDisplay: No Text component assigned!");
            enabled = false;
            return;
        }

        _timeLeft = _updateInterval;
    }

    private void Update()
    {
        _timeLeft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        _frames++;

        if (_timeLeft <= 0f)
        {
            float fps = _accum / _frames;
            _fpsText.text = $"FPS: {fps:F1}";

            _timeLeft = _updateInterval;
            _accum = 0f;
            _frames = 0;
        }
    }
}