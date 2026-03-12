using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _playerAnimator;
    private string _currentAnimation;

    private void Awake()
    {
        if (_playerAnimator == null)
        {
            Debug.LogError("No animator found for Player!");
            return;
        }    
    }

    private void ChangeAnimation(string animation, float crossfade = 0.2f)
    {
        if(_playerAnimator != null)
        {
            if (_currentAnimation != animation)
            {
                _currentAnimation = animation;
                _playerAnimator.CrossFade(animation, crossfade);
            }
        }
    }


}
