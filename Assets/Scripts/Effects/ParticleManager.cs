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
            // Optional: DontDestroyOnLoad(gameObject); // Uncomment if ParticleManager should persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate ParticleManagers
        }
    }

    /// <summary>
    /// Instantiates and plays the orb collect particle effect at a given position,
    /// then destroys it after its duration.
    /// </summary>
    /// <param name="position">The world position where the effect should play.</param>
   

    /// <summary>
    /// Instantiates and plays the jump particle effect at a given position,
    /// then destroys it after its duration.
    /// </summary>
    /// <param name="position">The world position where the effect should play.</param>
    public void PlayJumpEffect(Vector3 position)
    {
        if (jumpEffectPrefab != null)
        {
            // Instantiate a new particle system from the prefab
            ParticleSystem newEffect = Instantiate(jumpEffectPrefab, position, Quaternion.identity);
            newEffect.Play(); // Play the effect

            // Start a coroutine to destroy the particle system after it finishes playing
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
            // Instantiate a new particle system from the prefab
            ParticleSystem newEffect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            newEffect.Play(); // Play the effect

            // Start a coroutine to destroy the particle system after it finishes playing
            StartCoroutine(DestroyParticleSystemAfterDuration(newEffect));
        }
        else
        {
            Debug.LogWarning("Explosion Effect Prefab is not assigned in ParticleManager.");
        }
    }
    /// <summary>
    /// Coroutine to wait for a particle system to finish playing and then destroy its GameObject.
    /// </summary>
    /// <param name="particleSystem">The ParticleSystem instance to destroy.</param>
    IEnumerator DestroyParticleSystemAfterDuration(ParticleSystem ps)
    {
        // Get the maximum duration of the particle system.
        // ps.main.duration is the duration of the main module (e.g., if it loops).
        // ps.main.startLifetime.constantMax is the maximum initial lifetime of any particle.
        // We add them to ensure we wait long enough for all particles to die off.
        float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

        yield return new WaitForSeconds(totalDuration);

        // Alternatively, for more precise control, wait until it's no longer alive:
        // yield return new WaitWhile(() => ps.IsAlive(true));

        if (ps != null)
        {
            Destroy(ps.gameObject); // Destroy the GameObject holding the particle system
        }
    }
}
