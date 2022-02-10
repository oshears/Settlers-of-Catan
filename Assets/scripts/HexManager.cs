using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catan
{
    public class HexManager
    {
        public Hex[] hexArray;
        public Point[] pointArray;
        public Edge[] edgeArray;
        public Port[] ports;

        int numPlayers;

        public HexManager(int numPlayers)
        {
            // store the number of players
            this.numPlayers = numPlayers;

            // configure array of hexes and hex manager
            hexArray = new Hex[CatanGame.NUM_HEXES];

            // configure hex values
            int[] hexValues = new int[] { 10, 2, 9, 12, 6, 4, 10, 9, 11, -1, 3, 8, 8, 3, 4, 5, 5, 6, 11 };
            int[] hexBiomes = new int[] {
                CatanGame.ID_Mountain,
                CatanGame.ID_Pasture,
                CatanGame.ID_Wood,
                CatanGame.ID_Farm,
                CatanGame.ID_Hill,
                CatanGame.ID_Pasture,
                CatanGame.ID_Hill,
                CatanGame.ID_Farm,
                CatanGame.ID_Forest,
                CatanGame.ID_Desert,
                CatanGame.ID_Forest,
                CatanGame.ID_Mountain,
                CatanGame.ID_Forest,
                CatanGame.ID_Mountain,
                CatanGame.ID_Farm,
                CatanGame.ID_Pasture,
                CatanGame.ID_Hill,
                CatanGame.ID_Farm,
                CatanGame.ID_Pasture
            };

            // store hexes
            for (int i = 0; i < CatanGame.NUM_HEXES; i++)
            {
                //var hex_text = GameObject.Find("hex_text_" + i);
                //Debug.Log(hex_text);
                //hexArray[i] = new Hex(hexValues[i], hex_text);
                hexArray[i] = new Hex(hexValues[i], hexBiomes[i], GameObject.Find("hex_text_" + i));
            
            }

        

            pointArray = new Point[CatanGame.NUM_POINTS];
            for (int i = 0; i < pointArray.Length; i++)
            {
                //Debug.Log("Linking settlement: " + GameObject.Find("settlement_" + i).name);
                pointArray[i] = new Point(i, GameObject.Find("settlement_" + i));
            }

            edgeArray = new Edge[CatanGame.NUM_EDGES];
            for (int i = 0; i < edgeArray.Length; i++)
            {
                //Debug.Log("Linking road: " + GameObject.Find("road_" + i).name);
                edgeArray[i] = new Edge(i, GameObject.Find("road_" + i));
            }

            ports = new Port[CatanGame.NUM_PORTS];
            ports[0] = new Port(0, -1, true);
            ports[1] = new Port(1, CatanGame.ID_Wheat);
            ports[2] = new Port(2, CatanGame.ID_Stone);
            ports[3] = new Port(3, -1, true);
            ports[4] = new Port(4, CatanGame.ID_Sheep);
            ports[5] = new Port(5, -1, true);
            ports[6] = new Port(6, -1, true);
            ports[7] = new Port(7, CatanGame.ID_Brick);
            ports[8] = new Port(8, CatanGame.ID_Wood);

            this.connectBoard();
        }

        public void updateHexMaterials(int dieValue)
        {
            for (int i = 0; i < hexArray.Length; i++)
            {
                Hex hex = hexArray[i];
                //Debug.Log("die value = " + dieValue + "hex[" + i + "] = " + hex.getNumValue());
                if (hex.getNumValue() == dieValue)
                {
                    hex.highlight();
                }
                else
                {
                    hex.clear();
                }
            }
        }
        public void setRoadPlacementMode(int potentialOwner)
        {
            foreach (Edge edge in edgeArray)
            {
                edge.setPlacementMode(true, potentialOwner);
            }

        }
        public void setSettlementPlacementMode(int potentialOwner, bool inSetup = false)
        {
            foreach (Point point in pointArray)
            {
                point.setPlacementMode(true, potentialOwner, inSetup, false);
            }
        }
        public void setCityPlacementMode(int potentialOwner)
        {
            foreach (Point point in pointArray)
            {
                point.setPlacementMode(true, potentialOwner, false, true);
            }
        }
        
        public void clearPlacementMode()
        {
            foreach (Point point in pointArray)
            {
                point.setPlacementMode(false, -1, false);
            }

            foreach (Edge edge in edgeArray)
            {
                edge.setPlacementMode(false, -1);
            }
        }

        public List<int[]> getPlayerResources(int dieValue, bool force = false)
        {
            List<int[]> playerResources = new List<int[]>();
            for(int i = 0; i < this.numPlayers; i++)
            {
                playerResources.Add(new int[] { 0, 0, 0, 0, 0 });
            }

            // for each point on the board
            foreach (Point point in pointArray)
            {
                // for each hex connected to this point
                foreach(Hex hex in point.adjacentHexes)
                {
                    // if the hex matches the dice value, or resources are forced
                    // AND there is a settlement at the point
                    if((hex.getNumValue() == dieValue || force) && point.hasSettlement() && !hex.isDesert())
                    {
                        // if the settlement is a city, give the owner of the settlement 2 of that resource
                        if (point.isCity())
                        {
                            playerResources[point.getOwner()][hex.getResourceType()] += 2;
                        }
                        // else give the owner of the settlement 1 of that resource
                        else
                        {
                            playerResources[point.getOwner()][hex.getResourceType()] += 1;
                        }
                    }
                }
            }

            return playerResources;
        }

        public void enablePortsForPlayer(int player)
        {
            for(int i = 0; i < CatanGame.NUM_POINTS; i++)
            {
                if (pointArray[i].getOwner() == player) pointArray[i].enablePortSelection();
            }
        }

        public void disablePorts()
        {
            for (int i = 0; i < CatanGame.NUM_POINTS; i++)
            {
                pointArray[i].disablePortSelection();
            }
        }

        void connectBoard()
        {
            // connection lists
            List < int[] > edge_edge_list = new List< int[] > ();
            List < int[] > edge_settlement_list = new List< int[] > ();
            List < int[] > settlement_hex_list = new List< int[] > ();

            // edge -> edge connections
            edge_edge_list.Add(new int[] { 6, 1 }); //road_0
            edge_edge_list.Add(new int[] { 0, 7, 2}); //road_1
            edge_edge_list.Add(new int[] { 1, 7, 3}); //road_2
            edge_edge_list.Add(new int[] { 2, 8, 4}); //road_3
            edge_edge_list.Add(new int[] { 3, 8, 5}); //road_4
            edge_edge_list.Add(new int[] { 4, 9}); //road_5
            edge_edge_list.Add(new int[] { 0, 10, 11}); //road_6
            edge_edge_list.Add(new int[] { 1, 2, 12, 13}); //road_7
            edge_edge_list.Add(new int[] { 3, 4, 14, 15}); //road_8
            edge_edge_list.Add(new int[] { 5, 16, 17}); //road_9
            edge_edge_list.Add(new int[] { 18, 6, 11}); //road_10
            edge_edge_list.Add(new int[] { 6, 10, 12, 19}); //road_11
            edge_edge_list.Add(new int[] {11,19,7,13}); //road_12
            edge_edge_list.Add(new int[] {12,7,14,20}); //road_13
            edge_edge_list.Add(new int[] {13,20,8,15}); //road_14
            edge_edge_list.Add(new int[] {14,8,16,21}); //road_15
            edge_edge_list.Add(new int[] {15,21,9,17}); //road_16
            edge_edge_list.Add(new int[] {16,9,22}); //road_17
            edge_edge_list.Add(new int[] {10,23,24}); //road_18
            edge_edge_list.Add(new int[] {11,12,25,26}); //road_19
            edge_edge_list.Add(new int[] {13,14,27,28}); //road_20
            edge_edge_list.Add(new int[] {15,16,29,30}); //road_21
            edge_edge_list.Add(new int[] {17,31,32}); //road_22
            edge_edge_list.Add(new int[] {18,33,24}); //road_23
            edge_edge_list.Add(new int[] {18,23,25,34}); //road_24
            edge_edge_list.Add(new int[] {24,34,19,26}); //road_25
            edge_edge_list.Add(new int[] {25,19,27,35}); //road_26
            edge_edge_list.Add(new int[] {26,35,20,28}); //road_27
            edge_edge_list.Add(new int[] {27,20,29,36}); //road_28
            edge_edge_list.Add(new int[] {28,36,21,30}); //road_29
            edge_edge_list.Add(new int[] {29,21,37,31}); //road_30
            edge_edge_list.Add(new int[] {30,37,22,32}); //road_31
            edge_edge_list.Add(new int[] {22,31,38}); //road_32
            edge_edge_list.Add(new int[] {23,39}); //road_33
            edge_edge_list.Add(new int[] {24,25,40,41}); //road_34
            edge_edge_list.Add(new int[] {26,27,42,43}); //road_35
            edge_edge_list.Add(new int[] {28,29,44,45}); //road_36
            edge_edge_list.Add(new int[] {30,31,46,47}); //road_37
            edge_edge_list.Add(new int[] {32,48}); //road_38
            edge_edge_list.Add(new int[] {33,40,49}); //road_39
            edge_edge_list.Add(new int[] {39,49,34,41}); //road_40
            edge_edge_list.Add(new int[] {34,40,42,50}); //road_41
            edge_edge_list.Add(new int[] {35,41,43,50}); //road_42
            edge_edge_list.Add(new int[] {35,42,44,51}); //road_43
            edge_edge_list.Add(new int[] {36,43,45,51}); //road_44
            edge_edge_list.Add(new int[] {36,44,46,52}); //road_45
            edge_edge_list.Add(new int[] {37,45,47,52}); //road_46
            edge_edge_list.Add(new int[] {37,46,48,53}); //road_47
            edge_edge_list.Add(new int[] {47,48,53}); //road_48
            edge_edge_list.Add(new int[] {39,40,54}); //road_49
            edge_edge_list.Add(new int[] {41,42,55,56}); //road_50
            edge_edge_list.Add(new int[] {43,44,57,58}); //road_51
            edge_edge_list.Add(new int[] {45,46,59,60}); //road_52
            edge_edge_list.Add(new int[] {47,48,61}); //road_53
            edge_edge_list.Add(new int[] {49,55,62}); //road_54
            edge_edge_list.Add(new int[] {50,54,56,62}); //road_55
            edge_edge_list.Add(new int[] {50,55,57,63}); //road_56
            edge_edge_list.Add(new int[] {51,56,58,63}); //road_57
            edge_edge_list.Add(new int[] {51,57,59,64}); //road_58
            edge_edge_list.Add(new int[] {52,58,60,64}); //road_59
            edge_edge_list.Add(new int[] {52,59,61,65}); //road_60
            edge_edge_list.Add(new int[] {53,60,65}); //road_61
            edge_edge_list.Add(new int[] {54,55,66}); //road_62
            edge_edge_list.Add(new int[] {56,57,67,68}); //road_63
            edge_edge_list.Add(new int[] {58,59,69,70}); //road_64
            edge_edge_list.Add(new int[] {60,61,65}); //road_65
            edge_edge_list.Add(new int[] {62,67}); //road_66
            edge_edge_list.Add(new int[] {63,66,68}); //road_67
            edge_edge_list.Add(new int[] {63,67,68}); //road_68
            edge_edge_list.Add(new int[] {64,68,70}); //road_69
            edge_edge_list.Add(new int[] {64,69,71}); //road_70
            edge_edge_list.Add(new int[] {65,70}); //road_71

            // edge -> settlement connections
            edge_settlement_list.Add(new int[] {0,3}); // road_0
            edge_settlement_list.Add(new int[] {0,4}); // road_1
            edge_settlement_list.Add(new int[] {1,4}); // road_2
            edge_settlement_list.Add(new int[] {1,5}); // road_3
            edge_settlement_list.Add(new int[] {2,5}); // road_4
            edge_settlement_list.Add(new int[] {2,6}); // road_5
            edge_settlement_list.Add(new int[] {3,7}); // road_6
            edge_settlement_list.Add(new int[] {4,8}); // road_7
            edge_settlement_list.Add(new int[] {5,9}); // road_8
            edge_settlement_list.Add(new int[] {6,10}); // road_9
            edge_settlement_list.Add(new int[] {7,11}); // road_10
            edge_settlement_list.Add(new int[] {7,12}); // road_11
            edge_settlement_list.Add(new int[] {8,12}); // road_12
            edge_settlement_list.Add(new int[] {8,13}); // road_13
            edge_settlement_list.Add(new int[] {9,13}); // road_14
            edge_settlement_list.Add(new int[] {9,14}); // road_15
            edge_settlement_list.Add(new int[] {10,14}); // road_16
            edge_settlement_list.Add(new int[] {10,15}); // road_17
            edge_settlement_list.Add(new int[] {11,16}); // road_18
            edge_settlement_list.Add(new int[] {12,17}); // road_19
            edge_settlement_list.Add(new int[] {13,18}); // road_20
            edge_settlement_list.Add(new int[] {14,19}); // road_21
            edge_settlement_list.Add(new int[] {15,20}); // road_22
            edge_settlement_list.Add(new int[] {16,21}); // road_23
            edge_settlement_list.Add(new int[] {16,22}); // road_24
            edge_settlement_list.Add(new int[] {17,22}); // road_25
            edge_settlement_list.Add(new int[] {17,23}); // road_26
            edge_settlement_list.Add(new int[] {18,23}); // road_27
            edge_settlement_list.Add(new int[] {18,24}); // road_28
            edge_settlement_list.Add(new int[] {19,24}); // road_29
            edge_settlement_list.Add(new int[] {19,25}); // road_30
            edge_settlement_list.Add(new int[] {20,25}); // road_31
            edge_settlement_list.Add(new int[] {20,26}); // road_32
            edge_settlement_list.Add(new int[] {21,27}); // road_33
            edge_settlement_list.Add(new int[] {22,28}); // road_34
            edge_settlement_list.Add(new int[] {23,29}); // road_35
            edge_settlement_list.Add(new int[] {24,30}); // road_36
            edge_settlement_list.Add(new int[] {25,31}); // road_37
            edge_settlement_list.Add(new int[] {26,32}); // road_38
            edge_settlement_list.Add(new int[] {27,33}); // road_39
            edge_settlement_list.Add(new int[] {28,33}); // road_40
            edge_settlement_list.Add(new int[] {28,34}); // road_41
            edge_settlement_list.Add(new int[] {29,34}); // road_42
            edge_settlement_list.Add(new int[] {29,35}); // road_43
            edge_settlement_list.Add(new int[] {30,35}); // road_44
            edge_settlement_list.Add(new int[] {30,36}); // road_45
            edge_settlement_list.Add(new int[] {31,36}); // road_46
            edge_settlement_list.Add(new int[] {31,37}); // road_47
            edge_settlement_list.Add(new int[] {32,37}); // road_48
            edge_settlement_list.Add(new int[] {33,38}); // road_49
            edge_settlement_list.Add(new int[] {34,39}); // road_50
            edge_settlement_list.Add(new int[] {35,40}); // road_51
            edge_settlement_list.Add(new int[] {36,41}); // road_52
            edge_settlement_list.Add(new int[] {37,42}); // road_53
            edge_settlement_list.Add(new int[] {38,43}); // road_54
            edge_settlement_list.Add(new int[] {39,43}); // road_55
            edge_settlement_list.Add(new int[] {39,44}); // road_56
            edge_settlement_list.Add(new int[] {40,44}); // road_57
            edge_settlement_list.Add(new int[] {40,45}); // road_58
            edge_settlement_list.Add(new int[] {41,45}); // road_59
            edge_settlement_list.Add(new int[] {41,46}); // road_60
            edge_settlement_list.Add(new int[] {42,46}); // road_61
            edge_settlement_list.Add(new int[] {43,47}); // road_62
            edge_settlement_list.Add(new int[] {44,48}); // road_63
            edge_settlement_list.Add(new int[] {45,49}); // road_64
            edge_settlement_list.Add(new int[] {46,50}); // road_65
            edge_settlement_list.Add(new int[] {47,51}); // road_66
            edge_settlement_list.Add(new int[] {48,51}); // road_67
            edge_settlement_list.Add(new int[] {48,52}); // road_68
            edge_settlement_list.Add(new int[] {49,52}); // road_69
            edge_settlement_list.Add(new int[] {49,53}); // road_70
            edge_settlement_list.Add(new int[] {50,53}); // road_71

            // populate neighboring edges and settlements for each edge
            for (int edge_id = 0; edge_id < edge_edge_list.Count; edge_id++)
            {
                foreach (int adj_edge_id in edge_edge_list[edge_id]) this.edgeArray[edge_id].adjacentRoads.Add(this.edgeArray[adj_edge_id]);
            }

            for (int edge_id = 0; edge_id < edge_settlement_list.Count; edge_id++)
            {
                foreach (int adj_settlement_id in edge_settlement_list[edge_id])
                {
                    //Debug.Log("Connecting ROAD_" + edge_id + " to SETTLEMENT_" + adj_settlement_id);
                    this.edgeArray[edge_id].adjacentPoints.Add(this.pointArray[adj_settlement_id]);

                    // add edges to settlements
                    this.pointArray[adj_settlement_id].adjacentRoads.Add(edgeArray[edge_id]);
                }
            }

            // settlement -> hex connections
            settlement_hex_list.Add(new int[] {0}); // settlement_0
            settlement_hex_list.Add(new int[] {1}); // settlement_1
            settlement_hex_list.Add(new int[] {2}); // settlement_2
            settlement_hex_list.Add(new int[] {0}); // settlement_3
            settlement_hex_list.Add(new int[] {0,1}); // settlement_4
            settlement_hex_list.Add(new int[] {1,2}); // settlement_5
            settlement_hex_list.Add(new int[] {2}); // settlement_6
            settlement_hex_list.Add(new int[] {0,3}); // settlement_7
            settlement_hex_list.Add(new int[] {0,1,4}); // settlement_8
            settlement_hex_list.Add(new int[] {1,2,5}); // settlement_9
            settlement_hex_list.Add(new int[] {2,6}); // settlement_10
            settlement_hex_list.Add(new int[] {3}); // settlement_11
            settlement_hex_list.Add(new int[] {0,3,4}); // settlement_12
            settlement_hex_list.Add(new int[] {1,4,5}); // settlement_13
            settlement_hex_list.Add(new int[] {2,5,6}); // settlement_14
            settlement_hex_list.Add(new int[] {6}); // settlement_15
            settlement_hex_list.Add(new int[] { 3, 7 }); // settlement_16
            settlement_hex_list.Add(new int[] { 3, 4, 8 }); // settlement_17
            settlement_hex_list.Add(new int[] { 4, 5, 9 }); // settlement_18
            settlement_hex_list.Add(new int[] { 5, 6, 10 }); // settlement_19
            settlement_hex_list.Add(new int[] { 6, 11 }); // settlement_20
            settlement_hex_list.Add(new int[] {7}); // settlement_21
            settlement_hex_list.Add(new int[] {3,7,8}); // settlement_22
            settlement_hex_list.Add(new int[] {4,8,9}); // settlement_23
            settlement_hex_list.Add(new int[] {5,9,10}); // settlement_24
            settlement_hex_list.Add(new int[] {6,10,11}); // settlement_25
            settlement_hex_list.Add(new int[] {11}); // settlement_26
            settlement_hex_list.Add(new int[] {7}); // settlement_27
            settlement_hex_list.Add(new int[] {7,8,12}); // settlement_28
            settlement_hex_list.Add(new int[] {8,9,13}); // settlement_29
            settlement_hex_list.Add(new int[] {9,10,14}); // settlement_30
            settlement_hex_list.Add(new int[] {10,11,15}); // settlement_31
            settlement_hex_list.Add(new int[] {11}); // settlement_32
            settlement_hex_list.Add(new int[] {7,12}); // settlement_33
            settlement_hex_list.Add(new int[] {8,12,13}); // settlement_34
            settlement_hex_list.Add(new int[] {9,13,14}); // settlement_35
            settlement_hex_list.Add(new int[] {10,14,15}); // settlement_36
            settlement_hex_list.Add(new int[] {11,15}); // settlement_37
            settlement_hex_list.Add(new int[] {12}); // settlement_38
            settlement_hex_list.Add(new int[] {12,13,16}); // settlement_39
            settlement_hex_list.Add(new int[] {13,14,17}); // settlement_40
            settlement_hex_list.Add(new int[] {14,15,18}); // settlement_41
            settlement_hex_list.Add(new int[] {15}); // settlement_42
            settlement_hex_list.Add(new int[] {12,16}); // settlement_43
            settlement_hex_list.Add(new int[] {13,16,17}); // settlement_44
            settlement_hex_list.Add(new int[] {14,17,18}); // settlement_45
            settlement_hex_list.Add(new int[] {15,18}); // settlement_46
            settlement_hex_list.Add(new int[] {16}); // settlement_47
            settlement_hex_list.Add(new int[] {16,17}); // settlement_48
            settlement_hex_list.Add(new int[] {17,18}); // settlement_49
            settlement_hex_list.Add(new int[] {18}); // settlement_50
            settlement_hex_list.Add(new int[] {16}); // settlement_51
            settlement_hex_list.Add(new int[] {17}); // settlement_52
            settlement_hex_list.Add(new int[] {18}); // settlement_53

            for (int settlement_id = 0; settlement_id < settlement_hex_list.Count; settlement_id++)
            {
                foreach (int adj_hex_id in settlement_hex_list[settlement_id])
                {
                    //Debug.Log("Connecting SETTLEMENT_" + settlement_id + " to HEX_" + adj_hex_id);
                    this.pointArray[settlement_id].adjacentHexes.Add(this.hexArray[adj_hex_id]);
                }
            }

            // set ports for given points
            pointArray[0].setPort(ports[0]);
            pointArray[3].setPort(ports[0]);
            pointArray[1].setPort(ports[1]);
            pointArray[5].setPort(ports[1]);
            pointArray[10].setPort(ports[2]);
            pointArray[15].setPort(ports[2]);
            pointArray[26].setPort(ports[3]);
            pointArray[32].setPort(ports[3]);
            pointArray[42].setPort(ports[4]);
            pointArray[46].setPort(ports[4]);
            pointArray[49].setPort(ports[5]);
            pointArray[52].setPort(ports[5]);
            pointArray[51].setPort(ports[6]);
            pointArray[47].setPort(ports[6]);
            pointArray[38].setPort(ports[7]);
            pointArray[33].setPort(ports[7]);
            pointArray[16].setPort(ports[8]);
            pointArray[11].setPort(ports[8]);
        }

        public int getNumPlayers()
        {
            return this.numPlayers;
        }
    }

    public class Point
    {
        public int owner;
        public int id;
        public int portId;

        public bool settlementPlaced;
        public bool cityPlaced;

        public List<Hex> adjacentHexes;
        public List<Edge> adjacentRoads;

        public GameObject settlementGameObject;
        //public GameObject portGameObject;
        public Port port;

        public Point(int id, GameObject settlementGameObject)
        {
            this.id = id;
            this.settlementGameObject = settlementGameObject;
            this.settlementGameObject.GetComponent<SettlementLogic>().setId(id);

            this.settlementPlaced = false;

            this.adjacentHexes = new List<Hex>();
            this.adjacentRoads = new List<Edge>();

            this.owner = -1;

            portId = -1;
        }

        public bool canHostSettlementFor(int potentialOwner, bool inSetup)
        {
            // Rule 1: Cannot place if settlement already exists

            // Rule 2: Cannot place if there are neighboring settlements
            bool hasNeighbors = false;
            for (int i = 0; i < adjacentRoads.Count; i++)
            {
                for (int j = 0; j < adjacentRoads[i].adjacentPoints.Count; j++)
                {
                    Point neighboringPoint = adjacentRoads[i].adjacentPoints[j];
                
                    // cannot place if the settlement on this edge is not the current point and the point has a settlement
                    // a settlement cannot be placed here
                    if (neighboringPoint.id != this.id && neighboringPoint.hasSettlement())
                    {
                        hasNeighbors = true;
                    }

                }
            }

            // Rule 3: Cannot place if there are no neighboring roads (when game is beyond the setup phase)
            bool hasAdjacentOwnedRoads = false;
            for (int i = 0; i < adjacentRoads.Count; i++)
            {
                if (adjacentRoads[i].getOwner() == potentialOwner) hasAdjacentOwnedRoads = true;
            }

            //Debug.Log("Evaluation: settlementPlaced = " + settlementPlaced + "; hasNeighbors = " + hasNeighbors + "; hasAdjacentOwnedRoads = " + hasAdjacentOwnedRoads + "; inSetup = " + inSetup);

            return !settlementPlaced && !hasNeighbors && (hasAdjacentOwnedRoads || inSetup);
        }

        public bool canHostCityFor(int potentialOwner)
        {
            // Rule 1: Must have an existing settlement for this player
            //print("City can be built at point_"+_id+" = settlementPlaced = " + settlementPlaced +)
            return settlementPlaced && (potentialOwner == owner);
        }

        public void setOwner(int owner)
        {
            this.owner = owner;
        }
        public int getOwner()
        {
            return this.owner;
        }

        public bool hasSettlement()
        {
            return settlementPlaced;
        }

        public void setHasSettlement(bool settlementPlaced)
        {
            this.settlementPlaced = settlementPlaced;
        }

        public void setSettlementPlaced()
        {
            this.settlementPlaced = true;
        }

        public void setPlacementMode(bool placementMode, int potentialOwner, bool inSetup = false, bool placeCity = false)
        {
            //Debug.Log("Configuring placement mode for point " + id + " with settings: mode = " + placementMode + "; potentialOwner = " + potentialOwner + "; inSetup = " + inSetup);
            //Debug.Log("Requesting a city to be built = "+placeCity);
            if (canHostSettlementFor(potentialOwner, inSetup) && placementMode && !placeCity)
            {
                //Debug.Log("Enabling collider for game object: " + settlementGameObject.name);
                settlementGameObject.GetComponent<SettlementLogic>().setCollider(placementMode);
            }
            else if (canHostCityFor(potentialOwner) && placementMode && placeCity)
            {
                //Debug.Log("Allowing a city to be built at: point_" + id);
                settlementGameObject.GetComponent<SettlementLogic>().setCollider(placementMode);
            }
            else
            {
                settlementGameObject.GetComponent<SettlementLogic>().setCollider(false);
            }
        }

        public void setIsCity(bool cityPlaced)
        {
            this.cityPlaced = cityPlaced;
        }

        public bool isCity()
        {
            return this.cityPlaced;
        }

        public void setPort(Port port)
        {
            if (port == null)
            {
                Debug.LogError("ERROR: No valid port provided to point_" + id);
            }

            this.portId = port.getId();
            this.port = port;
        }

        public bool hasPort()
        {
            return port != null && portId != -1;
        }

        public void enablePortSelection()
        {
            if (hasPort()) port.enablePortSelection();
        }
        public void disablePortSelection()
        {
            if(hasPort()) port.disablePortSelection();
        }
    }

    public class Edge
    {
        public int id;
        public int owner;

        bool roadPlaced;

        //public Point[] adjacentPoints;
        //public Edge[] adjacentRoads;
        public List<Point> adjacentPoints;
        public List<Edge> adjacentRoads;
        public GameObject roadGameObject;

        public Edge(int id, GameObject roadGameObject)
        {
            this.id = id;
            this.roadGameObject = roadGameObject;
            this.roadGameObject.GetComponent<RoadLogic>().setId(id);

            this.roadPlaced = false;

            this.adjacentPoints = new List<Point>();
            this.adjacentRoads = new List<Edge>();

            this.owner = -1;
        }

        public bool canHostRoadFor(int potentialOwner)
        {
            // Rule 1: Cannot place if road already exists here 

            // Rule 2: Can only place if there are neighboring roads or settlements of the same owner
            bool hasNeighboringRoadsOrSettlements = false;
            for (int i = 0; i < adjacentRoads.Count; i++)
            {
                Edge neighboringRoad = adjacentRoads[i];

                // if the adjacent road is not this road and the road belongs to the potential owner of this road
                if (neighboringRoad.id != this.id && neighboringRoad.getOwner() == potentialOwner)
                {
                    hasNeighboringRoadsOrSettlements = true;
                }
            }

            for (int i = 0; i < adjacentPoints.Count; i++)
            {
                Point point = adjacentPoints[i];

                // if the adjacent point has a settlement and belongs to the potential owner of this road
                if (point.hasSettlement() && point.getOwner() == potentialOwner)
                {
                    hasNeighboringRoadsOrSettlements = true;
                }

            }

            return !roadPlaced && hasNeighboringRoadsOrSettlements;
        }

        public void setOwner(int owner)
        {
            this.owner = owner;
        }
        public int getOwner()
        {
            return this.owner;
        }

        public bool hasRoad()
        {
            return roadPlaced;
        }

        public void setRoadPlaced()
        {
            this.roadPlaced = true;
        }

        public void setPlacementMode(bool placementMode, int potentialOwner)
        {
            //Debug.Log("Configuring placement mode for edge " + id + " with settings: mode = " + placementMode + "; potentialOwner = " + potentialOwner);
            if (canHostRoadFor(potentialOwner) && placementMode)
            {
                roadGameObject.GetComponent<RoadLogic>().setCollider(placementMode);
            }
            else
            {
                roadGameObject.GetComponent<RoadLogic>().setCollider(false);
            }
        }
    }

    public class Hex
    {
        int numValue;

        GameObject hexText;

        int biome;

        public Hex(int numValue, int biome, GameObject hexText)
        {
            this.numValue = numValue;
            this.hexText = hexText;

            this.hexText.GetComponent<Renderer>().material = Resources.Load("materials/text_0", typeof(Material)) as Material;

            this.biome = biome;


            if (isDesert()) this.hexText.SetActive(false);
        }

        public int getNumValue()
        {
            return numValue;
        }

        public void highlight()
        {
            if (!isDesert()) hexText.GetComponent<Renderer>().material = Resources.Load("materials/text_1", typeof(Material)) as Material;
        }

        public void clear()
        {
            if (!isDesert()) hexText.GetComponent<Renderer>().material = Resources.Load("materials/text_0", typeof(Material)) as Material;
        }

        public int getResourceType()
        {
            return this.biome;
        }

        public bool isDesert()
        {
            return this.biome == CatanGame.ID_Desert;
        }
    }

    public class Port
    {
        int portId;
        int resource;
        bool canTradeRandomResource;
        int tradeCost;
        GameObject portIndicatorGameObject;
        public Port(int portId,int resource,bool canTradeRandomResource = false)
        {
            this.portId = portId;
            this.resource = resource;
            this.canTradeRandomResource = canTradeRandomResource;

            if (canTradeRandomResource)
            {
                tradeCost = 3;
            }
            else
            {
                tradeCost = 2;
            }

            portIndicatorGameObject = GameObject.Find("port_indicator_" + portId);
            if (portIndicatorGameObject == null)
            {
                Debug.LogError("ERROR: Could not find port_indicator_" + portId + " in new port constructor.");
            }
            portIndicatorGameObject.GetComponent<PortIndicatorLogic>().setId(portId);
        }
        public int getId()
        {
            return this.portId;
        }
        public int getResource()
        {
            return this.resource;
        }
        public bool canTradeAny()
        {
            return this.canTradeRandomResource;
        }
        public bool canTradeRandom()
        {
            return this.canTradeRandomResource;
        }
        public int getTradeCost()
        {
            return tradeCost;
        }
        public void enablePortSelection()
        {
            //Debug.Log("Enabling port_" + portId);
            this.portIndicatorGameObject.GetComponent<PortIndicatorLogic>().setCollider(true);
        }
        public void disablePortSelection()
        {
            //Debug.Log("Disabling port_" + portId);
            this.portIndicatorGameObject.GetComponent<PortIndicatorLogic>().setCollider(false);
        }
    }

}