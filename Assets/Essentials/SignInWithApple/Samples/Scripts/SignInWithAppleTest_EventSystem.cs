using System;
using UnityEngine;
using UnityEngine.SignInWithApple;

public class SignInWithAppleTest_EventSystem : MonoBehaviour
{
    private string userId;

    public void ButtonPress()
    {
        var siwa = gameObject.GetComponent<SignInWithApple>();
        siwa.Login();
    }

    public void CredentialButton()
    {
        // User id that was obtained from the user signed into your app for the first time.
        var siwa = gameObject.GetComponent<SignInWithApple>();
        siwa.GetCredentialState(userId);
    }

    public void OnCredentialState(SignInWithApple.CallbackArgs args)
    {
        Debug.Log(string.Format("User credential state is: {0}", args.credentialState));

        if (args.error != null)
            Debug.Log(string.Format("Errors occurred: {0}", args.error));
    }

    public void OnLogin(SignInWithApple.CallbackArgs args)
    {
        Debug.Log("Sign in with Apple login has completed.");

        UserInfo userInfo = args.userInfo;
        
        // Save the userId so we can use it later for other operations.
        userId = userInfo.userId;
        
        // Print out information about the user who logged in.
        Debug.Log(
            string.Format("Display Name: {0}\nEmail: {1}\nUser ID: {2}\nID Token: {3}", userInfo.displayName,
                userInfo.email, userInfo.userId, userInfo.idToken));
    }

    public void OnError(SignInWithApple.CallbackArgs args)
    {
        Debug.Log(string.Format("A Sign in with Apple error has occured! {0}", args.error));
}
}
