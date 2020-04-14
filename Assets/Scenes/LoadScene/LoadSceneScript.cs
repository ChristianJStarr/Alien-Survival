using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;
using System.Globalization;

public class LoadSceneScript : MonoBehaviour
{
    public GameObject mainScreen, loginScreen, signupScreen, loadScreen;
    public Slider loadSlider;
    public TextMeshProUGUI loginNotify, signupNotify;
    public TMP_InputField usernameText, passwordText, regUsernameText, regPassText;
    public Button loginButton, signupButton;
    public PlayerStats playerStats;
    private int userId, loadProgress, lastLoadProgress = 0;
    private bool canSignup, canLogin, canGetStats;
    private string username, password, regUsername, regPass, authKey;

    private readonly string loginUrl = "https://www.game.aliensurvival.com/login.php";
    private readonly string statsUrl = "https://www.game.aliensurvival.com/stats.php";

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
        usernameText.asteriskChar = passwordText.asteriskChar = regUsernameText.asteriskChar = regPassText.asteriskChar = '•';
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
        Debug.Log(_w.downloadHandler.text);
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
            //SUCCESS
            string[] floatData = _w.downloadHandler.text.Split(',');
            string exp = floatData[1];
            string coins = floatData[2];
            string hours = floatData[3];
            string recent = floatData[4];
            if (exp == "") { exp = "0"; }
            if (coins == "") { coins = "50"; }
            if (hours == "") { hours = "0.01"; }
            SaveData(exp,coins,hours,recent);
            op.allowSceneActivation = true;
        }
        else 
        {
            Close();
        }
    }

    private void SaveData(string exp, string coins, string hours, string recent) 
    {
        float hour;
        if (float.Parse(hours) == 0)
        {
            hour = 0.00F;
        }
        else
        {
            hour = float.Parse(hours);
        }
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
                playerStats.playerHours = hour;
            }
            else
            {
                Debug.Log("Using Local Data " + localDate.ToString());
                playerStats.playerExp = PlayerPrefs.GetInt("exp");
                playerStats.playerCoins = PlayerPrefs.GetInt("coins");
                playerStats.playerHours = PlayerPrefs.GetFloat("hours");
            }
        }
        else 
        {
            playerStats.playerExp = Convert.ToInt32(exp);
            playerStats.playerCoins = Convert.ToInt32(coins);
            playerStats.playerHours = hour;
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
        Debug.Log(passwordText.text);
    }

    public static string ToMd5(string str)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        byte[] bytes = encoding.GetBytes(str);
        var sha = new System.Security.Cryptography.MD5CryptoServiceProvider();
        return System.BitConverter.ToString(sha.ComputeHash(bytes));
    }

    public void DoLogIn() 
    {
        StartCoroutine(LogIn());
    }

    private IEnumerator LogIn()
    {
        if (canLogin)
        {
            loginNotify.text = "ENTER DETAILS TO LOG IN";
            username = usernameText.text.ToString();
            password = ToMd5(passwordText.text);
            Debug.Log(username + " " + password);

            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
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
                yield return w.SendWebRequest();
                Debug.Log(w.downloadHandler.text);
                if (w.downloadHandler.text.StartsWith("TRUE"))
                {
                    string[] data = w.downloadHandler.text.Split(',');
                    authKey = data[1];
                    userId = Convert.ToInt32(data[2]);
                    PlayerPrefs.SetString("authKey", authKey);
                    PlayerPrefs.SetInt("userId", userId);
                    PlayerPrefs.SetString("username", username);
                    PlayerPrefs.SetString("password", password);
                    PlayerPrefs.Save();
                    LoadGame();
                }
                if (w.downloadHandler.text == "Wrong")
                {
                    loginNotify.text = "INCORRECT USERNAME/PASSWORD";
                }
                if (w.downloadHandler.text == "No User")
                {
                    loginNotify.text = "INCORRECT USERNAME/PASSWORD";
                }
            }
        }   
    }
    private IEnumerator GuestLogIn(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        Debug.Log(_w.downloadHandler.text);
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] data = _w.downloadHandler.text.Split(',');
            authKey = data[1];
            userId = Convert.ToInt32(data[2]);
            PlayerPrefs.SetString("authKey", authKey);
            PlayerPrefs.SetInt("userId", userId);
            PlayerPrefs.Save();
            LoadGame();
        }
        if (_w.downloadHandler.text == "Wrong")
        {
            PlayerPrefs.DeleteKey("guest-a"); PlayerPrefs.DeleteKey("guest-b"); DoGuest(); 
        }
        if (_w.downloadHandler.text == "No User")
        {
            PlayerPrefs.DeleteKey("guest-a"); PlayerPrefs.DeleteKey("guest-b"); DoGuest();
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
            authKey = RandomUserCode(15);
            WWWForm form = new WWWForm();
            form.AddField("username", regUsername);
            form.AddField("password", regPass);
            form.AddField("authKey", authKey);
            form.AddField("action", "signup");
            UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
            StartCoroutine(Register(w));
        }
    }

    

    private IEnumerator Register(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        Debug.Log(_w.downloadHandler.text);
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] data = _w.downloadHandler.text.Split(',');
            userId = Convert.ToInt32(data[1]);
            PlayerPrefs.SetString("username", regUsername);
            PlayerPrefs.SetString("password", regPass);
            PlayerPrefs.SetString("authKey", authKey);
            PlayerPrefs.SetInt("userId", userId);

            LoadGame();
        }
        if (_w.downloadHandler.text == "Taken")
        {
            signupNotify.text = "USERNAME TAKEN";
        }
        Debug.Log(_w.downloadHandler.text);
        Debug.Log(_w.downloadHandler.text);
        Debug.Log(_w.downloadHandler.text);
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
            UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
            StartCoroutine(GuestLogIn(w));
        }
        else 
        {
            regUsername = "Guest-" + RandomUserCode(8);
            regPass = RandomUserCode(15);
            authKey = RandomUserCode(15);

            PlayerPrefs.SetString("guest-a", regUsername);
            PlayerPrefs.SetString("guest-b", regPass);
            PlayerPrefs.SetString("authKey", authKey);

            
            WWWForm form = new WWWForm();
            form.AddField("username", regUsername);
            form.AddField("password", regPass);
            form.AddField("authKey", authKey);
            form.AddField("action", "signup");
            UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
            StartCoroutine(Register(w));
        }        
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

