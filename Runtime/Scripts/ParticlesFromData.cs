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
        public Material mat;
        public int lifetime = 1;
        public int maxParticleCount = 50000; // number of particles to spawn

        public GameObject particleGOPrefab;

        // Use this for initialization

        private void Start()
        {
            ps = gameObject.GetComponent<ParticleSystem>();
            psMain = ps.main;

            psMain.startLifetime = lifetime;
            psMain.maxParticles = maxParticleCount;
            psMain.simulationSpace = ParticleSystemSimulationSpace.World;
            psMain.startSpeed = 0f;
            psMain.startSize = 0.02f;

            psRen = GetComponent<ParticleSystemRenderer>();
            psRen.material = mat;

            psEmi = ps.emission;
            psEmi.enabled = true;
            psEmi.rateOverTime = maxParticleCount;
        }

        public void Set(List<Vector3> vertices, List<Color32> colors)
        {
            Set(ps, vertices, colors);
        }

        public void Set(ParticleSystem ps, List<Vector3> vertices, List<Color32> colors)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[vertices.Count];
            ps.GetParticles(particles);

            for (int i = 0; i < vertices.Count; i++)
            {
                particles[i].position = transform.TransformPoint(vertices[i]);
                particles[i].startColor = colors[i];
            }
            ps.SetParticles(particles, vertices.Count);
        }


        public void New(List<Vector3> vertices, List<Color32> colors, float duration = 0)
        {
            var ps = Instantiate(particleGOPrefab).GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            //main.gravityModifier = 1;

            main.startLifetime = lifetime;
            main.maxParticles = maxParticleCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.startSize = 0.02f;

            Set(ps, vertices, colors);

            if (duration > 0)
                Destroy(ps.gameObject, duration);
        }
    }
}