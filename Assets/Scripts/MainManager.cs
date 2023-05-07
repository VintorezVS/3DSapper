using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    [SerializeField] private GameObject fieldObject;
    private Field field;

    void Start()
    {
        field = fieldObject.GetComponent<Field>();
        field.Generate();
    }
}
