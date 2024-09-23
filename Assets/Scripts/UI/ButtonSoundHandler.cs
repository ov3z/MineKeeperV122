using UnityEngine;

public class ButtonSoundHandler : MonoBehaviour
{
    public void PlayButtonSund()
    {
        SoundManager.Instance.Play(SoundTypes.Button);
    }
}
