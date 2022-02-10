using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Catan
{
    public class PortIndicatorLogic : NetworkBehaviour
    {
        float speed = 0.15f;

        int iterations = 0;
        public int id = 0;

        private bool colliderEnabled;

        GameManager gameManager;


        enum Direction { Up, Down };
        Direction direction;

        // Start is called before the first frame update
        void Start()
        {
            // indicator animation
            iterations = 0;
            direction = Direction.Up;

            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            colliderEnabled = false;
            MeshCollider[] colliders = gameObject.GetComponents<MeshCollider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Destroy(colliders[i]);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (direction == Direction.Up)
            {
                Vector3 movement = new Vector3(0, 0, 1);
                transform.Translate(movement * speed * Time.deltaTime);
            }
            else
            {
                Vector3 movement = new Vector3(0, 0, -1);
                transform.Translate(movement * speed * Time.deltaTime);
            }


            if (iterations > 500)
            {
                direction = (direction == Direction.Up) ? Direction.Down : Direction.Up;
                iterations = 0;
            }

            iterations++;
        }

        public void setId(int id)
        {
            this.id = id;
        }


        public void setCollider(bool enable)
        {
            if (enable && !this.colliderEnabled)
            {
                gameObject.AddComponent<MeshCollider>();
                this.colliderEnabled = true;
            }
            else
            {
                MeshCollider[] colliders = gameObject.GetComponents<MeshCollider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    Destroy(colliders[i]);
                }
                this.colliderEnabled = false;
            }
        }

        private void OnMouseEnter()
        {
            //Debug.Log("Mouse entered " + gameObject.name);
            //Debug.Log("Material[0]" + GetComponent<Renderer>().materials[0]);

            Material newMaterial = Resources.Load("materials/port_indicators/port_indicator_hover", typeof(Material)) as Material;
            Material[] newMaterials = GetComponent<Renderer>().materials;
            newMaterials[0] = newMaterial;
            GetComponent<Renderer>().materials = newMaterials;


            //Debug.Log("Changing to: " + newMaterial);
            //Debug.Log("Material[0]" + GetComponent<Renderer>().materials[0]);
        }

        private void OnMouseExit()
        {
            //Debug.Log("Mouse entered " + gameObject.name);
            //Debug.Log("Material[0]" + GetComponent<Renderer>().materials[0]);

            Material newMaterial = Resources.Load("materials/port_indicators/port_indicator", typeof(Material)) as Material;
            Material[] newMaterials = GetComponent<Renderer>().materials;
            newMaterials[0] = newMaterial;
            GetComponent<Renderer>().materials = newMaterials;

            //Debug.Log("Changing to: " + newMaterial);
            //Debug.Log("Material[0]" + GetComponent<Renderer>().materials[0]);
        }

        private void OnMouseUpAsButton()
        {

            //Debug.Log("Player clicked:" + gameObject.name);

            gameManager.portClick(id);
        }

    }
}