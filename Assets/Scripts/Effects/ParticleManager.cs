using UnityEngine;
using System.Collections; // Required for Coroutines

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [Header("Particle System Prefabs")]
    [Tooltip("Assign the PREFAB of the Orb Collect Particle System.")]
    public ParticleSystem orbCollectEffectPrefab;
    [Tooltip("Assign the PREFAB of the Explosion Particle System.")]
    public ParticleSystem explosionEffectPrefab;
    [Tooltip("Assign the PREFAB of the Jump Particle System.")]
    public ParticleSystem jumpEffectPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayJumpEffect(Vector3 position)
    {
        if (jumpEffectPrefab != null)
        {
            ParticleSystem newEffect = Instantiate(jumpEffectPrefab, position, Quaternion.identity);
            newEffect.Play(); 

            StartCoroutine(DestroyParticleSystemAfterDuration(newEffect));
        }
        else
        {
            Debug.LogWarning("Jump Effect Prefab is not assigned in ParticleManager.");
        }
    }
    public void PlayExplosionEffect(Vector3 position)
    {
        if (explosionEffectPrefab != null)
        {
            ParticleSystem newEffect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            newEffect.Play(); 

            StartCoroutine(DestroyParticleSystemAfterDuration(newEffect));
        }
        else
        {
            Debug.LogWarning("Explosion Effect Prefab is not assigned in ParticleManager.");
        }
    }
    IEnumerator DestroyParticleSystemAfterDuration(ParticleSystem ps)
    {
        float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

        yield return new WaitForSeconds(totalDuration);

        if (ps != null)
        {
            Destroy(ps.gameObject); 
        }
    }
}
