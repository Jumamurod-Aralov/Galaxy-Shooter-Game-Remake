using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 2.8f);   
    }
}