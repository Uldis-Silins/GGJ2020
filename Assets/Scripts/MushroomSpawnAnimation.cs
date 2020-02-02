using UnityEngine;
using System.Collections;

public class MushroomSpawnAnimation : MonoBehaviour
{
    public float animationTime = 0.5f;
    public AudioSource audioSource;
    public AnimationCurve curve;
    public Vector3 fromScale = new Vector3(0.15f, 0.2f, 0.15f);

    public ParticleSystem spawnShroomParticles;

    private float m_animtaionTimer;

    private bool m_particlesPlayed;

    public Transform MushroomTransform { get; private set; }
    public bool InAnimation { get; private set; }

    public void Play(Transform targetTransform)
    {
        MushroomTransform = targetTransform;
        MushroomTransform.localScale = fromScale;
        InAnimation = true;
        m_animtaionTimer = 0f;

        m_particlesPlayed = false;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.Play();
    }

    private void Update()
    {
        if(InAnimation)
        {
            var t = curve.Evaluate(m_animtaionTimer / animationTime);
            MushroomTransform.localScale = Vector3.one * t;

            if(!m_particlesPlayed && m_animtaionTimer > animationTime / 4f)
            {
                spawnShroomParticles.transform.position = MushroomTransform.position;
                spawnShroomParticles.Play();
                m_particlesPlayed = true;
            }

            m_animtaionTimer += Time.deltaTime;

            if(m_animtaionTimer > animationTime)
            {
                InAnimation = false;
            }
        }
    }
}
