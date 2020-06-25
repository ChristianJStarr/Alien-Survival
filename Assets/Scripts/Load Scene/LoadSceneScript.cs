using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadSceneScript : MonoBehaviour
{
    //What: Load Scene Script. Does Login/Signup/Stats & Loading into main menu.
    //Where: The Load Scene.
    public string[] loadTips;    
    public bool devServer = false;
    public GameObject mainScreen, loginScreen, signupScreen, loadScreen, userReporting; //Each screen layer.
    public TextMeshProUGUI loginNotify, signupNotify, loadTip; //Notify text field for login and signup screen.
    public TMP_InputField usernameText, passwordText, regUsernameText, regPassText; //Input fields for login and signup.
    public Button loginButton, signupButton; //Login and signup screen buttons.
    public PlayerStats playerStats; //Player Statistics data object.
    public WebServer webServer; //Web Server Handler.
    private int loadProgress, lastLoadProgress = 0; //Loading bar values.
    private bool singleAttempt = true; //A single attempt for retrying guest login.
    public RawImage background; //Background Image
    public Texture blurTexture; //Blurred Bkg Texture
    public Texture regTexture; //Regular Bkg Texture

    void Start() 
    {
#if UNITY_SERVER
        SceneManager.LoadScene(1);
#endif
#if UNITY_EDITOR
        if (devServer)
        {
            SceneManager.LoadScene(1);
        }
#endif
        Application.targetFrameRate = 30;
        //Check if user has logged in before and has username/pass stored.
        if (PlayerPrefs.GetString("username").Length > 0) 
        {
            LoginRememberMe();//Login with stored credentials.
        }
        else 
        {
            mainScreen.SetActive(true);//Show main menu screen.
        }
        //Change asterisk to dot in input field.
        usernameText.asteriskChar = passwordText.asteriskChar = regUsernameText.asteriskChar = regPassText.asteriskChar = '•';
    }

    //Button Function: Show login menu.
    public void LoginMenu()
    {
        mainScreen.SetActive(false);
        loginScreen.SetActive(true);
    }
    
    //Button Function: Show signup menu.
    public void SignupMenu()
    {
        mainScreen.SetActive(false);
        signupScreen.SetActive(true);
    }
    
    //Button Function: Show main menu.
    public void Close()
    {
        loadScreen.SetActive(false);
        loginScreen.SetActive(false);
        signupScreen.SetActive(false);
        mainScreen.SetActive(true);
        if(background.texture != regTexture) 
        {
            background.texture = regTexture;
        }
    }
    
    //Load MainMenu scene. Called if a login or signup was successful.
    private void LoadGame()
    {
        mainScreen.SetActive(false);
        loginScreen.SetActive(false);
        signupScreen.SetActive(false);
        userReporting.SetActive(false);
        loadScreen.SetActive(true);
        background.texture = blurTexture;
        loadTip.text = GetLoadTip();
        StartCoroutine(LoadRoutine());//Start loading the MainMenu scene.
    }
    
    //Load routine for loading MainMenu scene. Handles the loading bar and getting player stats.
    private IEnumerator LoadRoutine()
    {
        bool loadTime = true;
        AsyncOperation op = SceneManager.LoadSceneAsync(1);
        op.allowSceneActivation = false;
        while (!op.isDone)
        {
            if (op.progress < 0.9f)
            {
                loadProgress = (int)(op.progress * 100f);
            }
            else if(loadTime)
            {
                loadTime = false;
                loadProgress = 100;
                int userId = PlayerPrefs.GetInt("userId");
                string authKey = PlayerPrefs.GetString("authKey");
                webServer.StatRequest(PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), onRequestFinished =>
                {
                    if (onRequestFinished != null)
                    {
                        loadProgress = 75;
                        playerStats = onRequestFinished;
                        op.allowSceneActivation = true;
                    }
                    else 
                    {
                        Debug.Log("Network - Web - Unable to get stats.");
                        Close();
                    }
                });
            }
            if (lastLoadProgress != loadProgress) { lastLoadProgress = loadProgress;  }
            yield return null;
        }
    }

    //Login with stored PlayerPrefs.
    private void LoginRememberMe()
    {
        string username = PlayerPrefs.GetString("username");
        string password = PlayerPrefs.GetString("password");
        
        webServer.LoginRequest(username, password, onRequestFinished => 
        {
            if(onRequestFinished) 
            {
                LoadGame();
            }
            else 
            {
                mainScreen.SetActive(true);
            }
        });
    }

    //Signup with InputField credentials.
    public void SignUp()
    {
        string username = regUsernameText.text.ToString();
        string password = ToMd5(regPassText.text);
        string authKey = RandomUserCode(15);
        webServer.SignupRequest(username, password, authKey, onRequestFinished =>
        {
            if (onRequestFinished)
            {
                LoadGame();//Load the MainMenu scene.
            }
            else
            {
                //Unable to Signup. Likely username taken.
                signupNotify.text = "USERNAME TAKEN";
            }
        });
    }

    //Login with InputField credentials.
    public void LogIn()
    {
        string username = usernameText.text.ToString();
        string password = ToMd5(passwordText.text);
        webServer.LoginRequest(username, password, onRequestFinished =>
        {
            if (onRequestFinished)
            {
                LoadGame();//Load the MainMenu scene.
            }
            else
            {
                //Unable to Login. Likely incorrect fields.
                loginNotify.text = "INCORRECT USERNAME/PASSWORD";
            }
        });
    }

    //Create guest credentials and login.
    public void DoGuest()
    {
        if (PlayerPrefs.HasKey("guest-a") && PlayerPrefs.HasKey("guest-b"))
        {
            string username = PlayerPrefs.GetString("guest-a");
            string password = PlayerPrefs.GetString("guest-b");
            webServer.LoginRequest(username, password, onRequestFinished =>
            {
                if (onRequestFinished)
                {
                    LoadGame();//Load the MainMenu scene.
                }
                else
                {
                    //Signup was unsuccessful.
                    //Guest Credentials are wrong or connection error.
                    //Delete saved guest keys.
                    PlayerPrefs.DeleteKey("guest-a");
                    PlayerPrefs.DeleteKey("guest-b");
                    PlayerPrefs.Save();
                    //DoGuest(); //Start over.
                }
            });
        }
        else
        {
            string random = RandomUserCode(30);
            string username = "Guest-" + random.Substring(1,8); //Create a guest name.
            string password = random.Substring(9, 10); //Create guest password.
            string authKey = random.Substring(19, 10); //Create guest username.
            webServer.SignupRequest(username, password, authKey, onRequestFinished =>
            {
                if (onRequestFinished)
                {
                    LoadGame();//Load the MainMenu scene.
                }
                else
                {
                    ///Signup was unsuccesful.
                    //Guest Credentials are taken(rare) or connection error.
                    PlayerPrefs.DeleteKey("guest-a");
                    PlayerPrefs.DeleteKey("guest-b");
                    PlayerPrefs.Save();
                    //Only allow once.
                    if (singleAttempt)
                    {
                        //DoGuest(); //Try Again.
                        singleAttempt = false;
                    }
                }
            });
        }
    }

    //Text change called from all input fields.
    public void TextChange() 
    {
        if (usernameText.text.Length > 5 && passwordText.text.Length > 5)
        {
            loginButton.interactable = true;
        }
        else 
        {
            loginButton.interactable = false;
        }
        if (regUsernameText.text.Length > 5 && regPassText.text.Length > 5)
        {
            signupButton.interactable = true;
        }
        else 
        {
            signupButton.interactable = false;
        }
    }
    
    //TOOL: Generate random string of characters.
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
    
    //TOOL: Convert password to Md5
    public static string ToMd5(string str)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        byte[] bytes = encoding.GetBytes(str);
        var sha = new System.Security.Cryptography.MD5CryptoServiceProvider();
        return System.BitConverter.ToString(sha.ComputeHash(bytes));
    }

    //Get Load Screen Tip Text
    private string GetLoadTip()
    {
        return loadTips[Random.Range(0, loadTips.Length - 1)];
    }

}

