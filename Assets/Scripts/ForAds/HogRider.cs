using UnityEngine;

public class HogRider : MonoBehaviour
{
    [SerializeField] private ParticleSystem swapParticleSystem;
    [SerializeField] private Transform boar;

    [SerializeField] private Vector3 ridingPlayerPositon;
    [SerializeField] private Vector3 defaultPlayerPositon;

    private bool IsBoarEnabled;

    private Transform playerTransform => PlayerController.Instance.SkinSetup.transform;
    private Animator playerAnimator => PlayerController.Instance.Animator;

    private void OnSkinChange()
    {
        MovePlayerToBoarUp(IsBoarEnabled);
    }

    private void Start()
    {
        BoarRent.Instance.OnGet += EnableBoar;
        BoarRent.Instance.OnExpire += DisableBoar;
    }

    private void EnableBoar()
    {
        SetBoarEnabled(true);
        PlayerController.Instance.OnStateChange += OnPlayerStateChange;
        PlayerController.Instance.OnSkinChange += OnSkinChange;
    }

    private void DisableBoar()
    {
        SetBoarEnabled(false);
        PlayerController.Instance.OnStateChange -= OnPlayerStateChange;
        PlayerController.Instance.OnSkinChange -= OnSkinChange;
    }

    private void SetBoarEnabled(bool state)
    {
        IsBoarEnabled = state;
        swapParticleSystem.Play();

        MovePlayerToBoarUp(state);

        boar.gameObject.SetActive(state);
    }

    private void MovePlayerToBoarUp(bool onBoar)
    {
        if (onBoar)
        {
            playerTransform.localPosition = ridingPlayerPositon;
            playerAnimator.SetLayerWeight(2, 1);
        }
        else
        {
            playerTransform.localPosition = defaultPlayerPositon;
            playerAnimator.SetLayerWeight(2, 0);
        }
    }

    private void OnPlayerStateChange(States newState)
    {
    }

    private void OnDisable()
    {
        if (BoarRent.Instance)
        {
            BoarRent.Instance.OnGet -= EnableBoar;
            BoarRent.Instance.OnExpire -= DisableBoar;

        }
        if (PlayerController.Instance)
        {
            PlayerController.Instance.OnStateChange -= OnPlayerStateChange;
            PlayerController.Instance.OnSkinChange -= OnSkinChange;
        }
    }
}
