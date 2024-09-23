using UnityEngine;

public class AddWorkerOnPayout : PayoutValidation
{
    [SerializeField] private Storage storage;
  
    public override void OnPayout()
    {
        storage.UpgaradeNPCCount();
    }
}
