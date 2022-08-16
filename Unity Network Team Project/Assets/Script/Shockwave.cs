using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    private ScaleShockwave item;
    public float shockPower = 1.0f;

    public int pickedUpActorNumber;

    // Start is called before the first frame update
    void Start()
    {
        item = gameObject.GetComponentInParent<ScaleShockwave>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            StartCoroutine(Hit(other));
        }
    }

    private void OnHit(Collider other)
    {        
        if (other.GetComponent<CharacterController>().actorNumber != pickedUpActorNumber)
        {
            CharacterController controller = other.gameObject.GetComponent<CharacterController>();
            ParticleController particle = other.gameObject.GetComponent<ParticleController>();
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            Transform tr = other.gameObject.GetComponent<Transform>();

            rb.velocity = Vector3.zero;
            Vector3 dir = tr.position - transform.position;

            particle.ChargeHitEffect(other);
            controller.IsMineAddForce((dir.normalized * 3f + Vector3.up * 2f) * shockPower, ForceMode.Impulse);
            controller.SyncAnimTrigger("hardAttacked");
        }
    }

    IEnumerator Hit(Collider other)
    {
        OnHit(other);
        yield return new WaitForSeconds(0.01f);
    }
}