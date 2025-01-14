using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionProxy : MonoBehaviour
{
    public Action<Collision> OnCollisionEnter3D_Action;
    public Action<Collision> OnCollisionStay3D_Action;
    public Action<Collision> OnCollisionExit3D_Action;

    public Action<Collider> OnTriggerEnter3D_Action;
    public Action<Collider> OnTriggerStay3D_Action;
    public Action<Collider> OnTriggerExit3D_Action;

    public Action<Collision2D> OnCollisionEnter2D_Action;
    public Action<Collision2D> OnCollisionStay2D_Action;
    public Action<Collision2D> OnCollisionExit2D_Action;

    public Action<Collider2D> OnTriggerEnter2D_Action;
    public Action<Collider2D> OnTriggerStay2D_Action;
    public Action<Collider2D> OnTriggerExit2D_Action;


    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnter3D_Action?.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        OnCollisionStay3D_Action?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExit3D_Action?.Invoke(collision);
    }

    private void OnTriggerEnter(Collider collider)
    {
        OnTriggerEnter3D_Action?.Invoke(collider);
    }

    private void OnTriggerStay(Collider collider)
    {
        OnTriggerStay3D_Action?.Invoke(collider);
    }

    private void OnTriggerExit(Collider collider)
    {
        OnTriggerExit3D_Action?.Invoke(collider);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionEnter2D_Action?.Invoke(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionStay2D_Action?.Invoke(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        OnCollisionExit2D_Action?.Invoke(collision);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        OnTriggerEnter2D_Action?.Invoke(collider);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        OnTriggerStay2D_Action?.Invoke(collider);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        OnTriggerExit2D_Action?.Invoke(collider);
    }
}
