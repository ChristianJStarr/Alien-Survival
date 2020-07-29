using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LoadSceneScript : MonoBehaviour
{
    //What: Load Scene Script. Does Login/Signup/Stats & Loading into main menu.
    //Where: The Load Scene.
    public bool devServer = false;
    public GameObject mainScreen, loginScreen, signupScreen, loadScreen, userReporting, terms, termsCheck, connectionError; //Each screen layer.
    public TextMeshProUGUI loginNotify, signupNotify, loadTip, version_1, version_2, version_3; //Notify text field for login and signup screen.
    public TMP_InputField usernameText, passwordText, regUsernameText, regPassText; //Input fields for login and signup.
    public Button loginButton, signupButton, termsButton; //Login and signup screen buttons.
    public PlayerStats playerStats; //Player Statistics data object.
    public WebServer webServer; //Web Server Handler.
    private int loadProgress, lastLoadProgress = 0; //Loading bar values.
    public RawImage background; //Background Image
    public Texture blurTexture; //Blurred Bkg Texture
    public Texture regTexture; //Regular Bkg Texture
    public Texture nightTexture;
    public Texture blurNightTexture;

    private AsyncOperation asyncTemp;
    private string privacyPolicyUrl = "https://aliensurvival.com/privacy-app.php";
    private string termsConditionsUrl = "https://aliensurvival.com/terms-app.php";
    private int nightChance = 0;

    private float storedFontSizeLogin = 0;
    private float storedFontSizeSignup = 0;
    private string storedKeyLogin = "";
    private string storedKeySignup = "";

    private void OnEnable()
    {
        MultiLangSystem.ChangedLanguage += UpdateText;
    }
    private void OnDisable()
    {
        MultiLangSystem.ChangedLanguage -= UpdateText;
    }


    void Start() 
    {
        //Run Server if Server

#if UNITY_SERVER
                RunServer();
#endif
#if UNITY_EDITOR
        if (devServer)
        {
            RunServer();
        }
#endif
        ChangeNotify("signup", "defaultSignup");
        ChangeNotify("login", "defaultLogin");
        //Set Night Chance
        SetNightChance();
        
        //Set Version Text
        version_1.text = version_2.text = version_3.text = "v" + Application.version;

        //Handle Target FrameRate
        if(QualitySettings.GetQualityLevel() > 1) 
        {
            Application.targetFrameRate = 44;
        }
        else 
        {
            Application.targetFrameRate = 30;
        }
        
        //Check if user has logged in before and has username/pass stored.
        if (PlayerPrefs.GetInt("terms") == 0)
        {
            if (nightChance > 15)
            {
                background.texture = blurTexture;
            }
            else
            {
                background.texture = blurNightTexture;
            }
            terms.SetActive(true);
        }
        else
        {
            if (PlayerPrefs.GetString("username").Length > 0)
            {
                LoginRememberMe();//Login with stored credentials.
            }
            else
            {
                mainScreen.SetActive(true);
                if (nightChance > 15)
                {
                    background.texture = regTexture;
                }
                else
                {
                    background.texture = nightTexture;
                }
            }
        }
        //Change asterisk to dot in input field.
        usernameText.asteriskChar = passwordText.asteriskChar = regUsernameText.asteriskChar = regPassText.asteriskChar = '•';
    }

    private void UpdateText() 
    {
        ChangeNotify("login", storedKeyLogin);
        ChangeNotify("signup", storedKeySignup);
    }

    private void RunServer() 
    {
        Application.targetFrameRate = 20;
        SceneManager.LoadScene(1);
    }

    private void SetNightChance() 
    {
        nightChance = Random.Range(0, 30);
        PlayerPrefs.SetInt("nightChance", nightChance);
        PlayerPrefs.Save();
    }

    public void CheckTerms() 
    {
        if (termsButton.interactable) 
        {
            termsButton.interactable = false;
            termsCheck.SetActive(false);
        }
        else
        {
            termsButton.interactable = true;
            termsCheck.SetActive(true);
        }
    }

    public void ContinueTerms() 
    {
        PlayerPrefs.SetInt("terms", 1);
        PlayerPrefs.Save();
        terms.SetActive(false);
        if (PlayerPrefs.GetString("username").Length > 0)
        {
            LoginRememberMe();//Login with stored credentials.
        }
        else 
        {
            if (nightChance > 15)
            {
                background.texture = regTexture;
            }
            else
            {
                background.texture = nightTexture;
            }
            mainScreen.SetActive(true);
        }
    }

    public void CloseApp() 
    {
        Application.Quit();
    }

    public void OpenPrivacyUrl() 
    {
        Application.OpenURL(privacyPolicyUrl);
    }
    
    public void OpenTermsUrl()
    {
        Application.OpenURL(termsConditionsUrl);
    }

    private void ChangeNotify(string type, string key)
    {
        if (key.Length > 0)
        {
            LangDataSingle langDatas = MultiLangSystem.GetLangDataFromKey(key);
            if (langDatas != null)
            {
                if (type == "login")
                {
                    storedKeyLogin = key;
                    if (loginNotify.text != null)
                    {
                        if (storedFontSizeLogin == 0)
                        {
                            storedFontSizeLogin = loginNotify.fontSize;
                        }
                        loginNotify.text = langDatas.text;
                        if (langDatas.fontSize == 0)
                        {
                            if (loginNotify.fontSize != storedFontSizeLogin)
                            {
                                loginNotify.fontSize = storedFontSizeLogin;
                            }
                        }
                        else
                        {
                            loginNotify.fontSize = langDatas.fontSize;
                        }
                    }
                }
                else if (type == "signup")
                {
                    storedKeySignup = key;
                    if (signupNotify.text != null)
                    {
                        if (storedFontSizeSignup == 0)
                        {
                            storedFontSizeSignup = signupNotify.fontSize;
                        }
                        signupNotify.text = langDatas.text;
                        if (langDatas.fontSize == 0)
                        {
                            if (signupNotify.fontSize != storedFontSizeSignup)
                            {
                                signupNotify.fontSize = storedFontSizeSignup;
                            }
                        }
                        else
                        {
                            signupNotify.fontSize = langDatas.fontSize;
                        }
                    }
                }
            }
        }
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
        if (nightChance > 15)
        {
            if (background.texture != regTexture)
            {
                background.texture = regTexture;
            }
        }
        else
        {
            if (background.texture != nightTexture)
            {
                background.texture = nightTexture;
            }
        }
        regUsernameText.text = "";
        regPassText.text = "";
        usernameText.text = "";
        passwordText.text = "";
    }
    
    //Load MainMenu scene. Called if a login or signup was successful.
    private void LoadGame()
    {
        if (loadScreen.activeSelf) 
        {
        
        }
        else 
        {
            mainScreen.SetActive(false);
            loginScreen.SetActive(false);
            signupScreen.SetActive(false);
            userReporting.SetActive(false);
            loadScreen.SetActive(true);
            StartLoadTip();
            if (nightChance > 15)
            {
                background.texture = blurTexture;
            }
            else
            {
                background.texture = blurNightTexture;
            }
        }
        StartCoroutine(LoadRoutine());//Start loading the MainMenu scene.
    }
    
    //Load routine for loading MainMenu scene. Handles the loading bar and getting player stats.
    private IEnumerator LoadRoutine()
    {
        if (asyncTemp != null)
        {
            LoadRoutineStage(asyncTemp);
        }
        else
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
                else if (loadTime)
                {
                    loadTime = false;
                    loadProgress = 100;
                    LoadRoutineStage(op);
                }
                if (lastLoadProgress != loadProgress) { lastLoadProgress = loadProgress; }
                yield return null;
            }
        }
    }

    private void LoadRoutineStage(AsyncOperation op) 
    {
        webServer.StatRequest(PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), onRequestFinished =>
        {
            if (onRequestFinished)
            {
                loadProgress = 100;
                op.allowSceneActivation = true;
            }
            else
            {
                asyncTemp = op;
                ShowConnectionError(1);
            }
        });
    }

    //Connection Error
    private int currentErrorType = 0;
    
    private void ShowConnectionError(int type) 
    {
        //Error Table
        //  1 = Stats Retrieval Error
        //  2 = Login Request Error
        //  3 = Singup Request Error
        //  4 = Guest Login/Signup Error
        currentErrorType = type;
        loadScreen.SetActive(false);
        loginScreen.SetActive(false);
        signupScreen.SetActive(false);
        mainScreen.SetActive(false);
        connectionError.SetActive(true);
        if (nightChance > 15)
        {
            background.texture = blurTexture;
        }
        else
        {
            background.texture = blurNightTexture;
        }
    }

    public void RetryConnection()
    {
        StartCoroutine(RetryConnectionWait());
    }

    private IEnumerator RetryConnectionWait() 
    {
        connectionError.SetActive(false);
        loadScreen.SetActive(true);
        StartLoadTip();
        yield return new WaitForSeconds(5f);
        if (currentErrorType == 1)
        {
            LoginRememberMe();
        }
        if (currentErrorType == 2)
        {
            LogIn();
        }
        if (currentErrorType == 3)
        {
            SignUp();
        }
        if (currentErrorType == 4)
        {
            DoGuest();
        }
    }

    public void CloseConnectionError() 
    {
        connectionError.SetActive(false);
        Close();
    }

    //Login with stored PlayerPrefs.
    private void LoginRememberMe()
    {
        string username = PlayerPrefs.GetString("username");
        string password = PlayerPrefs.GetString("password");
        
        webServer.LoginRequest(username, password, onRequestFinished => 
        {
            if(onRequestFinished == "TRUE") 
            {
                LoadGame();
            }
            else 
            {
                ShowConnectionError(2);
            }
        });
    }

    //Signup with InputField credentials.
    public void SignUp()
    {
        string username = regUsernameText.text.ToString();
        string password = ToMd5(regPassText.text);
        if(username.Length > 6 && password.Length > 6) 
        {
            string authKey = RandomUserCode(15);
            webServer.SignupRequest(username, password, authKey, onRequestFinished =>
            {
                if (onRequestFinished == "TRUE")
                {
                    PlayerPrefs.SetInt("newPlayer", 1);
                    PlayerPrefs.Save();
                    LoadGame();//Load the MainMenu scene.
                }
                else if (onRequestFinished == "TAKEN")
                {
                    //Unable to Signup. Likely username taken.
                    ChangeNotify("signup", "takenLogin");
                }
                else 
                {
                    ShowConnectionError(3);
                }
            });
        }
    }

    //Login with InputField credentials.
    public void LogIn()
    {
        string username = usernameText.text.ToString();
        string password = ToMd5(passwordText.text);
        webServer.LoginRequest(username, password, onRequestFinished =>
        {
            if (onRequestFinished == "TRUE")
            {
                LoadGame();//Load the MainMenu scene.
            }
            else if(onRequestFinished != "ERROR")
            {
                //Unable to Login. Likely incorrect fields.
                loginNotify.text = "INCORRECT USERNAME/PASSWORD";
                ChangeNotify("login", "incorrectLogin");
            }
            else 
            {
                ShowConnectionError(2);
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
                if (onRequestFinished == "TRUE")
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
                    ShowConnectionError(4);
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
                if (onRequestFinished == "TRUE")
                {
                    LoadGame();//Load the MainMenu scene.
                    PlayerPrefs.SetInt("newPlayer", 1);
                    PlayerPrefs.Save();
                }
                else
                {
                    ///Signup was unsuccesful.
                    //Guest Credentials are taken(rare) or connection error.
                    PlayerPrefs.DeleteKey("guest-a");
                    PlayerPrefs.DeleteKey("guest-b");
                    PlayerPrefs.Save();
                    ShowConnectionError(4);
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

    private string[] loadingTips;
    private int loadingTipIndex;

    private void StartLoadTip()
    {
        string json = File.ReadAllText(Application.dataPath + "/Content/ExtData/loading-tips.txt");
        loadingTips = JsonHelper.FromJson<string>(json);
        loadingTipIndex = Random.Range(0, loadingTips.Length - 1);
        loadTip.text = loadingTips[loadingTipIndex];
        StartCoroutine(LoadTipWait());
    }

    private IEnumerator LoadTipWait()
    {
        yield return new WaitForSeconds(6);
        if (loadingTipIndex + 1 >= loadingTips.Length)
        {
            loadingTipIndex = 0;
        }
        else
        {
            loadingTipIndex++;
        }
        loadTip.text = loadingTips[loadingTipIndex];
        StartCoroutine(LoadTipWait());
    }

}

