using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatTextUI : MonoBehaviour
{
    private const int MAX_POOL_COUNT = 50;

    [SerializeField] TextMeshProUGUI _combatText;

    private int _currentIndex;
    private List<TextMeshProUGUI> _combatTextList = new List<TextMeshProUGUI>();

    public void Init()
    {
        for(int i = 0; i < MAX_POOL_COUNT; ++i)
        {
            var obj = Instantiate(_combatText).GetComponent<TextMeshProUGUI>();
            obj.transform.SetParent(this.transform, false);
            obj.gameObject.SetActive(false);

            _combatTextList.Add(obj);
        }

        Destroy(_combatText.gameObject);
    }

    public void ShowCombatText(int damage)
    {
        _currentIndex++;

        if (_currentIndex >= _combatTextList.Count) _currentIndex = 0;

        var text = _combatTextList[_currentIndex];
        text.transform.localPosition = Vector3.zero;
        text.text = damage.ToString();

        text.gameObject.SetActive(true);
        text.transform.DOLocalMoveY(100, 0.5f).OnComplete(() =>
        {
            text.gameObject.SetActive(false);
        });
    }
    public void ShowCombatText(Transform spawnTransform, int damage)
    {
        _currentIndex++;

        if (_currentIndex >= _combatTextList.Count) _currentIndex = 0;

        var text = _combatTextList[_currentIndex];
        text.transform.localPosition = Vector3.zero;
        text.text = damage.ToString();

        text.gameObject.SetActive(true);
        text.transform.DOLocalMoveY(100, 0.5f).OnComplete(() =>
        {
            text.gameObject.SetActive(false);
        });
    }

    public void ShowCombatText(string _text)
    {
        _currentIndex++;

        if (_currentIndex >= _combatTextList.Count) _currentIndex = 0;

        var text = _combatTextList[_currentIndex];
        text.transform.localPosition = Vector3.zero;
        text.text = _text;

        text.gameObject.SetActive(true);
        text.transform.DOLocalMoveY(100, 0.5f).OnComplete(() =>
        {
            text.gameObject.SetActive(false);
        });
    }
}
