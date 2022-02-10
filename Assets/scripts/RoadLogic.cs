using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Catan
{

    public class RoadLogic : NetworkBehaviour
    {
        public int owner = 0;
        public int new_owner = 0;
        public int id = 0;

        private bool placed;
        private bool colliderEnabled;

        Material road_mat;

        GameManager gameManager;

        //Material[] playerMaterials;

        // Start is called before the first frame update
        void Start()
        {
            Material newMaterial = Resources.Load("materials/settlement_invis", typeof(Material)) as Material;
            GetComponent<Renderer>().material = newMaterial;

            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            //this.playerMaterials = new Material[CatanGame.NUM_PLAYERS];
            //for (int i = 0; i < CatanGame.NUM_PLAYERS; i++)
            //{
            //    this.playerMaterials[i] = Resources.Load("materials/settlement_" + i, typeof(Material)) as Material;
            //}

            this.placed = false;
            this.colliderEnabled = false;
        }

        // Update is called once per frame
        void Update()
        {
        }
        public void setCollider(bool enable)
        {
            //Debug.Log("Configuring collider for: " + gameObject.name);
            if (enable && !this.colliderEnabled)
            {
                gameObject.AddComponent<MeshCollider>();
                this.colliderEnabled = true;
            }
            //else if (!enable && colliderEnabled)
            //{
            //    Destroy(GetComponent<MeshCollider>());
            //    colliderEnabled = false;
            //}
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

        public void setId(int id)
        {
            this.id = id;
        }

        public int getId()
        {
            return this.id;
        }

        private void OnMouseEnter()
        {
            if (!placed)
            {
                Material newMaterial = Resources.Load("materials/settlement_trans", typeof(Material)) as Material;
                GetComponent<Renderer>().material = newMaterial;
            }
        }

        private void OnMouseExit()
        {
            if (!placed)
            {
                Material newMaterial = Resources.Load("materials/settlement_invis", typeof(Material)) as Material;
                GetComponent<Renderer>().material = newMaterial;
            }
        }

        private void OnMouseUpAsButton()
        {
            if (!placed)
            {
                // set owner to the current player
                this.owner = gameManager.getTurn();

                // change material
                // change material
                //this.road_mat = playerMaterials[this.owner];
                //GetComponent<Renderer>().material = road_mat;

                //this.placed = true;


                //gameManager.setPlayerBuiltRoadServerRpc();
                //gameManager.setBoardResponseServerRpc();

                //if (NetworkManager.Singleton.IsServer)
                //{
                //    //Debug.Log("[SERVER] making placement.");
                //    setPlacementClientRpc(owner);
                //}
                //else
                //{
                //    //Debug.Log("[CLIENT] making placement.");
                //    setPlacementServerRpc(owner);
                //}

                gameManager.setPlayerBuiltRoadServerRpc(id, owner);
                gameManager.setBoardResponseServerRpc();

                setPlacementServerRpc(owner);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void setPlacementServerRpc(int owner)
        {
            //Debug.Log("[SERVER] Client requested a placement.");
            setPlacementClientRpc(owner);
        }
        [ClientRpc]
        void setPlacementClientRpc(int owner)
        {
            this.placed = true;
            this.owner = owner;
            this.road_mat = Resources.Load("materials/settlement_" + owner, typeof(Material)) as Material;
            GetComponent<Renderer>().material = road_mat;
        }
    }


}