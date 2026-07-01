using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerSkillNameBaloon : MonoBehaviour
{
    public static PlayerSkillNameBaloon Instance { get; private set; }

    [SerializeField] private TMP_Text _skillNameText;
    [SerializeField] private float _displayTime = 2f;

    private int _showId;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (_skillNameText == null)
            _skillNameText = GetComponentInChildren<TMP_Text>(true);

        gameObject.SetActive(false);
    }

    public void Show(Skill skill)
    {
        if (_skillNameText == null || skill == null)
            return;

        _skillNameText.text = FormatSkillName(skill);
        gameObject.SetActive(true);

        int showId = ++_showId;
        HideAfterDelay(showId).Forget();
    }

    private async UniTask HideAfterDelay(int showId)
    {
        await UniTask.Delay((int)(_displayTime * 1000f), cancellationToken: this.GetCancellationTokenOnDestroy());

        if (showId != _showId)
            return;

        gameObject.SetActive(false);
    }

    private string FormatSkillName(Skill skill)
    {
        string skillName = !string.IsNullOrWhiteSpace(skill.Name) ? skill.Name : skill.name;
        return skillName.EndsWith("!") ? skillName : $"{skillName}!";
    }
}
