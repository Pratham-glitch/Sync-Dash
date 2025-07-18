using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [Header("Particle Systems")]
    public ParticleSystem orbCollectEffect;
    public ParticleSystem explosionEffect;
    public ParticleSystem jumpEffect;

    void Awake()
    {
        Instance = this;
    }

    public void PlayOrbCollectEffect(Vector3 position)
    {
        orbCollectEffect.transform.position = position;
        orbCollectEffect.Play();
    }

    public void PlayExplosionEffect(Vector3 position)
    {
        explosionEffect.transform.position = position;
        explosionEffect.Play();
    }

    public void PlayJumpEffect(Vector3 position)
    {
        jumpEffect.transform.position = position;
        jumpEffect.Play();
    }
}