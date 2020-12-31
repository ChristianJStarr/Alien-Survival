using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SignInWithApple;

public class LoadSceneScript : MonoBehaviour
{
    public GameObject login_menu, username_menu, error_menu, loading_menu;
    public TextMeshProUGUI game_version;
    public Button username_continue;
    public TMP_InputField username_input;

    public PlayerStats playerStats; //Player Statistics data object.
    public WebServer webServer; //Web Server Handler.

    private SignInWithApple signInWithApple;
    private string privacyPolicyUrl = "https://aliensurvival.com/privacy-app.php";
    private string termsConditionsUrl = "https://aliensurvival.com/terms-app.php";
    private string userGuidlinesUrl = "https://aliensurvival.com/user-guidlines.php";
    private bool loadingLevel = false;

    private string userLoginToken;
    private string userAuthKey;

    void Start() 
    {
#if !UNITY_SERVER  
        //Set Version Text
        game_version.text = string.Format("v{0}", Application.version);

        //Change asterisk to dot in input field.
        username_input.asteriskChar = '•';

        //Check if user has logged in before and has username/pass stored.
        if (!AttemptLoginWithStoredCredentials()) 
        {
            login_menu.SetActive(true);
        }
#endif
    }

    //Button Click: Retry Connection
    public void RetryConnection()
    {
        StartCoroutine(RetryConnectionWait());
    }

    //Button Click: Cancel from Connection Error
    public void CloseConnectionError() 
    {
        error_menu.SetActive(false);
        login_menu.SetActive(true);
    }

    //Button: Continue Login
    public void ContinueLogin()
    {
        if(username_input.text.Length > 6 && userLoginToken.Length > 0) 
        {
            SignupRequest(userLoginToken, userAuthKey, username_input.text);
        }
    }

    //Button Click: Login as Guest
    public void LoginAsGuest()
    {
        userLoginToken = GenerateRandomHash(8);
        userAuthKey = GenerateAuthenticationKey(GenerateRandomHash(10));
        login_menu.SetActive(false);
        username_menu.SetActive(true);
    }

    //Button Click: Login With Apple
    public void LoginWithApple() 
    {
        if(signInWithApple == null) 
        {
            signInWithApple = GetComponent<SignInWithApple>();
            signInWithApple.Login();
        }
    }

    //Open Terms of Use
    public void OpenTerms() 
    {
        if(termsConditionsUrl.Length > 0) 
        {
            Application.OpenURL(termsConditionsUrl);
        }
    }
    
    //Open Privacy Policy
    public void OpenPrivacy() 
    {
        if(privacyPolicyUrl.Length > 0) 
        {
            Application.OpenURL(privacyPolicyUrl);
        }
    }
    
    //User Guidlines
    public void OpenGuidlines() 
    { 
        if(userGuidlinesUrl.Length > 0) 
        {
            Application.OpenURL(userGuidlinesUrl);
        }
    }


    private void LoginRequest(string u_token, string u_authkey)
    {
        webServer.LoginRequest(u_token, u_authkey, requestData =>
        {
            if (requestData.successful)
            {
                SetPlayerPrefs(requestData.credentials, false);
                LoadGame();//Load the MainMenu scene.
            }
            else if (requestData.needsToSignup) 
            {
                login_menu.SetActive(false);
                username_menu.SetActive(true);
            }
            else
            {
                ShowConnectionError(requestData.errorCode);
            }
        });
    }
    private void SignupRequest(string u_token, string u_authkey, string u_name) 
    {
        webServer.SignupRequest(u_token, u_authkey, u_name, requestData =>
        {
            if (requestData.successful)
            {
                SetPlayerPrefs(requestData.credentials, true);
                LoadGame();//Load the MainMenu scene.
            }
            else
            {
                ShowConnectionError(requestData.errorCode);
            }
        });
    }
    private void OnCredentialState(SignInWithApple.CallbackArgs args)
    {
        Debug.Log(string.Format("User credential state is: {0}", args.credentialState));
        if (args.error != null)
            Debug.Log(string.Format("Errors occurred: {0}", args.error));
    }
    private void OnLogin(SignInWithApple.CallbackArgs args)
    {
        if (args.error != null)
        {
            Debug.Log("Errors occurred: " + args.error);
            return;
        }
        userLoginToken = args.userInfo.userId;
        userAuthKey = GenerateAuthenticationKey(args.userInfo.email);
        LoginRequest(userLoginToken, userAuthKey);
    }
    private string GenerateRandomHash(int length)
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
    private string GenerateAuthenticationKey(string input) 
    {
        input = ToMd5(input);
        if(input.Length > 5) 
        {
            return input.Substring(0, 5);
        }
        return input;
    }
    private string ToMd5(string input)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        byte[] bytes = encoding.GetBytes(input);
        var sha = new System.Security.Cryptography.MD5CryptoServiceProvider();
        return System.BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
    }
    private IEnumerator RetryConnectionWait()
    {
        error_menu.SetActive(false);
        loading_menu.SetActive(true);
        game_version.gameObject.SetActive(false); ;
        yield return new WaitForSeconds(4f);
        ContinueLogin();
    }
    private void SetPlayerPrefs(UserCredentialData credentials, bool newPlayer)
    {
        PlayerPrefs.SetString("username", credentials.user_name);
        PlayerPrefs.SetString("token", credentials.user_token);
        PlayerPrefs.SetString("authKey", credentials.user_authkey);
        PlayerPrefs.SetInt("userId", credentials.user_id);
        if (newPlayer) 
        {
            PlayerPrefs.SetInt("newPlayer", 1);
        }
        PlayerPrefs.Save();
    }
    public void InputFieldTextChanged()
    {
        if (username_input.text.Length > 6)
        {
            username_continue.interactable = true;
        }
        else
        {
            username_continue.interactable = false;
        }
    }
    private bool AttemptLoginWithStoredCredentials()
    {
        string u_token = PlayerPrefs.GetString("token");
        string u_authkey = PlayerPrefs.GetString("authKey");
        if (u_authkey.Length > 0 && u_token.Length > 0)
        {
            LoginRequest(u_token, u_authkey);
            return true;
        }
        return false;
    }
    private void LoadGame()
    {
        if (!loadingLevel)
        {
            loadingLevel = true;
            login_menu.SetActive(false);
            username_menu.SetActive(false);
            error_menu.SetActive(false);
            loading_menu.SetActive(true);
            game_version.gameObject.SetActive(false);
            SceneManager.LoadSceneAsync(1);
        }
    }
    private void ShowConnectionError(int type)
    {
        //Error Table
        //  1 = Wrong Authkey
        //  2 = Invalid Configuration
        //  3 = Network Error
        login_menu.SetActive(false);
        username_menu.SetActive(false);
        loading_menu.SetActive(false);
        game_version.gameObject.SetActive(true);
        error_menu.SetActive(true);
    }
}

