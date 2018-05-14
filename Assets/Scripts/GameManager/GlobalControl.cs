using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GlobalControl : MonoBehaviour
{
    public static GlobalControl Instance;

    public static int CurrentHealth;
    public static int CurrentMaxHealth;
    public static int CurrentDashes;
    public static int MaxDashes;
    public static bool GrappleEnabled;
    public static float GameTimer;

    public int SetCurrentHealth;
    public int SetCurrentDashes;
    public int SetMaxDashes;
    public bool SetGrappleEnabled;

    private static int _dashesPickedUp;
    private static bool _grapplePickedUp;
    //private static int _currentLevelDashes;

    void Awake()
    {

        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;

            CurrentHealth = SetCurrentHealth;
            CurrentMaxHealth = CurrentHealth;
            CurrentDashes = SetCurrentDashes;
            MaxDashes = SetMaxDashes;
            GrappleEnabled = SetGrappleEnabled;
            GameTimer = 0f;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }else if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public static void RestartCheckpoint()
    {

    }

    public static void Restart()
    {
        CurrentHealth--;


        if (CurrentHealth <= 0)
        {
            CurrentHealth = Instance.SetCurrentHealth;
            CurrentMaxHealth = CurrentHealth;
            CurrentDashes = Instance.SetCurrentDashes;
            //_currentLevelDashes = CurrentDashes;
            _dashesPickedUp = 0;
            _grapplePickedUp = false;
            SceneManager.LoadScene("Hub");

        }
        else
        {
            CurrentDashes -= _dashesPickedUp;
            _dashesPickedUp = 0;
            if (_grapplePickedUp)
                GrappleEnabled = false;
            _grapplePickedUp = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }

    public static void AddDash()
    {
        _dashesPickedUp++;
        CurrentDashes++;
    }

    public static void AddGrapple()
    {
        _grapplePickedUp = true;
        GrappleEnabled = true;
        GameObject.FindWithTag("GUI").GetComponent<HUD_Manager>().GetGrapple(true);
    }

    public static void LoadLevel(string level)
    {
        //_currentLevelDashes = CurrentDashes;
        _dashesPickedUp = 0;
        _grapplePickedUp = false;
        SceneManager.LoadScene(level);
    }
}