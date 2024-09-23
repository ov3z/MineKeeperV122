using UnityEngine;

public class StepSoundHandler : MonoBehaviour
{
    private VolumeLevels volumeLevel = VolumeLevels.Silent;

    public void PlayLeftFootSound()
    {
        if (GameManager.Instance.IsInCave)
        {
            SoundManager.Instance.Play(SoundTypes.FootstepTunnel_1, volumeLevel);
        }
        else
        {
            SoundManager.Instance.Play(SoundTypes.FootstepVillage_1, volumeLevel);
        }
    }

    public void PlayRightFootSound()
    {
        if (GameManager.Instance.IsInCave)
        {
            SoundManager.Instance.Play(SoundTypes.FootstepTunnel_2, volumeLevel);
        }
        else
        {
            SoundManager.Instance.Play(SoundTypes.FootstepVillage_2, volumeLevel);
        }
    }
}
