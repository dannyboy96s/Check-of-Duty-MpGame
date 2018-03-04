using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager _Instance { set; get; }
    [SerializeField]
    public GameObject _serverPrefab;
    [SerializeField]
    public GameObject _clientPrefab;
    [SerializeField]
    public GameObject _mainMenuGo;
    [SerializeField]
    public GameObject _connectMenuGo;
    [SerializeField]
    public GameObject _serverMenuGo;
    [SerializeField]
    public InputField _gamerTagInput;

    private void Start()
    {
        _Instance = this;
        _serverMenuGo.SetActive(false);
        _connectMenuGo.SetActive(false);
        DontDestroyOnLoad(gameObject);
    }

    public void startTheMatch()
    {
        SceneManager.LoadScene("Game");
    }


    public void hostButton()
    {
        try
        {
            Server s = Instantiate(_serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(_clientPrefab).GetComponent<Client>();
            c._clientName = _gamerTagInput.text;
            c._isHost = true;

            if (c._clientName == "")
                c._clientName = "Host";
            c.connectToTheServer("127.0.0.1", 5935);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        _mainMenuGo.SetActive(false);
        _serverMenuGo.SetActive(true);
    }

    public void connectButton()
    {
        _mainMenuGo.SetActive(false);
        _connectMenuGo.SetActive(true);
    }

    public void backButton()
    {
        _mainMenuGo.SetActive(true);
        _serverMenuGo.SetActive(false);
        _connectMenuGo.SetActive(false);

        Server s = FindObjectOfType<Server>();
        if (s != null)
        {
            Destroy(s.gameObject);
        }

        Client c = FindObjectOfType<Client>();
        if (c != null)
        {
            Destroy(c.gameObject);
        }
    }

    public void connectToTheServerButton()
    {
        string hostAddress = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (hostAddress == "")
        {
            hostAddress = "127.0.0.1";
        }

        try
        {
            Client c = Instantiate(_clientPrefab).GetComponent<Client>();
            c._clientName = _gamerTagInput.text;
            if (c._clientName == "")
            {
                c._clientName = "Client";
            }
            c.connectToTheServer(hostAddress, 5935);
            _connectMenuGo.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
