using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Globalization;

public class LoadSceneScript : MonoBehaviour
{
    public GameObject mainScreen, loginScreen, signupScreen, loadScreen;
    public Slider loadSlider;
    public TextMeshProUGUI usernameText, passwordText, regUsernameText, regPassText, loginNotify, signupNotify;
    public Button loginButton, signupButton;
    public PlayerStats playerStats;
    private int loadProgress, lastLoadProgress = 0;
    private bool canSignup, canLogin, canGetStats;
    private string username, password, regUsername, regPass;

    private readonly string loginUrl = "https://outurer.com/login.php";
    private readonly string statsUrl = "https://outurer.com/stats.php";




    void Start() 
    {
        canGetStats = true;
        if(PlayerPrefs.GetString("username").Length > 0) 
        {
            RememberMe();
        }
        else 
        {
            mainScreen.SetActive(true);       
        }
    }

    private void RememberMe() 
    {
        string remName = PlayerPrefs.GetString("username");
        string remPass = PlayerPrefs.GetString("password");
        WWWForm form = new WWWForm();
        form.AddField("username", remName);
        form.AddField("password", remPass);
        form.AddField("action", "login");
        UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
        StartCoroutine(RememberMeWait(w));
    }
    private IEnumerator RememberMeWait(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        if (_w.downloadHandler.text == "Correct")
        {
            LoadGame();
        }
        else 
        {
            mainScreen.SetActive(true);
        }
    }

    public void Login() 
    {
        mainScreen.SetActive(false);
        loginScreen.SetActive(true);
    }
    public void Signup() 
    {
        mainScreen.SetActive(false);
        signupScreen.SetActive(true);
    }
    public void Close() 
    {
        loadScreen.SetActive(false);
        loginScreen.SetActive(false);
        signupScreen.SetActive(false);
        mainScreen.SetActive(true);
    }
    public void LoadGame() 
    {
        mainScreen.SetActive(false);
        loginScreen.SetActive(false);
        signupScreen.SetActive(false);
        loadScreen.SetActive(true);
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(1);
        op.allowSceneActivation = false;
        while (!op.isDone)
        {
            if (op.progress < 0.9f)
            {
                loadProgress = (int)(op.progress * 100f);
            }
            else
            {
                loadProgress = 100;
                GetPlayerStats(op);
            }
            if (lastLoadProgress != loadProgress) { lastLoadProgress = loadProgress; loadSlider.value = loadProgress; }
            yield return null;
        }
        loadProgress = 100;
        loadSlider.value = loadProgress;
    }
    public void GetPlayerStats(AsyncOperation op)
    {
        if (canGetStats) 
        {
            canGetStats = false;
            playerStats.playerName = PlayerPrefs.GetString("username");
            WWWForm form = new WWWForm();
            form.AddField("all", 1);
            form.AddField("username", PlayerPrefs.GetString("username"));
            form.AddField("password", PlayerPrefs.GetString("password"));
            UnityWebRequest w = UnityWebRequest.Post(statsUrl, form);
            StartCoroutine(GetStatsWait(w, op));
        } 
    }

    private IEnumerator GetStatsWait(UnityWebRequest _w, AsyncOperation op)
    {
        yield return _w.SendWebRequest();
        Debug.Log(_w.downloadHandler.text);
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            Debug.Log("Network - Get Stats Success");
            string[] floatData = _w.downloadHandler.text.Split(',');
            string exp = floatData[1];
            string coins = floatData[2];
            string hours = floatData[3];
            string health = floatData[4];
            string water = floatData[5];
            string food = floatData[6];
            string recent = floatData[7];
            if (exp == "") { exp = "0"; }
            if (coins == "") { coins = "50"; }
            if (hours == "") { hours = "0.01"; }
            if (health == "") { health = "100"; }
            if (water == "") { water = "100"; }
            if (food == "") { food = "100"; }
            SaveData(exp,coins,hours,health,water,food,recent);
            op.allowSceneActivation = true;
        }
        else 
        {
            Close();
        }
    }

    private void SaveData(string exp, string coins, string hours, string health, string water, string food, string recent) 
    {
        if (PlayerPrefs.HasKey("recent") && recent != "") 
        {
            CultureInfo global = new CultureInfo("de-DE");
            DateTime cloudDate = DateTime.Parse(recent);
            DateTime localDate = DateTime.Parse(PlayerPrefs.GetString("recent"));
            int result = DateTime.Compare(cloudDate, localDate);

            if (result < 0)
            {
                Debug.Log("Using Cloud Data " + cloudDate.ToString());
                playerStats.playerExp = Convert.ToInt32(exp);
                playerStats.playerCoins = Convert.ToInt32(coins);
                playerStats.playerHours = float.Parse(hours);
                playerStats.playerHealth = Convert.ToInt32(health);
                playerStats.playerWater = Convert.ToInt32(water);
                playerStats.playerFood = Convert.ToInt32(food);

            }
            else
            {
                Debug.Log("Using Local Data " + localDate.ToString());

                playerStats.playerExp = PlayerPrefs.GetInt("exp");
                playerStats.playerCoins = PlayerPrefs.GetInt("coins");
                playerStats.playerHours = PlayerPrefs.GetFloat("hours");
                playerStats.playerHealth = PlayerPrefs.GetInt("health");
                playerStats.playerWater = PlayerPrefs.GetInt("water");
                playerStats.playerFood = PlayerPrefs.GetInt("food");
            }


        }
        else 
        {
            playerStats.playerExp = Convert.ToInt32(exp);
            playerStats.playerCoins = Convert.ToInt32(coins);
            playerStats.playerHours = float.Parse(hours);
            playerStats.playerHealth = Convert.ToInt32(health);
            playerStats.playerWater = Convert.ToInt32(water);
            playerStats.playerFood = Convert.ToInt32(food);
        }
    }


    public void TextChange() 
    {
        if (usernameText.text.Length > 5 && passwordText.text.Length > 5)
        {
            canLogin = true;
            loginButton.interactable = true;
        }
        else 
        {
            canLogin = false;
            loginButton.interactable = false;
        }
        if (regUsernameText.text.Length > 5 && regPassText.text.Length > 5)
        {
            canSignup = true;
            signupButton.interactable = true;
        }
        else 
        {
            signupButton.interactable = false;
            canSignup = false;
        }
    }

    public static string ToMd5(string str)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        byte[] bytes = encoding.GetBytes(str);
        var sha = new System.Security.Cryptography.MD5CryptoServiceProvider();
        return System.BitConverter.ToString(sha.ComputeHash(bytes));
    }

    public void DoLogin()
    {
        if (!canLogin)
            return;

        loginNotify.text = "ENTER DETAILS TO LOG IN";
        username = usernameText.text.ToString();
        password = ToMd5(passwordText.text);

        if (username == "" || password == "")
        {
            loginNotify.text = "COMPLETE ALL FIELDS";
        }
        else
        {

            WWWForm form = new WWWForm();
            form.AddField("username", username);
            form.AddField("password", password);
            form.AddField("action", "login");

            UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
            StartCoroutine(LogIn(w,false));
        }
    }

    public void DoSignUp()
    {
        if (!canSignup) { return; }

        signupNotify.text = "ENTER DETAILS TO SIGN UP";
        regUsername = regUsernameText.text.ToString();
        regPass = ToMd5(regPassText.text);

        if (regUsername == "" || regPass == "") 
        {
            signupNotify.text = "COMPLETE ALL FIELDS";
        }
        else
        {
            WWWForm form = new WWWForm();
            form.AddField("username", regUsername);
            form.AddField("password", regPass);
            form.AddField("action", "signup");
            UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
            StartCoroutine(Register(w));
        }
    }

    private IEnumerator LogIn(UnityWebRequest _w, bool guest)
    {
        Debug.Log(_w.downloadHandler.text);
        yield return _w.SendWebRequest();
        Debug.Log(_w.downloadHandler.text);
        if (_w.downloadHandler.text == "Correct")
        {
            if (!guest) 
            {
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("password", password);
            }
            LoadGame();
        }
        if (_w.downloadHandler.text == "Wrong")
        { 
            if (guest) { PlayerPrefs.DeleteKey("guest-a"); PlayerPrefs.DeleteKey("guest-b"); DoGuest(); }
            else { loginNotify.text = "INCORRECT USERNAME/PASSWORD"; }
        }
        if (_w.downloadHandler.text == "No User")
        {
            if (guest) { PlayerPrefs.DeleteKey("guest-a"); PlayerPrefs.DeleteKey("guest-b"); DoGuest(); }
            else { loginNotify.text = "INCORRECT USERNAME/PASSWORD"; }
        }
    }

    private IEnumerator Register(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        Debug.Log(_w.downloadHandler.text);
        if (_w.downloadHandler.text == "Registered")
        {
            PlayerPrefs.SetString("username", regUsername);
            PlayerPrefs.SetString("password", regPass);

            LoadGame();
        }
        if (_w.downloadHandler.text == "Taken")
        {
            signupNotify.text = "USERNAME TAKEN";
        }
    }

    public void DoGuest()
    {
        if (PlayerPrefs.HasKey("guest-a") && PlayerPrefs.HasKey("guest-b")) 
        {
            username = PlayerPrefs.GetString("guest-a");
            password = PlayerPrefs.GetString("guest-b");
            WWWForm form = new WWWForm();
            form.AddField("username", username);
            form.AddField("password", password);
            form.AddField("action", "login");
            UnityWebRequest w = UnityWebRequest.Post("https://outurer.com/login.php", form);
            StartCoroutine(LogIn(w,true));
        }
        else 
        {
            regUsername = "Guest-" + RandomUserCode(8);
            regPass = RandomUserCode(15);

            PlayerPrefs.SetString("guest-a", regUsername);
            PlayerPrefs.SetString("guest-b", regPass);

            WWWForm form = new WWWForm();
            form.AddField("username", regUsername);
            form.AddField("password", regPass);
            form.AddField("action", "signup");
            UnityWebRequest w = UnityWebRequest.Post("https://outurer.com/login.php", form);
            StartCoroutine(Register(w));
        }        
    }
    public void DoGuestLogin(string guestA, string guestB) 
    {
    
    
    }
    public string RandomUserCode(int value)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[value];
        var random = new System.Random();
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }

}

