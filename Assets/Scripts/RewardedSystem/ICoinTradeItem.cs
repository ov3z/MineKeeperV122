using UnityEngine.UI;

public interface ICoinTradeItem
{
    public float Price => float.MaxValue;
    public Button BuyButton => null;
    public bool IsUnlocked => true;
    public void Aquire();
}
