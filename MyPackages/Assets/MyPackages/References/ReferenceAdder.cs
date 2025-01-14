using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceAdder : MonoBehaviour
{

    [SerializeField] ReferenceScriptableObject reference;

    // Start is called before the first frame update
    void Start()
    {
        ReferenceManager.ReferenceManagerInstance.SetReference(reference, gameObject);
    }

}
