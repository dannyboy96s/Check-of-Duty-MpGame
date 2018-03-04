using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;


public class ServerClient
{
    public string clientName;
    public bool isHost;
    public TcpClient tcp;
   

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}

public class Server : MonoBehaviour
{
    //public int _port = 6321;
    [SerializeField]
    public int _port = 5935;
    private TcpListener _server;
    [SerializeField]
    private bool _serverHasStarted;
    private List<ServerClient> _disconnectList;
    private List<ServerClient> _clientsList;


    public void Init()
    {
        DontDestroyOnLoad(gameObject);

        _clientsList = new List<ServerClient>();
        _disconnectList = new List<ServerClient>();

        try
        {
            _server = new TcpListener(IPAddress.Any, _port);
            _server.Start();

            startListening();
            _serverHasStarted = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error ocurred: " + e.Message);
        }
    }

    private void startListening()
    {
        _server.BeginAcceptTcpClient(acceptTcpClient, _server);
    }

    private void Update()
    {
        if (!_serverHasStarted)
        {
            return;
        }

        foreach (ServerClient sc in _clientsList)
        {
            //check if the client is still connected
            if (!isConnected(sc.tcp))
            {
                sc.tcp.Close();
                _disconnectList.Add(sc);
                continue;
            }
            else
            {
                NetworkStream ns = sc.tcp.GetStream();
                if (ns.DataAvailable)
                {
                    StreamReader reader = new StreamReader(ns, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        onIncomingData(sc, data);
                    }
                }
            }
        }

        //notify that someone has disconnected
        for (int i = 0; i < _disconnectList.Count - 1; i++)
        {
            Debug.Log("Someone has left the match");
            _clientsList.Remove(_disconnectList[i]);
            _disconnectList.RemoveAt(i);
        }
    }


    private bool isConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }

                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    private void acceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        string allUsers = "";
        foreach (ServerClient iter in _clientsList)
        {
            allUsers += iter.clientName + '|';
        }

        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        _clientsList.Add(sc);

        startListening();

        broadcastInfo("SERVERWHO|" + allUsers, _clientsList[_clientsList.Count - 1]);
    }


    private void broadcastInfo(string data, ServerClient c)
    {
        List<ServerClient> sc = new List<ServerClient> { c };
        broadcastInfo(data, sc);
    }

    // Server Send
    private void broadcastInfo(string data, List<ServerClient> cl)
    {
        foreach (ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e.Message);
            }
        }
    }


    //server read
    private void onIncomingData(ServerClient sc, string data)
    {
        Debug.Log("Server:" + data);

        string[] msg = data.Split('|');

        switch (msg[0])
        {
            case "CLIENTWHO":
                sc.clientName = msg[1];
                sc.isHost = (msg[2] == "0") ? false : true;
                broadcastInfo("SERVERCNN|" + sc.clientName, _clientsList);
                break;

            case "CLIENTMOVE":
                broadcastInfo("SERVERMOVE|" + msg[1] + "|" + msg[2] + "|" + msg[3] + "|" + msg[4], _clientsList);
                break;

            case "CLIENTMESSAGE":
                broadcastInfo("SERVERMESSAGE|" + sc.clientName + " : " + msg[1], _clientsList);
                break;
        }
    }
}


