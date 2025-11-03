using UnityEngine;
using UnityEngine.UI;

public class CheckIfPlayerHasWaterBody : MonoBehaviour
{
    [SerializeField] private Image _skillImage;
    private bool _hasActivatedSkillImage = false;

    private void Awake()
    {
        _skillImage.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (SkillController.Instance.HasWaterBodySkill && !_hasActivatedSkillImage)
        {
            _skillImage.gameObject.SetActive(SkillController.Instance.HasWaterBodySkill);
            _hasActivatedSkillImage = true;
        }
    }
}
