using UnityEngine;

public class FireSwordController : MonoBehaviour
{
    private Transform fireSword;
    private Transform ordinaryTrail;

    private void Start()
    {
        GameManager.Instance.OnFireSwordPick += EnableFireSword;

        fireSword = PlayerController.Instance.SkinSetup.FireSword;
        ordinaryTrail = PlayerController.Instance.SkinSetup.OrdinaryTrail;
    }

    private void EnableFireSword(DamageEffect _)
    {
        fireSword.gameObject.SetActive(true);
        ordinaryTrail.GetComponent<TrailRenderer>().enabled = false;
    }
}
