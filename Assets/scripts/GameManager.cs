using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace Catan
{
    public class GameManager : NetworkBehaviour
    {

        int turn;
        int[] dieValue;

        int numPlayers;

        bool nextTurn;
        bool responseFromBoard;
        bool playerBuilding;
        bool tradeModeActive;

        bool playerActive;

        public static readonly string[] resourceNames = { "Brick", "Sheep", "Stone", "Wood", "Wheat" };

        Player[] players;
        Player clientPlayer;

        public HexManager hexManager;

        public Button nextButton;
        public Button buildButton;
        public Button buildRoadButton;
        public Button buildSettlementButton;
        public Button buildCityButton;
        public Button tradeButton;
        public Button cancelTradeButton;
        public Button tradeRequestButton;
        public Button acceptTradeButton;
        public Button rejectTradeButton;
        public Button requestPortTradeButton;
        public Button cancelPortTradeButton;

        public Text incomingTradeText;
        public Text tradeOfferStatusText;
        public Text portTradeOfferStatusText;

        public Image incomingTradeWindow;

        public GameObject mainCamera;
        public GameObject tradeMenuCamera;
        public GameObject portTradeMenuCamera;

        public GameObject mainCanvas;
        public GameObject tradeMenuCanvas;
        public GameObject portTradeMenuCanvas;


        public Dropdown tradeOfferSelection;
        public Dropdown tradeRequestSelection;
        public Dropdown tradePlayerSelection;
        public Dropdown portTradeOfferSelection;
        public Dropdown portTradeRequestSelection;


        public enum Resources { Brick = 0, Sheep = 1, Stone = 2, Wood = 3, Wheat = 4 };
        public enum GameState { Game_Config = 0, Setup_0 = 1, Setup_1 = 2, Active = 3 };
        public enum SetupState { Setup_Settlement, Setup_Road };

        GameState gameState;
        SetupState setupState;

        TradeOffer tradeOffer;
        Port selectedPort;

        // when the GUI requests an update
        void OnGUI()
        {
            // Configure network connection buttons and status
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else if (gameState == GameState.Game_Config){
                StatusLabels();

                if (NetworkManager.Singleton.IsServer)
                {
                    if(NetworkManager.Singleton.ConnectedClientsList.Count > 1)
                    {
                        if (GUILayout.Button("Start Game")) serverStartGame();
                    }
                    else
                    {
                        GUILayout.Label("Waiting for players to connect...");
                    }
                }
            }
            else
            {
                StatusLabels();
            }

            GUILayout.EndArea();
        }

        // Configure network connection buttons
        static void StartButtons()
        {
            GUILayout.Label("Settlers of Catan");
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            //if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
            
        }

        static void StatusLabels()
        {
            GUILayout.Label("Settlers of Catan");

            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            GUILayout.Label("Mode: " + mode);

            GUILayout.Label("Player ID: " + NetworkManager.Singleton.LocalClientId);
        }
        
        // Start is called before the first frame update
        private void Start()
        {
            // position the camera
            //mainCamera.transform.rotation = Quaternion.Euler(new Vector3(33.3f, 35f, 0f));
            //mainCamera.transform.position = new Vector3(-270.3f, 15.85f, -7.8f);
            mainCamera.transform.rotation = Quaternion.Euler(new Vector3(33f, 35f, 0f));
            mainCamera.transform.position = new Vector3(-281f, 27f, -22f);

            // disable trade menu views
            tradeMenuCamera.gameObject.SetActive(false);
            tradeMenuCanvas.gameObject.SetActive(false);

            // disable trade menu views
            portTradeMenuCamera.gameObject.SetActive(false);
            portTradeMenuCanvas.gameObject.SetActive(false);

            mainCamera.gameObject.SetActive(true);
            mainCanvas.gameObject.SetActive(true);

            

            // set game to the game config state (waiting for clients to join)
            gameState = GameState.Game_Config;

            // initialize dice values
            dieValue = new int[] { 0, 0, 0 };

            // add buttons
            nextButton.onClick.AddListener(nextTurnBtnClick);
            buildButton.onClick.AddListener(buildBtnClick);
            buildRoadButton.onClick.AddListener(buildRoadBtnClick);
            buildSettlementButton.onClick.AddListener(buildSettlementBtnClick);
            buildCityButton.onClick.AddListener(buildCityBtnClick);
            tradeButton.onClick.AddListener(tradeBtnClick);
            cancelTradeButton.onClick.AddListener(tradeBtnClick);
            cancelPortTradeButton.onClick.AddListener(tradeBtnClick);
            tradeRequestButton.onClick.AddListener(tradeRequestBtnClick);
            acceptTradeButton.onClick.AddListener(acceptTradeBtnClick);
            rejectTradeButton.onClick.AddListener(rejectTradeBtnClick);
            requestPortTradeButton.onClick.AddListener(portTradeRequestBtnClick);

            

            // clear response from user clicking a board object
            responseFromBoard = false;

            // current turn is client 0 and next turn is false
            turn = 0;
            nextTurn = false;
            playerActive = false;

            // disable GUI buttons for all players
            disableAllButtons();

            // clear board responses for all clients
            playerBuilding = false;
            tradeModeActive = false;

            // enable the main view
            //enableMainView();

            // remove all colliders from game objects
            for(int i = 0; i < CatanGame.NUM_EDGES; i++) Destroy(GameObject.Find("road_" + i).GetComponent<MeshCollider>());
            for(int i = 0; i < CatanGame.NUM_POINTS; i++) Destroy(GameObject.Find("settlement_" + i).GetComponent<MeshCollider>());
        }

        void serverStartGame()
        {
            // record the number of players
            numPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
            configureClientPlayersClientRpc(numPlayers);

            // clear all client placement modes
            clearPlacementModeClientRpc();

            // update client GUIs
            if (NetworkManager.Singleton.IsServer) updateMainGUIClientRpc();

            // put the game in the setup state
            //gameState = GameState.Setup_0;
            setGameStateClientRpc(GameState.Setup_0);
            // put the setup state into settlement placement
            setupState = SetupState.Setup_Settlement;

            // update the list of active clients
            if (NetworkManager.Singleton.IsServer) setPlayerActiveClientRpc(turn);

            // enable placement mode for the current active client 0
            Debug.Log("Client[" + NetworkManager.Singleton.LocalClientId + "]: Active: " + playerActive + " Turn: " + turn);
            setSettlementPlacementModeClientRpc(turn,true);
        }

        [ClientRpc]
        void configureClientPlayersClientRpc(int numPlayers)
        {
            Debug.Log("Configuring Client with numPlayers = " + numPlayers);
            
            // initialize players
            this.numPlayers = numPlayers;
            players = new Player[numPlayers];
            for (int i = 0; i < numPlayers; i++) players[i] = new Player(i, "Player_" + (i + 1));
            clientPlayer = players[getClientId()];

            // initialize hex manager
            hexManager = new HexManager(getNumPlayers());
            Debug.Log("Hex Manager configured with numPlayers = " + hexManager.getNumPlayers());


            // clear colliders from existing objects
            hexManager.clearPlacementMode();
        }

        private void Update()
        {
            // =================== Setup State =================== //
            if (gameInSetupState() && responseFromBoard && isServer())
            {
                // clear all client placement modes
                clearPlacementModeClientRpc();

                // clear response from board
                responseFromBoard = false;

                switch (setupState)
                {
                    //Settlement Placement
                    case SetupState.Setup_Settlement:

                        // enter road placement state for the current player
                        setupState = SetupState.Setup_Road;

                        // set road placement mode for the current player
                        setRoadPlacementModeClientRpc(turn);

                        break;
                    //Road Placement
                    case SetupState.Setup_Road:

                        // if the second stage of placements is done
                        if (turn == 0 && gameState == GameState.Setup_1)
                        {
                            // put game in active state
                            //gameState = GameState.Active;
                            setGameStateClientRpc(GameState.Active);

                            // update player resources on all clients
                            updatePlayerResourcesClientRpc(true);

                            // roll the die
                            rollDie();

                            // update player resources based on the die rolls
                            updatePlayerResourcesClientRpc();

                            // update the GUIs for each client
                            updateMainGUIClientRpc();

                            // update port interactions
                            //Debug.Log("Calling ClientRPC to enable ports");
                            updatePortInteractionsClientRpc();

                            // configure GUI buttons on all clients
                            configureGUIButtonsClientRpc();

                            // set number of roads and settlements for all players
                            setupPlayerCountsClientRpc();
                        }
                        // if the first stage of placements is done
                        else if (turn == (getNumPlayers() - 1) && gameState == GameState.Setup_0)
                        {
                            // set game mode to the second stage
                            //gameState = GameState.Setup_1;
                            setGameStateClientRpc(GameState.Setup_1);

                            // update active player information for all clients
                            setPlayerActiveClientRpc(turn);

                            // set settlement placement mode for active client
                            setSettlementPlacementModeClientRpc(turn, true);
                        }
                        // if the current play finished placing their settlement and road
                        else if (gameState == GameState.Setup_0)
                        {
                            // increment turn for all clients
                            incrementTurnClientRpc();

                            // update active player information for all clients
                            setPlayerActiveClientRpc(turn);

                            // set settlement placement mode for active client
                            setSettlementPlacementModeClientRpc(turn,true);
                        }
                        // if in the second round of placements and the current player finished placing
                        // their seocnd settlement and road
                        else if (gameState == GameState.Setup_1)
                        {
                            // decrement turn for all clients
                            decrementTurnClientRpc();

                            // update active player information for all clients
                            setPlayerActiveClientRpc(turn);

                            // set settlement placement mode for active client
                            setSettlementPlacementModeClientRpc(turn, true);
                        }

                        // enter settlement placement state
                        setupState = SetupState.Setup_Settlement;

                        break;
                    default:
                        break;
                }

                // update GUIs for all clients
                updateMainGUIClientRpc();
            }

            // =================== Active State =================== //

            // if the player placed something on the board and the server got the message
            if (responseFromBoard && gameIsActive() && isServer())
            {
                // clear placement mode for all clients
                clearPlacementModeClientRpc();

                // clear response from board
                responseFromBoard = false;

                // clear build mode on all clients
                clearBuildModeClientRpc();

                // reset GUI for all clients
                resetGUIClientRpc();

                // update GUI for all clients
                updateMainGUIClientRpc();
            }

            // if the active player selected next turn
            if (nextTurn == true && gameIsActive() && isServer())
            {
                setupNextTurn();
            }
        }

        [ClientRpc]
        void configureGUIButtonsClientRpc()
        {
            // only enable the GUI buttons if it is this client's turn
            if (thisClientTurn())
            {
                nextButton.gameObject.SetActive(true);
                if (clientPlayer.canBuild()) buildButton.gameObject.SetActive(true);
                if (clientPlayer.canTrade()) tradeButton.gameObject.SetActive(true);
            }
            
        }

        void setupNextTurn()
        {
            // increment turn on all clients
            incrementTurnClientRpc();

            // clear the next turn notification
            nextTurn = false;

            // clear all previous placement modes
            clearPlacementModeClientRpc();

            // clear all previous build modes
            clearBuildModeClientRpc();

            // reset the GUI for each client
            resetGUIClientRpc();

            // roll the die
            rollDie();

            // update player resources based on the die rolls
            updatePlayerResourcesClientRpc();

            // update the GUIs for each client
            updateMainGUIClientRpc();

            // update port interactions
            //Debug.Log("Calling ClientRPC to enable ports");
            updatePortInteractionsClientRpc();
        }

        [ClientRpc]
        void updatePortInteractionsClientRpc()
        {
            // disable all ports
            hexManager.disablePorts();

            // if this client's turn, enable ports for this client
            if (thisClientTurn())
            {
                //Debug.Log("Enabling ports for player_"+getClientId());
                hexManager.enablePortsForPlayer((int) getClientId());
            }
        }




        [ClientRpc]
        void incrementTurnClientRpc()
        {
            turn = (turn + 1) % getNumPlayers();
            playerActive = getClientId() == (ulong)turn;
        }

        [ClientRpc]
        void decrementTurnClientRpc()
        {
            turn = (turn - 1) % getNumPlayers();
            playerActive = getClientId() == (ulong)turn;
        }

        void nextTurnBtnClick()
        {
            //Debug.Log("Client Selected Next Turn Button");
            // only register if the client is active
            if (thisClientTurn())
            {
                // set next turn on the server
                setNextTurnServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void setNextTurnServerRpc()
        {
            // set next turn to true if the game is active
            nextTurn = gameIsActive();
        }


        [ClientRpc]
        void clearBuildModeClientRpc()
        {
            playerBuilding = false;
        }

        // ====== Button Methods
        void buildBtnClick()
        {
            if (thisClientTurn() && clientPlayer.canBuild())
            {
                // if the player was not building
                if (!getPlayerBuilding())
                {
                    // enable the build buttons
                    enableBuildButtons();

                    // rename the build button to "stop building"
                    buildButton.GetComponentInChildren<Text>().text = "Stop Building";

                    // indicate that the player is building
                    playerBuilding = true;
                }
                else
                {
                    // turn off buttons
                    disableBuildButtons();

                    // clear placement mode for the current client
                    hexManager.clearPlacementMode();

                    // revert the build button text
                    buildButton.GetComponentInChildren<Text>().text = "Build";

                    // indicate that the player is not building
                    playerBuilding = false;
                }
            }
        }

        void buildRoadBtnClick()
        {
            if (thisClientTurn())
            {
                // clear previous placement modes
                hexManager.clearPlacementMode();

                // start road placement mode
                hexManager.setRoadPlacementMode(turn);

                // turn off buttons
                disableBuildButtons();

                // deactivate build button if player can no longer build
                if (!clientPlayer.canBuild()) buildButton.gameObject.SetActive(false);
            }
        }
        void buildSettlementBtnClick()
        {
            if (thisClientTurn())
            {
                // clear placement modes
                hexManager.clearPlacementMode();

                // set settlement placement mode
                hexManager.setSettlementPlacementMode(turn);

                // turn off buttons
                disableBuildButtons();

                // deactivate build button if player can no longer build
                if (!clientPlayer.canBuild()) buildButton.gameObject.SetActive(false);
            }
        }
        void buildCityBtnClick()
        {
            if (thisClientTurn())
            {
                // clear placement modes
                hexManager.clearPlacementMode();

                // set settlement/city placement mode
                hexManager.setCityPlacementMode(turn);

                // turn off buttons
                disableBuildButtons();

                // deactivate build button if player can no longer build
                if (!clientPlayer.canBuild()) buildButton.gameObject.SetActive(false);
            }
        }
        void enableBuildButtons()
        {
            // only enable if its the client's turn
            if (thisClientTurn())
            {
                // turn on build buttons
                if (clientPlayer.canBuildRoad()) buildRoadButton.gameObject.SetActive(true);
                if (clientPlayer.canBuildSettlement()) buildSettlementButton.gameObject.SetActive(true);
                if (clientPlayer.canBuildCity()) buildCityButton.gameObject.SetActive(true);
            }
        }
        void disableBuildButtons()
        {
            // turn off buttons
            buildRoadButton.gameObject.SetActive(false);
            buildSettlementButton.gameObject.SetActive(false);
            buildCityButton.gameObject.SetActive(false);
        }
        void disableAllButtons()
        {
            // turn off build buttons
            disableBuildButtons();

            // turn off other buttons
            nextButton.gameObject.SetActive(false);
            buildButton.gameObject.SetActive(false);
            tradeButton.gameObject.SetActive(false);

            // clear incoming trade prompt
            clearIncomingTradeWindow();
        }
        ////////////////////////////////////

        [ClientRpc]
        void resetGUIClientRpc()
        {
            if (thisClientTurn())
            {
                nextButton.gameObject.SetActive(true);
                if (clientPlayer.canBuild()) buildButton.gameObject.SetActive(true);
                if (clientPlayer.canTrade()) tradeButton.gameObject.SetActive(true);
            }
            else
            {
                nextButton.gameObject.SetActive(false);
                buildButton.gameObject.SetActive(false);
                tradeButton.gameObject.SetActive(false);
            }
            buildRoadButton.gameObject.SetActive(false);
            buildSettlementButton.gameObject.SetActive(false);
            buildCityButton.gameObject.SetActive(false);
            buildButton.GetComponentInChildren<Text>().text = "Build";
        }

        // Update GUIs on all clients
        [ServerRpc(RequireOwnership = false)]
        void updateMainGUIServerRpc()
        {
            updateMainGUIClientRpc();
        }
        [ClientRpc]
        void updateMainGUIClientRpc()
        {
            if(!tradeModeActive)
            {
                // game info text
                var game_info_gui_text = GameObject.Find("GameInfoText");
                //Debug.Log("Game Info GUI Text: " + game_info_gui_text);
                game_info_gui_text.GetComponent<Text>().text = "";
                game_info_gui_text.GetComponent<Text>().text += "Phase: " + (gameInSetupState() ? "Setup" : "Active") + "\n";
                game_info_gui_text.GetComponent<Text>().text += "Player Turn: " + players[turn].name + "\n";
                game_info_gui_text.GetComponent<Text>().text += "Dice 1 Value: " + dieValue[0] + "\n";
                game_info_gui_text.GetComponent<Text>().text += "Dice 2 Value: " + dieValue[1] + "\n";
                game_info_gui_text.GetComponent<Text>().text += "Dice Value: " + dieValue[2] + "\n";
                for (int i = 0; i < players.Length; i++)
                {
                    game_info_gui_text.GetComponent<Text>().text += "P" + players[i].id + ": Points: " + players[i].getPoints() + "; Resources: " + players[i].getResourceCount() + "\n";
                }

                // player info text
                var player_info_gui_text = GameObject.Find("PlayerInfoText");
                player_info_gui_text.GetComponent<Text>().text = "";
                player_info_gui_text.GetComponent<Text>().text += clientPlayer.name + "'s " + "Resources:\n";
                for (int i = 0; i < resourceNames.Length; i++)
                {
                    player_info_gui_text.GetComponent<Text>().text += resourceNames[i] + ": " + clientPlayer.resourceCounts[i] + "\n";
                }

                // update hex highlights
                hexManager.updateHexMaterials(getDieValue());

                // disable buttons if it is not this client's turn
                if (!thisClientTurn())
                {
                    disableAllButtons();
                }
                else
                {
                    // disable build and trade buttons if the client no longer has enough resources
                    if (clientPlayer.canBuild()) buildButton.gameObject.SetActive(true);
                    else buildButton.gameObject.SetActive(false);
                    if (clientPlayer.canTrade()) tradeButton.gameObject.SetActive(true);
                    else tradeButton.gameObject.SetActive(false);
                }

            }
        }

        // Update player resources on all clients
        [ClientRpc]
        void updatePlayerResourcesClientRpc(bool force = false)
        {
            List<int[]> resources = hexManager.getPlayerResources(getDieValue(), force);
        
            for (int i = 0; i < getNumPlayers(); i++)
            {
                //Debug.Log("New P" + i + " values:");
                for (int resource_idx = 0; resource_idx < CatanGame.NUM_RESOURCES; resource_idx++)
                {
                    players[i].resourceCounts[resource_idx] += resources[i][resource_idx];
                    //Debug.Log(resourceNames[resource_idx] + ": " + players[i].resourceCounts[resource_idx]);
                }
            }

        }

        // =========== Player State Methods
        public bool getPlayerBuilding()
        {
            return playerBuilding;
        }

        // =========== Turn Methods
        public int getTurn()
        {
            return turn;
        }
        public bool thisClientTurn()
        {
            return playerActive;
        }

        // =========== Dice Methods
        void rollDie()
        {
            dieValue[0] = Random.Range(1, 7);
            dieValue[1] = Random.Range(1, 7);
            dieValue[2] = dieValue[0] + dieValue[1];
            updateClientDiceClientRpc(dieValue);
        }

        [ClientRpc]
        void updateClientDiceClientRpc(int[] dieValue)
        {
            this.dieValue = dieValue;
        }
        int getDieValue()
        {
            return dieValue[2];
        }

        // =========== Game State Methods
        bool gameInSetupState()
        {
            return gameState < GameState.Active;
        }
        bool gameIsActive()
        {
            return gameState == GameState.Active;
        }

        // =========== RPC Methods for Players Building Roads, Settlements and Cities
        [ServerRpc(RequireOwnership = false)]
        public void setBoardResponseServerRpc()
        {
            setBoardResponseClientRpc();
        }
        [ClientRpc]
        public void setBoardResponseClientRpc()
        {
            responseFromBoard = true;
        }
        [ServerRpc(RequireOwnership = false)]
        public void setPlayerBuiltRoadServerRpc(int edgeId, int owner)
        {
            setPlayerBuiltRoadClientRpc(edgeId, owner);
        }
        [ClientRpc]
        public void setPlayerBuiltRoadClientRpc(int edgeId, int owner)
        {
            hexManager.edgeArray[edgeId].setOwner(owner);
            hexManager.edgeArray[edgeId].setRoadPlaced();
            
            // only decrement resources if game is out of the setup phase
            if (gameIsActive()) players[owner].buildRoad();
        }
        [ServerRpc(RequireOwnership = false)]
        public void setPlayerBuiltSettlementServerRpc(int pointId,int owner)
        {
            setPlayerBuiltSettlementClientRpc(pointId, owner);
        }
        [ClientRpc]
        public void setPlayerBuiltSettlementClientRpc(int pointId, int owner)
        {
            hexManager.pointArray[pointId].setOwner(owner);
            hexManager.pointArray[pointId].setSettlementPlaced();

            // only decrement resources if game is out of the setup phase
            if (gameIsActive()) players[owner].buildSettlement();
        }
        [ServerRpc(RequireOwnership = false)]
        public void setPlayerBuiltCityServerRpc(int pointId, int owner)
        {
            setPlayerBuiltCityClientRpc(pointId,owner);
        }
        [ClientRpc]
        public void setPlayerBuiltCityClientRpc(int pointId, int owner)
        {
            hexManager.pointArray[pointId].setIsCity(true);
            players[owner].buildCity();
        }

        // only register the trade button click if the current client is active
        void tradeBtnClick()
        {
            if (thisClientTurn())
            {
                if (!tradeModeActive)
                {
                    enableTradeView();
                }
                else
                {
                    enableMainView();
                }
            }
        }

        void enableTradeView()
        {
            // disable the main camera and canvas
            mainCamera.gameObject.SetActive(false);
            mainCanvas.gameObject.SetActive(false);

            // disable the port trade camera and canvas
            portTradeMenuCamera.gameObject.SetActive(false);
            portTradeMenuCanvas.gameObject.SetActive(false);

            // enable the trade menu camera and canvas
            tradeMenuCamera.gameObject.SetActive(true);
            tradeMenuCanvas.gameObject.SetActive(true);

            // enable trade mode for the current player 
            tradeModeActive = true;

            // configure drop down menu for the players that can be traded with (do not include current player)
            tradePlayerSelection.ClearOptions();
            List<Dropdown.OptionData> optionsList = new List<Dropdown.OptionData>();
            for (int i = 0; i < getNumPlayers(); i++)
            {
                if(i != turn)
                {
                    optionsList.Add(new Dropdown.OptionData(players[i].name));
                }
            }
            tradePlayerSelection.AddOptions(optionsList);

            // update the trade menu play resource counts
            updateTradeMenuPlayerInfoText();

            // configure text for the trade offer status
            tradeOfferStatusText.text = "Trade Offer Pending...";

        }

        void enablePortTradeView(int portId)
        {
            // disable the main camera and canvas
            mainCamera.gameObject.SetActive(false);
            mainCanvas.gameObject.SetActive(false);

            // disable the trade menu camera and canvas
            tradeMenuCamera.gameObject.SetActive(false);
            tradeMenuCanvas.gameObject.SetActive(false);

            // enable the port trade menu camera and canvas
            portTradeMenuCamera.gameObject.SetActive(true);
            portTradeMenuCanvas.gameObject.SetActive(true);

            // enable trade mode for the current player 
            tradeModeActive = true;

            selectedPort = hexManager.ports[portId];

            // configure if the player can use the drop box to trade any resource
            portTradeOfferSelection.gameObject.SetActive(selectedPort.canTradeAny());

            // configure the title for the type of port
            var player_info_gui_text = GameObject.Find("PortTradeTypeText");
            if (selectedPort.canTradeAny())
            {
                player_info_gui_text.GetComponent<Text>().text = "Port Type: Random";
            }
            else
            {
                player_info_gui_text.GetComponent<Text>().text = "Port Type: "+resourceNames[selectedPort.getResource()];
            }

            // update the trade menu play resource counts
            updatePortTradeMenuPlayerInfoText();

            //var portTradeOfferStatusText = GameObject.Find("PortTradeOfferStatusText");
            portTradeOfferStatusText.GetComponent<Text>().text = "Waiting for trade request...";
        }

        public void portClick(int portId)
        {
            if (thisClientTurn())
            {
                if (!tradeModeActive)
                {
                    enablePortTradeView(portId);
                }
            }
        }

        public void updateTradeMenuPlayerInfoText()
        {
            // player info text
            var player_info_gui_text = GameObject.Find("TradePlayerInfoText");
            player_info_gui_text.GetComponent<Text>().text = "";
            player_info_gui_text.GetComponent<Text>().text += players[turn].name + "'s " + "Resources:\n";
            for (int i = 0; i < resourceNames.Length; i++)
            {
                player_info_gui_text.GetComponent<Text>().text += resourceNames[i] + ": " + players[turn].resourceCounts[i] + "\n";
            }
        }

        public void updatePortTradeMenuPlayerInfoText()
        {
            // player info text
            var player_info_gui_text = GameObject.Find("PortTradePlayerInfoText");
            player_info_gui_text.GetComponent<Text>().text = "";
            player_info_gui_text.GetComponent<Text>().text += players[turn].name + "'s " + "Resources:\n";
            for (int i = 0; i < resourceNames.Length; i++)
            {
                player_info_gui_text.GetComponent<Text>().text += resourceNames[i] + ": " + players[turn].resourceCounts[i] + "\n";
            }
        }

        void enableMainView()
        {
            tradeMenuCamera.gameObject.SetActive(false);
            tradeMenuCanvas.gameObject.SetActive(false);

            portTradeMenuCamera.gameObject.SetActive(false);
            portTradeMenuCanvas.gameObject.SetActive(false);

            mainCamera.gameObject.SetActive(true);
            mainCanvas.gameObject.SetActive(true);

            tradeModeActive = false;

            updateMainGUIServerRpc();
        }

        void tradeRequestBtnClick()
        {
            if (clientPlayer.hasResource(tradeOfferSelection.value))
            {
                //Player p0 = players[turn];

                int tradePlayerId = tradePlayerSelection.value;


                // need to offset the player id's with the selection number in the drop down
                tradePlayerId += (tradePlayerId >= turn) ? 1 : 0;


                int p0_resource = tradeOfferSelection.value;
                int p1_resource = tradeRequestSelection.value;

                // send request to other player
                tradeOfferStatusText.text = "Waiting for Player" + (tradePlayerId + 1) + "'s response...";

                disableTradeMenuButtons();

                proposeTradeOfferServerRpc(turn, tradePlayerId, p0_resource, p1_resource);

                //Debug.Log("Performing Trade:" + turn + "; " + tradePlayerId + "; " + p0_resource + "; " + p1_resource);
            }
            else
            {
                tradeOfferStatusText.text = "You don't have any " + resourceNames[tradeOfferSelection.value] + " to trade!";
            }


        }

        void portTradeRequestBtnClick()
        {
            if (selectedPort.canTradeAny() && clientPlayer.canTradeAtRandomPort() && clientPlayer.resourceCounts[portTradeOfferSelection.value] > 2)
            {
                // execute trade
                int resourceOffered = portTradeOfferSelection.value;
                int resourceRequested = portTradeRequestSelection.value;
                executePortTradeServerRpc((int) getClientId(), resourceOffered, 3, resourceRequested);
            }
            else if (!selectedPort.canTradeAny() && clientPlayer.resourceCounts[selectedPort.getResource()] > 1)
            {
                // execute trade
                int resourceOffered = selectedPort.getResource();
                int resourceRequested = portTradeRequestSelection.value;
                executePortTradeServerRpc((int) getClientId(), resourceOffered, 2, resourceRequested);
            }
            else
            {
                //var portTradeOfferStatusText = GameObject.Find("PortTradeOfferStatusText");
                if (selectedPort.canTradeAny())
                {
                    portTradeOfferStatusText.GetComponent<Text>().text = "You do not have enough " + resourceNames[portTradeOfferSelection.value] + " to make the trade.";
                }
                else
                {
                    portTradeOfferStatusText.GetComponent<Text>().text = "You do not have enough " + resourceNames[selectedPort.getResource()] + " to trade at this port.";
                }
            }
        }
        [ServerRpc(RequireOwnership = false)]
        public void executePortTradeServerRpc(int p0, int p0_resource, int amountOffered, int p1_resource)
        {
            executePortTradeClientRpc(p0, p0_resource, amountOffered, p1_resource);
            updateMainGUIClientRpc();
        }
        [ClientRpc]
        public void executePortTradeClientRpc(int p0, int p0_resource, int amountOffered, int p1_resource)
        {
            players[p0].resourceCounts[p0_resource] -= amountOffered;
            players[p0].resourceCounts[p1_resource]++;

            if (getClientId() == (ulong)p0)
            {
                updatePortTradeMenuPlayerInfoText();
                portTradeOfferStatusText.text = "Trade Completed!";
            }
        }
        public void enableTradeMenuButtons()
        {
            tradeRequestButton.gameObject.SetActive(true);
            cancelTradeButton.gameObject.SetActive(true);
        }
        public void disableTradeMenuButtons()
        {
            tradeRequestButton.gameObject.SetActive(false);
            cancelTradeButton.gameObject.SetActive(false);
        }

        [ServerRpc(RequireOwnership = false)]
        void proposeTradeOfferServerRpc(int proposingPlayer, int targetPlayer, int resourceOffered, int resourceRequested)
        {
            proposeTradeOfferClientRpc(proposingPlayer,targetPlayer,resourceOffered,resourceRequested);
        }

        [ClientRpc]
        void proposeTradeOfferClientRpc(int proposingPlayer, int targetPlayer, int resourceOffered, int resourceRequested)
        {
            if(getClientId() == (ulong)targetPlayer)
            {
                setIncomingTradeWindow(proposingPlayer, targetPlayer, resourceOffered, resourceRequested);
            }
        }
        public void setIncomingTradeWindow(int proposingPlayer, int targetPlayer, int resourceOffered, int resourceRequested)
        {
            tradeOffer = new TradeOffer(proposingPlayer, targetPlayer, resourceOffered, resourceRequested);
            incomingTradeWindow.gameObject.SetActive(true);
            incomingTradeText.text = "Player " + tradeOffer.getP0() + " is offering a trade!\n";
            incomingTradeText.text += "Player " + tradeOffer.getP0() + " will give: "+resourceNames[tradeOffer.getP0Resource()] +"\n";
            incomingTradeText.text += "Player " + tradeOffer.getP1() + " (You) will give: "+resourceNames[tradeOffer.getP1Resource()] + "\n";
        }

        public void acceptTradeBtnClick()
        {
            if (clientPlayer.hasResource(tradeOffer.getP1Resource()))
            {
                executeTradeServerRpc(tradeOffer.getP0(), tradeOffer.getP1(), tradeOffer.getP0Resource(), tradeOffer.getP1Resource());
                clearIncomingTradeWindow();
            }
            else
            {
                incomingTradeText.text = "Player " + tradeOffer.getP0() + " is offering a trade!\n";
                incomingTradeText.text += "Player " + tradeOffer.getP0() + " will give: " + resourceNames[tradeOffer.getP0Resource()] + "\n";
                incomingTradeText.text += "Player " + tradeOffer.getP1() + " (You) will give: " + resourceNames[tradeOffer.getP1Resource()] + "\n";
                incomingTradeText.text += "You don't have enough " + resourceNames[tradeOffer.getP1Resource()] + " to trade!\n";
            }
            
        }

        public void rejectTradeBtnClick()
        {
            rejectTradeServerRpc(tradeOffer.getP0());
            clearIncomingTradeWindow();
        }

        [ServerRpc(RequireOwnership = false)]
        public void executeTradeServerRpc(int p0, int p1, int p0_resource, int p1_resource)
        {
            executeTradeClientRpc(p0,p1,p0_resource,p1_resource);
            updateMainGUIClientRpc();
        }
        [ClientRpc]
        public void executeTradeClientRpc(int p0, int p1, int p0_resource, int p1_resource)
        {
            players[p0].resourceCounts[p0_resource]--;
            players[p0].resourceCounts[p1_resource]++;
            players[p1].resourceCounts[p1_resource]--;
            players[p1].resourceCounts[p0_resource]++;

            if (getClientId() == (ulong)p0)
            {
                enableTradeMenuButtons();
                updateTradeMenuPlayerInfoText();
                tradeOfferStatusText.text = "Trade Completed!";
            }
        }
        [ServerRpc(RequireOwnership = false)]
        public void rejectTradeServerRpc(int p0)
        {
            rejectTradeClientRpc(p0);
        }
        [ClientRpc]
        public void rejectTradeClientRpc(int p0)
        {
            if(getClientId() == (ulong) p0)
            {
                enableTradeMenuButtons();
                tradeOfferStatusText.text = "Trade Declined!";
            }
        }


        public void clearIncomingTradeWindow()
        {
            incomingTradeWindow.gameObject.SetActive(false);
        }

        public bool isServer()
        {
            return NetworkManager.Singleton.IsServer;
        }

        public ulong getClientId()
        {
            return NetworkManager.Singleton.LocalClientId;
        }

        [ClientRpc]
        public void setPlayerActiveClientRpc(int player)
        {
            playerActive = (NetworkManager.Singleton.LocalClientId == (ulong) player);
        }
        [ClientRpc]
        public void setSettlementPlacementModeClientRpc(int turn,bool setup)
        {
            if (playerActive) hexManager.setSettlementPlacementMode(turn, setup);
        }
        [ClientRpc]
        public void setRoadPlacementModeClientRpc(int turn)
        {
            if (playerActive) hexManager.setRoadPlacementMode(turn);
        }
        [ClientRpc]
        public void clearPlacementModeClientRpc()
        {
            hexManager.clearPlacementMode();
        }
        [ClientRpc]
        public void setupPlayerCountsClientRpc()
        {
            for (int i = 0; i < getNumPlayers(); i++)
            {
                players[i].numSettlements = 2;
                players[i].numRoads = 2;
            }
        }

        [ClientRpc]
        public void setGameStateClientRpc(GameState gameState)
        {
            this.gameState = gameState;
        }

        public int getNumPlayers()
        {
            return numPlayers;
        }

        
    }

    class TradeOffer
    {
        int p0;
        int p1;
        int p0_resource;
        int p1_resource;

        public TradeOffer(int p0, int p1, int p0_resource, int p1_resource)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p0_resource = p0_resource;
            this.p1_resource = p1_resource;
        }

        public int getP0() { return p0; }
        public int getP1() { return p1; }
        public int getP0Resource() { return p0_resource; }
        public int getP1Resource() { return p1_resource; }
    }
}


