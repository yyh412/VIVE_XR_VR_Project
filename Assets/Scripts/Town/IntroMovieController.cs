using UnityEngine;

public class IntroMovieController : MonoBehaviour
{
    [Header("三个动画")]
    public Animator cameraAnimator;
    public Animator upperEyelidAnimator;
    public Animator lowerEyelidAnimator;

    void Start()
    {
        PlayIntro();
    }

    public void PlayIntro()
    {
        if (cameraAnimator != null)
        {
            cameraAnimator.Play("IntroCamera_14s", 0, 0f);
        }

        if (upperEyelidAnimator != null)
        {
            upperEyelidAnimator.Play("WakeUpUpper", 0, 0f);
        }

        if (lowerEyelidAnimator != null)
        {
            lowerEyelidAnimator.Play("WakeUpLower", 0, 0f);
        }
    }
}