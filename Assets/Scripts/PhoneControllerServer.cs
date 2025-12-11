using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System;

public class PhoneControllerServer : MonoBehaviour
{
    public static PhoneControllerServer instanceMain;
    private HttpListener http;
    private WebSocketServer ws;

    private static PlayerConn instance;
    private static bool gameStarted = false;
    public Snake snake1;
    public Snake2 snake2;
    public Snake3 snake3;
    public Snake4 snake4;
    public Snake5 snake5;
    public Snake6 snake6;

    private class Player
    {
        public string id;
        public int seat;
        public string name;
        public string color;
        public bool ready;
    }

    private static Dictionary<string, Player> players = new();
    private static HashSet<int> takenSeats = new();

    private static readonly string[] playerNames =
        { "Eminem", "Adele", "Ahmed", "Beyonce", "Frank Sinatra", "Lady Gaga" };

    private static readonly string[] playerColors =
        { "#FF7D00", "#00FF00", "#FF0019", "#0022FF", "#FF00CD", "#00FFE1" };

    void Start()
    {
        instanceMain = this;
        StartHTTPServer();
        StartWSServer();
        Debug.Log("Phone controller system ready.");
    }

    void OnApplicationQuit()
    {
        http?.Stop();
        ws?.Stop();
    }

    private void StartHTTPServer()
    {
        http = new HttpListener();
        http.Prefixes.Add("http://*:8080/");
        http.Start();
        http.BeginGetContext(OnHttpRequest, null);
        Debug.Log("HTTP server running at http://<yourip>:8080");
    }

    private void OnHttpRequest(IAsyncResult result)
    {
        if (!http.IsListening) return;

        var context = http.EndGetContext(result);
        http.BeginGetContext(OnHttpRequest, null);

        string path = context.Request.Url.AbsolutePath;
        if (path == "/") path = "/index.html";

        string fullPath = Application.dataPath + "/WebUI" + path;

        if (File.Exists(fullPath))
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
        else
        {
            context.Response.StatusCode = 404;
        }

        context.Response.Close();
    }

    private void StartWSServer()
    {
        ws = new WebSocketServer("ws://0.0.0.0:8081");
        ws.AddWebSocketService<PlayerConn>("/ws");
        ws.Start();

        Debug.Log("WebSocket server running at ws://<yourip>:8081/ws");
    }

    public class PlayerConn : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            instance = this;

            Send("{\"type\":\"id\",\"id\":\"" + ID + "\"}");

            Debug.Log($"New connection: {ID}");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = JsonUtility.FromJson<ClientMsg>(e.Data);

            switch (msg.action)
            {
                case "join":
                    if (!gameStarted)
                        AssignSeat(ID);
                    else
                        Send("{\"type\":\"waiting\"}");
                    break;

                case "ready":
                    if (!gameStarted)
                    {
                        players[ID].ready = true;
                        BroadcastState();

                        bool allReady = true;
                        foreach (var p in players.Values)
                        {
                            if (!p.ready)
                            {
                                allReady = false;
                                break;
                            }
                        }

                        if (allReady)
                        {
                            gameStarted = true;
                            Debug.Log("GAME STARTED");
                        }
                    }
                    break;

                case "input":
                    if (players.ContainsKey(ID))
                    {
                        int seat = players[ID].seat;

                        if (seat == 1 && PhoneControllerServer.instanceMain.snake1 != null)
                            PhoneControllerServer.instanceMain.snake1.SetDirection(msg.value);
                        else if (seat == 2 && PhoneControllerServer.instanceMain.snake2 != null)
                            PhoneControllerServer.instanceMain.snake2.SetDirection(msg.value);
                        else if (seat == 3 && PhoneControllerServer.instanceMain.snake3 != null)
                            PhoneControllerServer.instanceMain.snake3.SetDirection(msg.value);
                        else if (seat == 4 && PhoneControllerServer.instanceMain.snake4 != null)
                            PhoneControllerServer.instanceMain.snake4.SetDirection(msg.value);
                        else if (seat == 5 && PhoneControllerServer.instanceMain.snake5 != null)
                            PhoneControllerServer.instanceMain.snake5.SetDirection(msg.value);
                        else if (seat == 6 && PhoneControllerServer.instanceMain.snake6 != null)
                            PhoneControllerServer.instanceMain.snake6.SetDirection(msg.value);
                    }
                    break;
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (players.ContainsKey(ID))
            {
                takenSeats.Remove(players[ID].seat);
                players.Remove(ID);
                BroadcastState();
            }

            ResetGameIfEmpty();
        }

        private static void AssignSeat(string ID)
        {
            int seat = 1;
            while (takenSeats.Contains(seat)) seat++;

            takenSeats.Add(seat);

            players[ID] = new Player
            {
                id = ID,
                seat = seat,
                name = playerNames[seat - 1],
                color = playerColors[seat - 1],
                ready = false
            };

            BroadcastState();
        }

        private static void ResetGameIfEmpty()
        {
            if (players.Count == 0 && gameStarted)
            {
                takenSeats.Clear();
                gameStarted = false;

                if (instance != null)
                    instance.Sessions.Broadcast("{\"type\":\"reset\"}");

                Debug.Log("All players left — game reset.");
            }
        }

        private static void BroadcastState()
        {
            if (gameStarted) return;

            List<PlayerData> list = new();
            foreach (var p in players.Values)
            {
                list.Add(new PlayerData
                {
                    id = p.id,
                    seat = p.seat,
                    name = p.name,
                    color = p.color,
                    ready = p.ready
                });
            }

            var wrapper = new PlayerWrapper { type = "state", players = list.ToArray() };
            string json = JsonUtility.ToJson(wrapper);

            if (instance != null)
                instance.Sessions.Broadcast(json);
        }
    }

    [System.Serializable]
    public class ClientMsg { public string action; public string value; }

    [System.Serializable]
    public class PlayerData
    {
        public string id;
        public int seat;
        public string name;
        public string color;
        public bool ready;
    }

    [System.Serializable]
    public class PlayerWrapper
    {
        public string type;
        public PlayerData[] players;
    }
}
