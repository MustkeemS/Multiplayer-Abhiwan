using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject PlayerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
        if (PhotonNetwork.IsConnectedAndReady)
        {
            int rng = Random.Range(-20, 10);
            PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(rng, 0f, rng), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
