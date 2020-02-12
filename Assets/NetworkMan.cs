using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    private GameObject playerGO;

    public string myAddress;
    public Dictionary<string,GameObject> currentPlayers;
    public List<string> newPlayers, droppedPlayers;
    public GameState lastestGameState;
    public ListOfPlayers initialSetofPlayers;
    public MessageType latestMessage;
    public ListOfPlayers playerList;

    // Start is called before the first frame update
    void Start()
    {
        playerGO = Resources.Load("Player") as GameObject;
        newPlayers = new List<string>();
        droppedPlayers = new List<string>();
        currentPlayers = new Dictionary<string, GameObject>();
        initialSetofPlayers = new ListOfPlayers();
        playerList = new ListOfPlayers();

        udp = new UdpClient();
        udp.Connect("3.19.218.14", 12345);
        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
        udp.Send(sendBytes, sendBytes.Length);
        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy(){
        udp.Dispose();
    }

    [Serializable]
    public struct receivedColor{
        public float R;
        public float G;
        public float B;
    }
    [Serializable]
    public struct receivedPosition
    {
        public float X;
        public float Y;
        public float Z;
    }

    [Serializable]
    public class Player{
        public string id;
        public receivedColor color;
        public receivedPosition position;
    }

    [Serializable]
    public class ListOfPlayers{
        public Player[] players;

        public ListOfPlayers(){
            players = new Player[0];
        }
    }
    [Serializable]
    public class ListOfDroppedPlayers{
        public string[] droppedPlayers;
    }
    [Serializable]
    public class GameState
    {
        public int pktID;
        public Player[] players;
    }

    [Serializable]
    public class MessageType{
        public commands cmd;
    }
    public enum commands{
        PLAYER_CONNECTED,
        GAME_UPDATE,
        PLAYER_DISCONNECTED,
        CONNECTION_APPROVED,
        LIST_OF_PLAYERS,
    };
    
    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        // Debug.Log("Got this: " + returnData);
        
        latestMessage = JsonUtility.FromJson<MessageType>(returnData);
        
        Debug.Log(returnData);
        try{
            switch(latestMessage.cmd){
                case commands.PLAYER_CONNECTED:
                    playerList = JsonUtility.FromJson<ListOfPlayers>(returnData);
                    Debug.Log(returnData);
                    foreach (Player player in playerList.players){
                        newPlayers.Add(player.id);
                    }
                    break;
                case commands.GAME_UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
                case commands.PLAYER_DISCONNECTED:
                    Debug.Log(returnData);
                    ListOfDroppedPlayers latestDroppedPlayer = JsonUtility.FromJson<ListOfDroppedPlayers>(returnData);
                    foreach (string player in latestDroppedPlayer.droppedPlayers){
                        droppedPlayers.Add(player);
                    }
                    break;
                case commands.CONNECTION_APPROVED:
                    playerList = JsonUtility.FromJson<ListOfPlayers>(returnData);
                    Debug.Log(returnData);
                    foreach (Player player in playerList.players){
                        newPlayers.Add(player.id);
                        myAddress = player.id;
                    }
                    break;
                case commands.LIST_OF_PLAYERS:
                    Debug.Log(returnData);
                    initialSetofPlayers = JsonUtility.FromJson<ListOfPlayers>(returnData);
                    break; 
                default:
                    Debug.Log("Error: " + returnData);
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(){

        if (newPlayers.Count > 0)
        {
            foreach (Player player in playerList.players)
            {
                if (!currentPlayers.ContainsKey(player.id))
                {
                    currentPlayers.Add(player.id, Instantiate(playerGO, new Vector3(player.position.X, player.position.Y, 0), Quaternion.identity));
                    currentPlayers[player.id].GetComponent<Renderer>().material.color = new Color(player.color.R, player.color.G, player.color.B);
                    currentPlayers[player.id].transform.position = new Vector3(player.position.X, player.position.Y, 0);
                    currentPlayers[player.id].name = player.id;
                }
            }
            newPlayers.Clear();
        }
        foreach (Player player in initialSetofPlayers.players)
        {
            if (!currentPlayers.ContainsKey(player.id))
            {
                currentPlayers.Add(player.id, Instantiate(playerGO, new Vector3(player.position.X, player.position.Y, 0), Quaternion.identity));
                currentPlayers[player.id].GetComponent<Renderer>().material.color = new Color(player.color.R, player.color.G, player.color.B);
                currentPlayers[player.id].transform.position = new Vector3(player.position.X, player.position.Y, 0);
                currentPlayers[player.id].name = player.id;
            }
            initialSetofPlayers = new ListOfPlayers();
        }
        //if (newPlayers.Count > 0)
        //{
        //    foreach (Player player in lastestGameState.players)
        //    {
        //        if (currentPlayers.ContainsKey(player.id))
        //            continue;
        //            currentPlayers.Add(player.id, Instantiate(playerGO, new Vector3(0, 0, 0), Quaternion.identity));
        //            currentPlayers[player.id].GetComponent<Renderer>().material.color = new Color(player.color.R, player.color.G, player.color.B);
        //            currentPlayers[player.id].transform.position = new Vector3(player.position.X, player.position.Y, player.position.Z);
        //            currentPlayers[player.id].name = player.id;

        //    }
        //    newPlayers.Clear();
        //}

        //if (initialSetofPlayers.players.Length > 0)
        //{
        //    foreach (Player player in initialSetofPlayers.players)
        //    {
        //        if (player.id == myAddress)
        //            continue;
        //        currentPlayers.Add(player.id, Instantiate(playerGO, new Vector3(0, 0, 0), Quaternion.identity));
        //        currentPlayers[player.id].GetComponent<Renderer>().material.color = new Color(player.color.R, player.color.G, player.color.B);
        //        currentPlayers[player.id].transform.position = new Vector3(player.position.X, player.position.Y, player.position.Z);
        //        currentPlayers[player.id].name = player.id;
        //    }
        //    initialSetofPlayers = new ListOfPlayers();
        //}

    }

    void UpdatePlayers(){
        if (currentPlayers.Count>0){
            foreach (Player player in lastestGameState.players){
                //currentPlayers[player.id].GetComponent<Renderer>().material.color = new Color(player.color.R, player.color.G, player.color.B);
                Vector3 pos = new Vector3(player.position.X, player.position.Y, player.position.Z);
                currentPlayers[player.id].transform.position = pos;
                //Debug.Log(currentPlayers);
            }
            lastestGameState.players = new Player[0];
        }
    }

    void SendPosition()
    {
        if(currentPlayers.ContainsKey(myAddress))
        {
            string sendPosition = "sendPosition:" + myAddress+currentPlayers[myAddress].GetComponent<CubeController>().position;
            Debug.Log(sendPosition);
            Byte[] sendBytes = Encoding.ASCII.GetBytes(sendPosition);
            udp.Send(sendBytes, sendBytes.Length);
        }
    }

    void DestroyPlayers(){
        if (droppedPlayers.Count > 0){
            foreach (string playerID in droppedPlayers){
                Debug.Log(playerID);
                Debug.Log(currentPlayers[playerID]);
                Destroy(currentPlayers[playerID].gameObject);
                currentPlayers.Remove(playerID);
            }
            droppedPlayers.Clear();
        }
    }
    
    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update(){
        SpawnPlayers();
        UpdatePlayers();
        SendPosition();
        DestroyPlayers();
    }
}