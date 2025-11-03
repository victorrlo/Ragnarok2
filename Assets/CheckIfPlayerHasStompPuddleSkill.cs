using UnityEngine;
using UnityEngine.UI;

public class CheckIfPlayerHasStompPuddleSkill : MonoBehaviour
{
    [SerializeField] private Image _skillImage;
    private bool _hasActivatedSkillImage = false;

    private void Awake()
    {
        _skillImage.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (SkillController.Instance.HasStompPuddleSkill && !_hasActivatedSkillImage)
        {
            _skillImage.gameObject.SetActive(SkillController.Instance.HasStompPuddleSkill);
            _hasActivatedSkillImage = true;
        }
    }
}
