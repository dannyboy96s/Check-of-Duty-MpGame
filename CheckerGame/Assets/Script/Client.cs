using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System;
using System.Collections.Generic;

public class GameClient
{
    public string name;
    public bool isHost;
}

public class Client : MonoBehaviour
{
    [SerializeField]
    private bool _socketReady;
    private TcpClient _socket;
    private NetworkStream _stream;
    private StreamWriter _writer;
    private StreamReader _reader;
    [SerializeField]
    public string _clientName;
    [SerializeField]
    public bool _isHost;

    public List<GameClient> _playersList = new List<GameClient>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_socketReady)
        {
            if (_stream.DataAvailable)
            {
                string data = _reader.ReadLine();
                if (data != null)
                {
                    onIncomingData(data);
                }
            }
        }
    }

    public bool connectToTheServer(string host, int port)
    {
        if (_socketReady)
        {
            return false;
        }

        try
        {
            _socket = new TcpClient(host, port);
            _stream = _socket.GetStream();
            _writer = new StreamWriter(_stream);
            _reader = new StreamReader(_stream);

            _socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error " + e.Message);
        }

        return _socketReady;
    }



    //send message to the server
    public void send(string data)
    {
        if (!_socketReady)
        {
            return;
        }

        _writer.WriteLine(data);
        _writer.Flush();
    }

    private void userConnected(string name, bool host)
    {
        GameClient c = new GameClient();
        c.name = name;

        _playersList.Add(c);

        if (_playersList.Count == 2)
        {
            GameManager._Instance.startTheMatch();
        }
    }

    //read  the messages from the server
    private void onIncomingData(string data)
    {
        Debug.Log("Client:" + data);
        string[] msg = data.Split('|');

        switch (msg[0])
        {
            case "SERVERWHO":
                for (int i = 1; i < msg.Length - 1; i++)
                {
                    userConnected(msg[i], false);
                }
                send("CLIENTWHO|" + _clientName + "|" + ((_isHost) ? 1 : 0).ToString());
                break;
            case "SERVERCNN":
                userConnected(msg[1], false);
                break;
            case "SERVERMOVE":
                CheckersBoard._Instance.tryToMoveAPlayingPiece(int.Parse(msg[1]), int.Parse(msg[2]), int.Parse(msg[3]), int.Parse(msg[4]));
                break;

            case "SERVERMESSAGE":
                CheckersBoard._Instance.chatMessage(msg[1]);
                break; 
        }
    }

    private void closeSocket()
    {
        if (!_socketReady)
        {
            return;
        }

        _writer.Close();
        _reader.Close();
        _socket.Close();
        _socketReady = false;
    }

    private void onDisable()
    {
        closeSocket();
    }

    private void onApplicationQuit()
    {
        closeSocket();
    }


}

