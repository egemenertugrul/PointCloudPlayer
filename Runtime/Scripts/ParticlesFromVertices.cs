using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesFromVertices : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystem.MainModule psMain;
    ParticleSystemRenderer psRen;
    ParticleSystem.EmissionModule psEmi;
    //public float lifetime = 5;
    //public int maxParticleCount = 50000; // number of particles to spawn

    public GameObject particleGOPrefab;

    // Use this for initialization

    private void Start()
    {
        //ps = gameObject.GetComponent<ParticleSystem>();
        //ps.gameObject.layer = LayerMask.NameToLayer("PCL");

        //psMain = ps.main;

        //psMain.startLifetime = lifetime;
        //psMain.maxParticles = maxParticleCount;
        //psMain.simulationSpace = ParticleSystemSimulationSpace.World;
        //psMain.startSpeed = 0f;
        //psMain.startSize = 0.02f;

        //psRen = GetComponent<ParticleSystemRenderer>();
        //psRen.material = mat;

        //psEmi = ps.emission;
        //psEmi.enabled = true;
        //psEmi.rateOverTime = maxParticleCount;
    }

    public void Set(List<Vector3> vertices, List<Color32> colors)
    {
        Set(this.ps, vertices, colors);
    }

    public void Set(ParticleSystem ps, List<Vector3> vertices, List<Color32> colors)
    {
        // initialize particle system

        //ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        //int count = ps.GetParticles(particles);
        //for (int i = 0; i < particles.Length; i++)
        //{
        //    if (i >= vertices.Count)
        //    {
        //        particles[i].startColor = new Color(0, 0, 0, 0);
        //    }
        //    else
        //    {
        //        particles[i].position = transform.TransformPoint(vertices[i]);
        //        particles[i].startColor = colors[i];
        //    }
        //}
        //ps.SetParticles(particles, count);
        
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[vertices.Count];
        ParticleSystem.MainModule main = ps.main;
        ParticleSystem.EmissionModule emission = ps.emission;
        main.maxParticles = vertices.Count;
        emission.rateOverTime = vertices.Count;
        //emission.SetBurst(0, new ParticleSystem.Burst(0, (short)vertices.Count, 1, 1));

        ps.GetParticles(particles);

        for (int i = 0; i < vertices.Count; i++)
        {
            particles[i].position = transform.TransformPoint(vertices[i]);
            particles[i].startColor = colors[i];
        }

        if (particles.Length != vertices.Count)
        {
            Debug.LogError("here");
        }

        ps.SetParticles(particles, vertices.Count);
    }


    public void New(List<Vector3> vertices, List<Color32> colors)
    {
        var ps = Instantiate(particleGOPrefab, transform).GetComponent<ParticleSystem>();
        ps.gameObject.layer = LayerMask.NameToLayer("PCL");

        ParticleSystem.MainModule main = ps.main;
        main.gravityModifier = 0.1f;

        //main.startLifetime = lifetime;
        //main.duration = lifetime;
        main.maxParticles = vertices.Count;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.startSize = 0.02f;

        ps.Emit(main.maxParticles);

        Set(ps, vertices, colors);


        if (main.duration > 0)
            Destroy(ps.gameObject, main.duration);
    }

    void LateUpdate()
    {

    }
}
