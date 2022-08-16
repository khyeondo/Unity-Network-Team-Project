using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public TrailRenderer weaponTrail;
    public GameObject chargingEffect;
    public GameObject hitEffect;
    public GameObject chargeHitEffect;
    public GameObject upperHitEffect;
    public GameObject deathEffect;

    // Start is called before the first frame update
    void Start()
    {
        chargingEffect.SetActive(false);
        weaponTrail.startColor = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        //ChargingEffect();
    }
    
    void OnChargingEffect()
    {
        chargingEffect.SetActive(true);
    }

    void OffChargingEffect()
    {
        chargingEffect.SetActive(false);
    }

    public void HitEffect(Collider other)
    {
        GameObject _hitEffect = Instantiate(hitEffect, other.transform.position, other.transform.rotation);
        if(_hitEffect != null)
        {
            Destroy(_hitEffect, 1f);
        }
    }

    public void ChargeHitEffect(Collider other)
    {
        GameObject _chargeHitEffect = Instantiate(chargeHitEffect, other.transform.position, other.transform.rotation);
        if(_chargeHitEffect != null)
        {
            Destroy(_chargeHitEffect, 5f);
        }
    }

    public void UpperHitEffect(Collider other)
    {
        GameObject _upperHitEffect = Instantiate(upperHitEffect, other.transform.position, other.transform.rotation);
        if(_upperHitEffect != null)
        {
            Destroy(_upperHitEffect, 5f);
        }
    }

    public void DeathEffect()
    {
        GameObject _deathEffect = Instantiate(deathEffect, transform.position, transform.rotation);
        if(_deathEffect != null)
        {
            Destroy(_deathEffect, 5f);
        }
    }

    void OnHitChangeTrailColor()
    {
        weaponTrail.startColor = new Color(1f, 0.2862745f, 0f);
    }

    void OffHitChangeTrailColor()
    {
        weaponTrail.startColor = Color.white;
    }
}
