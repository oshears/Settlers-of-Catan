using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Catan
{

    public class Player
    {
        public int id;
        public string name;

        //{ "Brick", "Sheep", "Stone", "Wood", "Wheat" }
        public int[] resourceCounts;

        public int numRoads = 0;
        public int numSettlements = 0;
        public int numCities = 0;
        public int numPointCards = 0;

        public Player(int id, string name)
        {
            this.id = id;
            this.name = name;
            resourceCounts = new int[] { 0, 0, 0, 0, 0 };

            numRoads = 0;
            numSettlements = 0;
        }

        public bool canBuildRoad()
        {
            return (resourceCounts[CatanGame.ID_Brick] > 0) && (resourceCounts[CatanGame.ID_Wood] > 0);
        }

        public bool canBuildSettlement()
        {
            return (resourceCounts[CatanGame.ID_Brick] > 0) && (resourceCounts[CatanGame.ID_Wood] > 0) && (resourceCounts[CatanGame.ID_Sheep] > 0) && (resourceCounts[CatanGame.ID_Wheat] > 0);
        }

        public bool canBuildCity()
        {
            return (resourceCounts[CatanGame.ID_Stone] > 2) && (resourceCounts[CatanGame.ID_Wheat] > 1) && (numSettlements > 0);
        }

        public bool canGetDevelopmentCard()
        {
            return (resourceCounts[CatanGame.ID_Stone] > 0) && (resourceCounts[CatanGame.ID_Wheat] > 0) && (resourceCounts[CatanGame.ID_Sheep] > 0);
        }

        public bool canBuild()
        {
            return canBuildRoad() || canBuildSettlement() || canBuildCity();
        }

        public bool hasResources()
        {
            for (int i = 0; i < resourceCounts.Length; i++)
            {
                if(resourceCounts[i] > 0) return true;
            }
            return false;
        }

        public bool canTrade()
        {
            return hasResources();
        }

        public bool hasResource(int resource)
        {
            return resourceCounts[resource] > 0;
        }

        public int getResourceCount()
        {
            int count = 0;
            for(int i = 0; i < CatanGame.NUM_RESOURCES; i++)
            {
                count += resourceCounts[i];
            }
            return count;
        }

        public bool canTradeAtRandomPort()
        {
            for (int i = 0; i < CatanGame.NUM_RESOURCES; i++)
            {
                if(resourceCounts[i] > 2)
                {
                    return true;
                }
            }
            return false;
        }

        public int getPoints()
        {
            return numSettlements + numCities * 2 + numPointCards;
        }

        public void buildRoad()
        {
            resourceCounts[CatanGame.ID_Brick] -= 1;
            resourceCounts[CatanGame.ID_Wood] -= 1;
            numRoads++;
        }
        public void buildSettlement()
        {
            resourceCounts[CatanGame.ID_Brick] -= 1;
            resourceCounts[CatanGame.ID_Wood] -= 1;
            resourceCounts[CatanGame.ID_Sheep] -= 1;
            resourceCounts[CatanGame.ID_Wheat] -= 1;
            numSettlements++;
        }
        public void buildCity()
        {
            resourceCounts[CatanGame.ID_Stone] -= 3;
            resourceCounts[CatanGame.ID_Wheat] -= 2;
            numSettlements--;
            numCities++;
        }

        public void getCard()
        {
            resourceCounts[CatanGame.ID_Sheep] -= 1;
            resourceCounts[CatanGame.ID_Wheat] -= 1;
            resourceCounts[CatanGame.ID_Stone] -= 1;
        }

    }


}