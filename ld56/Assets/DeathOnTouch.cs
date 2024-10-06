using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathOnTouch : MonoBehaviour
{
    public void Die(Cricket cricket)
    {
        cricket.gameManager.ChangeState(GameState.Died);
    }
}
