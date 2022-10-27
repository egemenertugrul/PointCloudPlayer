using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCP
{
    public class ParticlesFromData : MonoBehaviour
    {
        ParticleSystem ps;
        ParticleSystem.MainModule psMain;
        ParticleSystemRenderer psRen;
        ParticleSystem.EmissionModule psEmi;
        public int lifetime = 1;
        //public int maxParticleCount = 50000; // number of particles to spawn

        public GameObject particleGOPrefab;

        // Use this for initialization

        private void Start()
        {
            ps = Instantiate(particleGOPrefab, transform).GetComponent<ParticleSystem>();
            ps.gameObject.layer = LayerMask.NameToLayer("PCL");

            //ps = gameObject.GetComponent<ParticleSystem>();
            psMain = ps.main;

            psMain.startLifetime = lifetime;
            //psMain.maxParticles = maxParticleCount;
            psMain.simulationSpace = ParticleSystemSimulationSpace.World;
            psMain.startSpeed = 0f;
            psMain.startSize = 0.02f;

            //psRen = GetComponent<ParticleSystemRenderer>();
            //psRen.material = mat;

            psEmi = ps.emission;
            psEmi.enabled = true;
            //psEmi.rateOverTime = maxParticleCount;
        }

        public void Set(List<Vector3> vertices, List<Color32> colors)
        {
            ps.Emit(vertices.Count);
            Set(ps, vertices, colors);
        }

        public void Set(ParticleSystem ps, List<Vector3> vertices, List<Color32> colors)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[vertices.Count];
            ParticleSystem.MainModule main = ps.main;
            ParticleSystem.EmissionModule emission = ps.emission;
            main.maxParticles = vertices.Count;
            emission.rateOverTime = vertices.Count;
            ps.GetParticles(particles);

            for (int i = 0; i < vertices.Count; i++)
            {
                particles[i].position = transform.TransformPoint(vertices[i]);
                particles[i].startColor = colors[i];
            }
            ps.SetParticles(particles, vertices.Count);
        }


        public void New(List<Vector3> vertices, List<Color32> colors)
        {
            var ps = Instantiate(particleGOPrefab).GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            //main.gravityModifier = 1;

            main.startLifetime = lifetime;
            main.maxParticles = vertices.Count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.startSize = 0.02f;

            Set(ps, vertices, colors);

            if (lifetime > 0)
                Destroy(ps.gameObject, lifetime);
        }
    }
}