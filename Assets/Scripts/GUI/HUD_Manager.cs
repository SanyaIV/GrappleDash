using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour {

    private Text livesUI;
    
    public PlayerController playerCtrl;

    [Header("LifeHUD")]
    //private int previousLife;
    public GameObject uDied;
    public GameObject gameOver;
    public GameObject uWin;

    [Header("ShieldHUD")]
    public Slider shieldEnergy;
    public Image shieldFill;
    public Color shieldFull;
    public Color shieldEmpty;

    [Header("DashHUD")]
    public int MaxDashes;
    public int currentDashes;
    public GameObject dashUpgrade;
    public GameObject[] dash;
    public GameObject[] dashEmpty;

    [Header("GrappleHUD")]
    public GameObject hasGrapple;


    [Header("Crosshair")]
    public RectTransform Up;
    public RectTransform Down;
    public RectTransform Left;
    public RectTransform Right;
    public Vector3 ClosedUp;
    public Vector3 ClosedDown;
    public Vector3 ClosedLeft;
    public Vector3 ClosedRight;
    public bool CrosshairOpen;

    private void Awake()
    {
        playerCtrl = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    void Start(){

        CrosshairOpen = true;

        livesUI = GameObject.Find("LifeCount").GetComponent<Text>();
        //MaxDashes = DashState.MAX_DASH_AMOUNT;
        MaxDashes = GlobalControl.MaxDashes;

        GetGrapple(GlobalControl.GrappleEnabled);

        livesUI.text = "x" + playerCtrl.Life.GetHealth();
        //previousLife = playerCtrl.CurrentLife;
    }

    void Update() {

        currentDashes = playerCtrl.GetState<DashState>().CurrentDashes;
        DashUI();
        LifeUI();
        //ShieldMeter();
        /*if(playerCtrl.CurrentLife < previousLife) {
            DeathUI();
            previousLife = playerCtrl.CurrentLife;
        }*/

    }

    private void DashUI() {

        for(int i = 0; i < MaxDashes; i++)
        {
            dash[i].SetActive(i < currentDashes ? true : false);
            dashEmpty[i].SetActive(i >= currentDashes ? true : false);
        }
    }

    private void LifeUI() {
        livesUI.text = "" + playerCtrl.Life.GetHealth();
    }

    private void DeathUI()
    {
        if (playerCtrl.Life.GetHealth() == 0)
        {
            gameOver.SetActive(true);
        }
        else
        {
            uDied.SetActive(true);
        }
    }

    public void GetGrapple(bool gotGrapple) {

        if(gotGrapple == true) {
            hasGrapple.SetActive(true);
        }else hasGrapple.SetActive(false);

    }

    public void DeathUI(bool state)
    {
        uDied.SetActive(state);
        if(state)
            StartCoroutine(FadeIn(uDied.GetComponentInChildren<Image>()));
    }

    public void GameOverUI(bool state)
    {
        gameOver.SetActive(state);
        if (state)
            StartCoroutine(FadeIn(gameOver.GetComponentInChildren<Image>()));
    }

    public void YouWinUI(bool state)
    {
        uWin.SetActive(state);
        if (state)
            StartCoroutine(FadeIn(uWin.GetComponentInChildren<Image>()));
    }

    public void ToggleCrosshairOpen(bool state)
    {
        if (CrosshairOpen && !state)
            CloseCrosshair();
        if (!CrosshairOpen && state)
            OpenCrosshair();
    }

    private void OpenCrosshair()
    {
        Up.anchoredPosition = Vector3.zero;
        Down.anchoredPosition = Vector3.zero;
        Left.anchoredPosition = Vector3.zero;
        Right.anchoredPosition = Vector3.zero;
        CrosshairOpen = true;
    }

    private void CloseCrosshair()
    {
        if (GlobalControl.Instance != null && GlobalControl.GrappleEnabled)
        {
            Up.anchoredPosition = ClosedUp;
            Down.anchoredPosition = ClosedDown;
            Left.anchoredPosition = ClosedLeft;
            Right.anchoredPosition = ClosedRight;
            CrosshairOpen = false;
        }
    }

    public void ShieldMeter(float newValue) {

        shieldEnergy.value = newValue; //playerCtrl.GetState<ShieldState>().CurrentEnergy;
        shieldFill.color = Color.Lerp(shieldEmpty, shieldFull, shieldEnergy.value / shieldEnergy.maxValue);
    }

    public IEnumerator DashUpgraded() {

        dashUpgrade.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        dashUpgrade.SetActive(false);
    }

    public IEnumerator FadeIn(Image image)
    {
        Color tmp = image.color;
        tmp.a = 0f;

        while(tmp.a < 1)
        {
            tmp.a = Mathf.Lerp(0f, 1f, tmp.a + 1f * Time.deltaTime);
            image.color = tmp;
            yield return null;
        }
        yield break;
    }

}
