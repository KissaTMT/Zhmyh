using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private Image _bar;
    [SerializeField] private Image _heart;
    [SerializeField] private float _barChangeSpeed;
    [SerializeField] private float _heartNoiseForce;

    private Vector3 _heartSize;
    private Zhmyh _unit;

    [Inject]
    public void Construct(PlayerUnitBrian player)
    {
        _unit = ((Zhmyh)(player.Unit));
        _heartSize = _heart.rectTransform.localScale;
    }
    private void OnEnable()
    {
        _unit.Health.Current.OnChanged += ChangeBar;
    }
    private void OnDisable()
    {
        _unit.Health.Current.OnChanged -= ChangeBar;
    }

    private void ChangeBar(float value)
    {
        StartCoroutine(GetNoiseToHeartRoutine(value));
        StartCoroutine(ChangeBarRoutine(value));
    }
    private IEnumerator GetNoiseToHeartRoutine(float value)
    {
        var delta = value / _unit.Health.Max.Value - _bar.fillAmount;
        for (var i = 0f; i < Mathf.PI; i += _heartNoiseForce * Time.deltaTime)
        {
            _heart.rectTransform.localScale = _heartSize * (1 + Mathf.Sign(delta) * Mathf.Sin(i) / 8);
            yield return null;
        }
        _heart.rectTransform.localScale = _heartSize;
    }
    private IEnumerator ChangeBarRoutine(float value)
    {
        var newFill = value / _unit.Health.Max.Value;
        var oldFill = _bar.fillAmount;
        for (var i = 0f; i < 1; i += _barChangeSpeed * Time.deltaTime)
        {
            _bar.fillAmount = Mathf.Lerp(oldFill, newFill, i);
            yield return null;
        }
        _bar.fillAmount = newFill;
    }
    
}
