using UnityEngine;

public class NPCAnimationHandler : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] bool _isTalk;
    [SerializeField] bool _isGreeting;
    [SerializeField] bool _isApplause;
    [SerializeField] bool _isSwim;
    [SerializeField] bool _isIdle;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    private void Start()
    {

        SetStartAnim();
    }

    public void SetIdleAnim()
    {
        _animator.SetBool("isTalk", false);
        _animator.SetBool("isGreeting", false);
        _animator.SetBool("isApplause", false);
    }
    public void SetStartAnim()
    {
        _animator.SetBool("isTalk", _isTalk);
        _animator.SetBool("isGreeting", _isGreeting);
        _animator.SetBool("isApplause", _isApplause);
        SetBoolIfExists("isSwim", _isSwim);
    }

    // isSwim объявлен не во всех контроллерах NPC, поэтому проверяем наличие параметра.
    private void SetBoolIfExists(string parameterName, bool value)
    {
        foreach (AnimatorControllerParameter parameter in _animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == parameterName)
            {
                _animator.SetBool(parameterName, value);
                return;
            }
        }
    }
    public void SetisApplauseAnim()
    {
        _animator.SetBool("isApplause", true);
        _animator.SetBool("isTalk", false);
        _animator.SetBool("isGreeting", false);
    }

    public void SetGreetingAnim()
    {
        _animator.SetBool("isGreeting", true);
        _animator.SetBool("isTalk", false);
        _animator.SetBool("isApplause", false);
    }

    public void SetTalkAnim()
    {
        _animator.SetBool("isTalk", true);
        _animator.SetBool("isGreeting", false);
        _animator.SetBool("isApplause", false);
    }
}
