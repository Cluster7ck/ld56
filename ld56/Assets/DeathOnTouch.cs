using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathOnTouch : MonoBehaviour
{
    public void Die(Cricket cricket)
    {
        Instantiate(cricket.DeathAnimationPrefab, cricket.transform.position, Quaternion.identity);
        cricket.gameManager.ChangeState(GameState.Died);
        
    }
}
