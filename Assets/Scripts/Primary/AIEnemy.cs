using MLAPI;
using MLAPI.Messaging;

public class AIEnemy : NetworkedBehaviour
{
    private int enemyHealth;




    public void Damage(int amount) 
    {
        InvokeServerRpc(DamagePlayer, amount);
    }




    private void Start()
    {
        if (NetworkingManager.Singleton.IsServer)
        {
            enemyHealth = 100; 
        }
        else 
        {
            GetComponent<BreadcrumbAi.Ai>().enabled = false;
        }
    }




    [ServerRPC(RequireOwnership = false)]
    private void DamagePlayer(int amount) 
    {
        if(enemyHealth - amount <= 0) 
        {
            //PlayerDies
        }
        else 
        {
            enemyHealth -= amount;
        }
    }
}
