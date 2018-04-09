using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviour
{
    //panels
    public GameObject RegForm;
    public GameObject AuthForm;
    //change panels
    public Button button_SingIn;
    public Button button_SignUp;
    //try reg and auth
    public Button button_TrySingIn;
    public Button button_TrySingUp;
    //input fields registartion
    public InputField text_regMail;
    public InputField text_regLogin;
    public InputField text_regPass1;
    public InputField text_regPass2;
    //input fields auth
    public InputField text_authLogin;
    public InputField text_authPass;
    //message box
    public GameObject message;

    private Validation validation;
    private Regex regexMail = new Regex(
       @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
       @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
       RegexOptions.IgnoreCase);

    //private AsyncOperation sceneAsync;

    private void Start()
    {
        validation = new Validation();
        
        button_SingIn.onClick.AddListener(SelectSingIn);
        button_SignUp.onClick.AddListener(SelectSignUp);
        button_TrySingIn.onClick.AddListener(()=> { StartCoroutine(TrySignIn()); });
        button_TrySingUp.onClick.AddListener(()=> { StartCoroutine(TrySignUp()); });

        text_regMail.onEndEdit.AddListener(delegate { StartCoroutine(MailCheck()); });
        text_regLogin.onEndEdit.AddListener(delegate { StartCoroutine(LoginCheck()); });
        text_regPass1.onEndEdit.AddListener(delegate { FirstPassCheck(); });
        text_regPass2.onEndEdit.AddListener(delegate { SecondPassCheck(); });
    }

    public void SelectSignUp()
    {
        if (AuthForm.activeSelf)
        {
            AuthForm.SetActive(false);
            RegForm.SetActive(true);   
        }
    }

    public void SelectSingIn()
    {
        if (RegForm.activeSelf)
        {
            RegForm.SetActive(false);
            AuthForm.SetActive(true);
        }
    }

    public IEnumerator TrySignIn()
    {
        yield return StartCoroutine(DataBaseConnector.AuthenticationPlayer(
            text_authLogin.text, text_authPass.text, validation));

        if (validation.ConfirmSignIn)
        {
            Cmn.player_id = validation.ID;
            SceneManager.LoadScene("Preload", LoadSceneMode.Additive);
        }
        else
        {
            ShowMessage("Login or password invalid. Check it and try again!");
        }
    }

    public IEnumerator TrySignUp()
    {
        if (validation.MailChecked && validation.LoginChecked &&
            validation.FstPassChecked && validation.SndPassChecked)
        {
            yield return StartCoroutine(DataBaseConnector.RegistrationPlayer(
            text_regLogin.text, text_regMail.text, text_regPass1.text, validation));

            if (validation.ConfirmSignUp)
            {
                ShowMessage("You have registered successfully, " +
                            "Now you can enter the game!");

                button_SingIn.onClick.Invoke();

                text_regMail.text = "";
                text_regLogin.text = "";
                text_regPass1.text = "";
                text_regPass2.text = "";
            }
            else
            {
                ShowMessage("Login and Mail were not confirmed on server!");
            }
        }
        else
        {
            ShowMessage("invalid!");
        }
    }

    public IEnumerator MailCheck()
    {
        validation.MailChecked = false;

        string mail = text_regMail.text;

        if (String.IsNullOrEmpty(mail))
        {
            ShowWarning(
                text_regMail.gameObject.transform,
                "Enter email field!");
            //ShowMessage("Enter email field!");
            yield break;
        }

        if (!regexMail.IsMatch(mail))
        {
            ShowWarning(
                text_regMail.gameObject.transform,
                "Entered mail is not valid");
            //ShowMessage("Entered mail is not valid");
            yield break;
        }

        yield return StartCoroutine(DataBaseConnector.MailExist(mail, validation));

        if (validation.MailExist)
        {
            ShowWarning(
                text_regMail.gameObject.transform,
                "This mail is already exist");
            //ShowMessage("This mail is already exist");
        }
        else
        {
            HideWarning(text_regMail.gameObject.transform);
            validation.MailChecked = true;
        }
    }

    public IEnumerator LoginCheck()
    {
        validation.LoginExist = false;

        string login = text_regLogin.text;

        if (String.IsNullOrEmpty(login))
        {
            ShowWarning(
                text_regLogin.gameObject.transform,
                "Enter login field!");
            //ShowMessage("Enter login field!");
            yield break;
        }

        if (false)
        {
            //check login (lenght, simbols and so on)
        }

        yield return StartCoroutine(DataBaseConnector.LoginExist(login, validation));

        if (validation.LoginExist)
        {
            ShowWarning(
                text_regLogin.gameObject.transform,
                "This login is already exist");
            //ShowMessage("This login is already exist");
        }
        else
        {
            HideWarning(text_regLogin.gameObject.transform);
            validation.LoginChecked = true;
        }
    }

    public void FirstPassCheck()
    {
        validation.FstPassChecked = false;

        string password1 = text_regPass1.text;

        if (String.IsNullOrEmpty(password1))
        {
            ShowWarning(
                text_regPass1.gameObject.transform,
                "Enter password field!");
            //ShowMessage("Enter password field!");
            return;
        }

        if (false)
        {
            //check password strong
        }

        HideWarning(text_regPass1.gameObject.transform);
        validation.FstPassChecked = true;
    }

    public void SecondPassCheck()
    {
        validation.SndPassChecked = false;

        string password2 = text_regPass2.text;

        if (String.IsNullOrEmpty(password2))
        {
            ShowWarning(
                text_regPass2.gameObject.transform,
                "Repeat your password!");
            //ShowMessage("Enter password field!");
            return;
        }

        string password1 = text_regPass1.text;

        if (password1 != password2)
        {
            ShowWarning(
                text_regPass2.gameObject.transform,
                "Passwords are different");
            //ShowMessage("You have entered different password. Check it and try again!");
            return;
        }

        validation.SndPassChecked = true;
        HideWarning(text_regPass2.gameObject.transform);
    }

    private void ShowMessage(string text)
    {
        message.SetActive(true);
        message.GetComponentInChildren<Text>().text = text;
    }

    private void ShowWarning(Transform transform, string text)
    {
        GameObject warning = transform.Find("Warning").gameObject;
        warning.GetComponentInChildren<Text>().text = text;
        warning.SetActive(true);
    }

    private void HideWarning(Transform transform)
    {
        transform.Find("Warning").gameObject.SetActive(false);
    }

    //IEnumerator loadScene(int index)
    //{
    //    AsyncOperation scene = SceneManager.LoadSceneAsync(
    //        index, LoadSceneMode.Additive);

    //    scene.allowSceneActivation = false;
    //    sceneAsync = scene;

    //    //Wait until we are done loading the scene
    //    while (scene.progress < 0.9f)
    //    {
    //        Debug.Log("Loading scene " + " [][] Progress: " + scene.progress);
    //        yield return null;
    //    }
    //    OnFinishedLoadingAllScene();
    //}
    //void enableScene(int index)
    //{
    //    //Activate the Scene
    //    sceneAsync.allowSceneActivation = true;
    //    Scene sceneToLoad = SceneManager.GetSceneByBuildIndex(index);
    //    if (sceneToLoad.IsValid())
    //    {
    //        Debug.Log("Scene is Valid");
    //        SceneManager.MoveGameObjectToScene(, sceneToLoad);
    //        SceneManager.SetActiveScene(sceneToLoad);
    //    }
    //}
    //void OnFinishedLoadingAllScene()
    //{
    //    Debug.Log("Done Loading Scene");
    //    enableScene(2);
    //    Debug.Log("Scene Activated!");
    //}
}

public class Validation
{
    //if mail is already exist in DB
    public bool MailExist;
    //if login is already exist in DB
    public bool LoginExist;
    //if sign in is confirmed
    public bool ConfirmSignIn;
    //if sign up is confirmed
    public bool ConfirmSignUp;
    //if mail checked
    public bool MailChecked;
    //if login checked
    public bool LoginChecked;
    //if password checked
    public bool FstPassChecked;
    //if password repeat checked
    public bool SndPassChecked;
    //get by auth
    public int ID;

    public Validation()
    {
        ConfirmSignIn = false;
        ConfirmSignUp = false;
        FstPassChecked = false;
        SndPassChecked = false;
        MailChecked = false;
        LoginChecked = false;
        MailExist = false;
        LoginExist = false;
        ID = -1;
    }
}
