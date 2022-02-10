using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Catan
{

    public class SettlementLogic : NetworkBehaviour
    {
        public int owner = 0;
        public int id = 0;

        private bool placed;
        private bool isCity;
        private bool colliderEnabled;

        Mesh settlementMesh;
        Mesh cityMesh;

        Material settlement_mat;

        GameManager gameManager;

        //Material[] playerMaterials;

        // Start is called before the first frame update
        void Start()
        {
            Material newMaterial = Resources.Load("materials/settlement_invis", typeof(Material)) as Material;
            GetComponent<Renderer>().material = newMaterial;

            settlementMesh = Resources.Load("models/game_models/settlement", typeof(Mesh)) as Mesh;
            cityMesh = Resources.Load("models/game_models/city", typeof(Mesh)) as Mesh;
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            placed = false;
            isCity = false;
            colliderEnabled = false;

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void setId(int id)
        {
            this.id = id;
        }

        public int getId()
        {
            return this.id;
        }

        public void setCollider(bool enable)
        {
            //Debug.Log("Configuring collider for: " + gameObject.name + " with enable = " + enable + "; colliderEnabled = " + this.colliderEnabled);
            if (enable && !this.colliderEnabled)
            {
                gameObject.AddComponent<MeshCollider>();
                this.colliderEnabled = true;
            }
            //else if (!enable && this.colliderEnabled)
            //{
            //    Destroy(GetComponent<MeshCollider>());
            //    this.colliderEnabled = false;
            //}
            else
            {
                //Destroy(GetComponent<MeshCollider>());
                MeshCollider[] colliders = gameObject.GetComponents<MeshCollider>();
                for(int i = 0; i < colliders.Length; i++)
                {
                    Destroy(colliders[i]);
                }

                this.colliderEnabled = false;
            }
            //Debug.Log(gameObject.name + " collier_enabled = " + this.colliderEnabled);
        }

        public void setCity(bool enable)
        {
            if(enable == true && !isCity)
            {
                GetComponent<MeshFilter>().mesh = cityMesh;
                this.isCity = true;
            }
            else if(enable == false && this.isCity)
            {
                GetComponent<MeshFilter>().mesh = settlementMesh;
                this.isCity = false;
            }
        }

        private void OnMouseEnter()
        {
            if (!this.placed)
            {
                Material newMaterial = Resources.Load("materials/settlement_trans", typeof(Material)) as Material;
                GetComponent<Renderer>().material = newMaterial;
            }
            else
            {
                Material newMaterial = Resources.Load("materials/settlement_hover", typeof(Material)) as Material;
                GetComponent<Renderer>().material = newMaterial;
            }
        }

        private void OnMouseExit()
        {
            if (!this.placed)
            {
                Material newMaterial = Resources.Load("materials/settlement_invis", typeof(Material)) as Material;
                GetComponent<Renderer>().material = newMaterial;
            }
            else
            {
                Material newMaterial = settlement_mat;
                GetComponent<Renderer>().material = newMaterial;
            }
        }

        private void OnMouseUpAsButton()
        {
            if (!this.placed)
            {
                // set owner to the current player
                this.owner = gameManager.getTurn();

                gameManager.setPlayerBuiltSettlementServerRpc(id,owner);
                gameManager.setBoardResponseServerRpc();

                setPlacementServerRpc(owner);
            }
            else if (placed && !isCity)
            {
                gameManager.setPlayerBuiltCityServerRpc(id,owner);
                gameManager.setBoardResponseServerRpc();

                setCityPlacementServerRpc();
            }
        }

        // Server RPC, Client does not need to own the game object to update it
        // Client tells the serve to run this method of this object
        [ServerRpc(RequireOwnership = false)]
        void setPlacementServerRpc(int owner)
        {
            //Debug.Log("[SERVER] Client requested a placement.");
            setPlacementClientRpc(owner);
        }
        // Server tells each client to run this method on their respective objects
        [ClientRpc]
        void setPlacementClientRpc(int owner)
        {
            this.placed = true;
            this.owner = owner;
            this.settlement_mat = Resources.Load("materials/settlement_" + owner, typeof(Material)) as Material;
            GetComponent<Renderer>().material = settlement_mat;
        }

        [ServerRpc(RequireOwnership = false)]
        void setCityPlacementServerRpc()
        {
            //Debug.Log("[SERVER] Client requested a placement.");
            setCityPlacementClientRpc();
        }
        // Server tells each client to run this method on their respective objects
        [ClientRpc]
        void setCityPlacementClientRpc()
        {
            this.isCity = true;

            // change mesh
            GetComponent<MeshFilter>().mesh = cityMesh;
            //this.settlement_mat = Resources.Load("materials/settlement_" + owner, typeof(Material)) as Material;
            //GetComponent<Renderer>().material = settlement_mat;
        }
    }
}