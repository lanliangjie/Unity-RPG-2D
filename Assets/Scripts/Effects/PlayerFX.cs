using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerFX : EntityFX
{
    [Header("Screen shake FX")]
    private CinemachineImpulseSource screenShake;
    [SerializeField] private float shakeMultiplier;
    public Vector3 shakeSwordImpact;
    public Vector3 shakeHighDamage;

    [Header("After image FX")]
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private float colorLooseRate;
    [SerializeField] private float afterImageCooldown;
    private float afterImageCooldownTimer;
    [Space]
    [SerializeField] private ParticleSystem dustFX;

    protected override void Start()
    {
        base.Start();
        screenShake = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        afterImageCooldownTimer -= Time.deltaTime;
    }

    public void ScreenShake(Vector3 _shakePower)
    {
        screenShake.m_DefaultVelocity = new Vector3(_shakePower.x * player.facingDir, _shakePower.y) * shakeMultiplier;
        screenShake.GenerateImpulse();
    }

    public void CreateAfterImage()
    {
        if (afterImageCooldownTimer < 0)
        {
            afterImageCooldownTimer = afterImageCooldown;
            GameObject newAfterImage = Instantiate(afterImagePrefab, transform.position, transform.rotation);
            newAfterImage.GetComponent<AfterImageFX>().SetupAfterImage(colorLooseRate, sr.sprite);
        }
    }

    public void PlayDustFX()
    {
        if (dustFX != null)
            dustFX.Play();
    }

    public new void MakeTransprent(bool _transprent)
    {
        if (!gameObject.activeInHierarchy) return;

        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr == null)
            {
                return;
            }
        }

        if (myHealthBar == null)
        {
            UI_HealthBar healthBarComponent = GetComponentInChildren<UI_HealthBar>();
            if (healthBarComponent != null)
                myHealthBar = healthBarComponent.gameObject;
        }

        if (_transprent)
        {
            if (myHealthBar != null)
                myHealthBar.SetActive(false);

            Color transparentColor = sr.color;
            transparentColor.a = 0.1f;
            sr.color = transparentColor;

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;
        }
        else
        {
            if (myHealthBar != null)
                myHealthBar.SetActive(true);

            Color opaqueColor = sr.color;
            opaqueColor.a = 1f;
            sr.color = opaqueColor;

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = true;
        }
    }
}
