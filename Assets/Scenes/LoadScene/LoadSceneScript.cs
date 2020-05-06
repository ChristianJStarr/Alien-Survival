using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Globalization;
using TMPro;

public class LoadSceneScript : MonoBehaviour
{
    public GameObject mainScreen, loginScreen, signupScreen, loadScreen; //Each screen layer.
    public Slider loadSlider; //Slider for load screen.
    public TextMeshProUGUI loginNotify, signupNotify; //Notify text field for login and signup screen.
    public TMP_InputField usernameText, passwordText, regUsernameText, regPassText; //Input fields for login and signup.
    public Button loginButton, signupButton; //Login and signup screen buttons.
    public PlayerStats playerStats;
    private int userId, loadProgress, lastLoadProgress = 0;
    private bool canSignup, canLogin, canGetStats;
    private string username, password, regUsername, regPass, authKey;

    private readonly string loginUrl = "https://www.game.aliensurvival.com/login.php"; //Url for login.
    private readonly string statsUrl = "https://www.game.aliensurvival.com/stats.php"; //Url for signup.

    void Start() 
    {
        canGetStats = true;
        //Check if user has logged in before and has username/pass stored.
        if(PlayerPrefs.GetString("username").Length > 0) 
        {
            StartCoroutine(LoginRememberMe());//Login with stored credentials.
        }
        else 
        {
            mainScreen.SetActive(true);//Show main menu screen.
        }
        //Change asterisk to dot in input field.
        usernameText.asteriskChar = passwordText.asteriskChar = regUsernameText.asteriskChar = regPassText.asteriskChar = '•';
    }

    //Show login menu.
    public void Login()
    {
        mainScreen.SetActive(false);
        loginScreen.SetActive(true);
    }
    //Show signup menu.
    public void Signup()
    {
        mainScreen.SetActive(false);
        signupScreen.SetActive(true);
    }
    //Show main menu.
    public void Close()
    {
        loadScreen.SetActive(false);
        loginScreen.SetActive(false);
        signupScreen.SetActive(false);
        mainScreen.SetActive(true);
    }
    //Load MainMenu scene.
    public void LoadGame()
    {
        mainScreen.SetActive(false);
        loginScreen.SetActive(false);
        signupScreen.SetActive(false);
        loadScreen.SetActive(true);
        StartCoroutine(LoadRoutine());//Start loading the MainMenu scene.
    }
    
    //Load routine for loading MainMenu scene.
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
                loadProgress = 75;
                StartCoroutine(GetPlayerStats(op));//Pass over AsyncOp and get player stats. Last 25% of load progress.
            }
            if (lastLoadProgress != loadProgress) { lastLoadProgress = loadProgress; loadSlider.value = loadProgress; }
            yield return null;
        }
        loadSlider.value = loadProgress = 75;
    }

    //Login with stored PlayerPrefs.
    private IEnumerator LoginRememberMe()
    {
        string remName = PlayerPrefs.GetString("username");
        string remPass = PlayerPrefs.GetString("password");
        UnityWebRequest w = MakeLoginRequest(remName, remPass);
        yield return w.SendWebRequest();
        if (w.downloadHandler.text == "Correct")
        {
            LoadGame();//Load MainMenu scene.
        }
        else 
        {
            mainScreen.SetActive(true);
        }
    }

    //Get the player stats from statsUrl.
    private IEnumerator GetPlayerStats(AsyncOperation op)
    {
        if (canGetStats)
        {
            canGetStats = false;
            string remName = PlayerPrefs.GetString("username");
            string remPass = PlayerPrefs.GetString("password");
            playerStats.playerName = remName;
            UnityWebRequest w = MakeStatsRequest(remName, remPass, 1);
            yield return w.SendWebRequest();
            if (w.downloadHandler.text.StartsWith("TRUE"))
            {
                loadSlider.value = 100;
                string[] floatData = w.downloadHandler.text.Split(',');
                string exp = floatData[1];
                string coins = floatData[2];
                string hours = floatData[3];
                string recent = floatData[4];
                if (exp == "") { exp = "0"; }
                if (coins == "") { coins = "50"; }
                if (hours == "") { hours = "0.01"; }
                SaveData(exp, coins, hours, recent);
                op.allowSceneActivation = true;
            }
            else
            {
                canGetStats = true;
                Close();
            }
        }
    }

    //Compare cloud data with local data. Pick newest data source.
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

    //Text change called from all input fields.
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

    //Convert password to Md5
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
            if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
            {
                loginNotify.text = "COMPLETE ALL FIELDS";
            }
            else
            {
                UnityWebRequest w = MakeLoginRequest(username, password);
                yield return w.SendWebRequest();
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
            StartCoroutine(Register(MakeSignupRequest(regUsername, regPass, RandomUserCode(15))));
        }
    }

    

    private IEnumerator Register(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] data = _w.downloadHandler.text.Split(',');
            userId = Convert.ToInt32(data[1]);
            PlayerPrefs.SetString("username", regUsername);
            PlayerPrefs.SetString("password", regPass);
            PlayerPrefs.SetString("authKey", authKey);
            PlayerPrefs.SetInt("userId", userId);
            PlayerPrefs.Save();
            LoadGame();
        }
        if (_w.downloadHandler.text == "Taken")
        {
            signupNotify.text = "USERNAME TAKEN";
        }
    }

    //Create guest credentials and login.
    public void DoGuest()
    {
        if (PlayerPrefs.HasKey("guest-a") && PlayerPrefs.HasKey("guest-b")) 
        {
            username = PlayerPrefs.GetString("guest-a");
            password = PlayerPrefs.GetString("guest-b");
            StartCoroutine(GuestLogIn(MakeLoginRequest(username, password)));
        }
        else 
        {
            regUsername = "Guest-" + RandomUserCode(8);
            regPass = RandomUserCode(15);
            authKey = RandomUserCode(15);
            PlayerPrefs.SetString("guest-a", regUsername);
            PlayerPrefs.SetString("guest-b", regPass);
            PlayerPrefs.SetString("authKey", authKey);
            PlayerPrefs.Save();
            StartCoroutine(Register(MakeSignupRequest(regUsername, regPass, authKey)));
        }        
    }
    
    //Make request for login.
    private UnityWebRequest MakeLoginRequest(string username, string password) 
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("action", "login");
        UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
        return w;
    }

    //Make request for signup.
    private UnityWebRequest MakeSignupRequest(string username, string password, string authKey)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("authKey", authKey);
        form.AddField("action", "signup");
        UnityWebRequest w = UnityWebRequest.Post(loginUrl, form);
        return w;
    }

    //Make request for signup.
    private UnityWebRequest MakeStatsRequest(string username, string password, int type)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);
        form.AddField("all", type);
        UnityWebRequest w = UnityWebRequest.Post(statsUrl, form);
        return w;
    }

    //Generate random string of characters.
    public string RandomUserCode(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        var random = new System.Random();
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }

}

