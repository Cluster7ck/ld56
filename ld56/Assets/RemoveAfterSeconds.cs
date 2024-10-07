using System.Collections;
using UnityEngine;

public class RemoveAfterSeconds : MonoBehaviour
{
    [SerializeField] private float seconds;
    
    void Start()
    {
        StartCoroutine(Remove(seconds));
    }

    IEnumerator Remove(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(this.gameObject);
    }
}
