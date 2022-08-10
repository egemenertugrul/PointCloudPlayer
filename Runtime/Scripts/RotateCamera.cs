using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCP.Examples
{
    public class RotateCamera : MonoBehaviour
    {
        public float Speed = 20.0f;
        private Camera cam;

        // Start is called before the first frame update
        void Start()
        {
            cam = GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {

            cam.transform.RotateAround(Vector3.zero, Vector3.up, Speed * Time.deltaTime);
        }
    }
}