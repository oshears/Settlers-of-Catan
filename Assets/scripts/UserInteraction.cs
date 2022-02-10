using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catan
{

    public class UserInteraction : MonoBehaviour
    {

        int owner = 0;
        int new_owner = 0;


        // Start is called before the first frame update
        private void Start()
        {
            Material newMaterial = Resources.Load("materials/settlement_red", typeof(Material)) as Material;
            GetComponent<Renderer>().material = newMaterial;
        }

        // Update is called once per frame
        void Update()
        {

            if (owner != new_owner)
            {
                owner = new_owner;
                if (owner == 0)
                {
                    Material newMaterial = Resources.Load("materials/settlement_red", typeof(Material)) as Material;
                    Debug.Log("old material: " + GetComponent<Renderer>().material.name);
                    Debug.Log("new material: " + newMaterial.name);
                    GetComponent<Renderer>().material = newMaterial;

                }
                else if (owner == 1)
                {
                    Material newMaterial = Resources.Load("materials/settlement_blue", typeof(Material)) as Material;
                    Debug.Log("old material: " + GetComponent<Renderer>().material.name);
                    Debug.Log("new material: " + newMaterial.name);
                    GetComponent<Renderer>().material = newMaterial;
                }
                else
                {
                    Material newMaterial = Resources.Load("materials/settlement_white", typeof(Material)) as Material;
                    Debug.Log("old material: " + GetComponent<Renderer>().material.name);
                    Debug.Log("new material: " + newMaterial.name);
                    GetComponent<Renderer>().material = newMaterial;
                }
            }
        }

        private void OnMouseUpAsButton()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rhInfo;

            // Casts the ray and get the first game object hit
            bool didHit = Physics.Raycast(ray, out rhInfo);

            if (didHit && rhInfo.collider.name == gameObject.name)
            {
                // gameObject.

                // MeshRenderer my_renderer = GetComponent<MeshRenderer>();
                // if ( my_renderer != null )
                // {
                //     Material my_material = my_renderer.material;
                // }
                // Debug.Log("Toggling Selection");
                Debug.Log("Clicked: " + gameObject.name);

                new_owner = (new_owner + 1) % 3;

            }
        }

    }
}