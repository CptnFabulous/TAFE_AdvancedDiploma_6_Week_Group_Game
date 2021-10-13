using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    [SerializeField] private int itemType;
    [SerializeField] private GameObject visualsRef;
    [SerializeField] private Collider collRef;

    private void Pickup()
    {
        AbilityManager.SoleManager.AddItem(itemType);
        collRef.enabled = false;
        visualsRef.SetActive(false);
        
        //temp
        StartCoroutine(nameof(NextRoom));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }

    //temp
    private IEnumerator NextRoom()
    {
        yield return new WaitForSeconds(3);
        GameManager.Instance.EnterNewRoom();
    }
}
