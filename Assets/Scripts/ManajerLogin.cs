using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Threading;

[System.Serializable]
public class Account
{
    public string username;
    public string passwordHash;
    public int loginAttempts;
    public bool isOnline;
    public string email;
    public string loginCooldownEndTime;
    public CharacterSO characterSO;
    public DateTime loginCooldownEndDateTime
    {
        get { return DateTime.Parse(loginCooldownEndTime); }
        set { loginCooldownEndTime = value.ToString(); }
    }
    public Account(string username, string password, string email)
    {
        this.username = username;
        this.passwordHash = password;
        this.email = email;
        this.loginAttempts = 0;
        this.loginCooldownEndDateTime = DateTime.MinValue;
        this.characterSO = ScriptableObject.CreateInstance<CharacterSO>();
    }
    public string HashPassword(string password)
    {
        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }
    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}

public static class AccountDatabase
{
    public static List<Account> accounts = new List<Account>();
    private const int MaxLoginAttempts = 3;
    private const int LoginCooldownMinutes = 3;
    public static string errorText;
    private static string saveFilePath = Path.Combine(Application.dataPath, "accountData.json");
    public static void SaveData()
    {
        bool dataSaved = false;
        int retryCount = 0;
        const int maxRetryCount = 3; // Maximum number of retries

        while (!dataSaved && retryCount < maxRetryCount)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(accounts);
                File.WriteAllText(saveFilePath, jsonData);
                dataSaved = true; // Mark the data as saved to exit the loop
            }
            catch (Exception ex)
            {
                // Handle the exception (you can log the error, display a message, etc.)
                Debug.LogError("Error while saving data: " + ex.Message);

                // Wait for a short time before the next retry (you can adjust the delay time)
                Thread.Sleep(1000); // 1 second delay

                // Increment the retry count
                retryCount++;
            }
        }
    }

    public static void LoadData()
    {
        if (File.Exists(saveFilePath))
        {
            string jsonData = File.ReadAllText(saveFilePath);
            accounts = JsonConvert.DeserializeObject<List<Account>>(jsonData);
        }
        else
        {
            Debug.LogWarning("Save file not found: " + saveFilePath);
        }
    }

    public static void UpdateAccountField(string username, Action<Account> updateAction)
    {
        // Load data dari file JSON ke dalam array accounts
        LoadData();

        // Cari akun berdasarkan username
        Account accountToUpdate = accounts.Find(account => account.username == username);
        if (accountToUpdate == null)
        {
            Debug.LogWarning("Account not found!");
            return;
        }

        // Panggil action untuk memperbarui field yang diinginkan pada akun
        updateAction(accountToUpdate);

        // Simpan kembali array yang telah diperbarui ke dalam JSON
        SaveData();

        Debug.Log("Account field updated successfully!");
    }

    public static bool SignUpAccount(string username, string password, string email)
    {
        // Validasi format email menggunakan regular expression
        if (!IsValidEmail(email) || EmailExists(email))
        {
            errorText = "Invalid email format or email is already registered!";
            return false;
        }
        // Validasi panjang username
        if (username.Length < 3 || string.IsNullOrEmpty(username))
        {
            errorText = "Username is required and should be at least 3 characters long!";
            return false;
        }
        // Validasi username tidak boleh mengandung angka atau spesial karakter
        if (ContainsNumber(username) || HasSpecialCharacter(username) || HasUppercaseLetter(username))
        {
            errorText = "Username cannot contain numbers, uppercase or special characters!";
            return false;
        }
        // Cek apakah username sudah ada dalam database
        if (AccountExists(username))
        {
            errorText = "Username already taken!";
            return false;
        }
        if (password.Length < 8)
        {
            errorText = "Password should be at least 8 characters long!";
            return false;
        }

        if (!HasSpecialCharacter(password) || !HasUppercaseLetter(password))
        {
            errorText = "Password should have at least one special character and one uppercase letter!";
            return false;
        }

        errorText = string.Empty;
        return true;
    }
    public static void CreateAccount(string username, string password, string email, CharacterSO characterSO)
    {
        Account newAccount = new Account(username, password, email);
        newAccount.passwordHash = newAccount.HashPassword(password);
        newAccount.characterSO = characterSO;
        accounts.Add(newAccount);
        Debug.Log("Account registered successfully!");
        SaveData();
    }
    public static bool LoginAccount(string username, string password)
    {
        // Validasi username kosong
        if (string.IsNullOrEmpty(username))
        {
            errorText = "Username is required!";
            return false;
        }

        // Validasi panjang password
        if (password.Length < 8)
        {
            errorText = "Password should be at least 8 characters long!";
            return false;
        }

        // Validasi password harus memiliki minimal satu karakter khusus dan satu huruf besar
        if (!HasSpecialCharacter(password) || !HasUppercaseLetter(password))
        {
            errorText = "Password should have at least one special character and one uppercase letter!";
            return false;
        }

        // Cek apakah akun ada dalam database
        Account account = GetAccount(username);
        if (account == null)
        {
            errorText = "Username not found!";
            return false;
        }

        // Cek apakah akun sedang dalam cooldown
        if (account.loginCooldownEndDateTime > DateTime.Now)
        {
            errorText = "Login is banned until  " + account.loginCooldownEndDateTime;
            return false;
        }
        // Verifikasi password
        bool passwordMatch = account.VerifyPassword(password);
        if (!passwordMatch)
        {
            errorText = "Incorrect password!";

            // Meningkatkan jumlah percobaan login dan menetapkan cooldown jika mencapai batas
            account.loginAttempts++;
            int attempt = account.loginAttempts;
            AccountDatabase.UpdateAccountField(account.username, (account) => {account.loginAttempts = attempt;});
            if (account.loginAttempts >= MaxLoginAttempts)
            {
                account.loginCooldownEndDateTime = DateTime.Now.AddMinutes(LoginCooldownMinutes);
                DateTime times = account.loginCooldownEndDateTime;
                AccountDatabase.UpdateAccountField(account.username, (account) => {account.loginCooldownEndTime = times.ToString();});                
                //Debug.Log("Account is in cooldown. Please try again later.");
            }
            return false;
        }
   
        
        // Cek apakah akun sedang online
        if (account.isOnline)
        {
            errorText = "Account is already logged in from another device!";
            return false;
        }

        // Reset jumlah percobaan login dan cooldown setelah berhasil login
        account.loginAttempts = 0;
        int attempt2 = account.loginAttempts;
        UpdateAccountField(account.username, (account) => {account.loginAttempts = attempt2;});

        account.loginCooldownEndDateTime = DateTime.MinValue;
        DateTime times2 = account.loginCooldownEndDateTime;
        UpdateAccountField(account.username, (account) => {account.loginCooldownEndTime = times2.ToString();});                
        errorText = string.Empty;

        account.isOnline = true;
        UpdateAccountField(account.username, (account) => {account.isOnline = true;});                
        Debug.Log("Login successful!");
        return true;
    }

    public static void LogoutAccount(string username)
    {
        Account account = GetAccount(username);
        if (account == null)
        {
            errorText = $"Username {username} not found!";
            return;
        }

        if(!account.isOnline)
        {
            errorText = $"Can't logout, account {username} is offline!";
            return;
        }
        if (account != null)
        {
            account.isOnline = false;
            errorText = $"{username} successfully logged out!";
            SaveData();
        }
    }

    private static bool IsValidEmail(string email)
    {
        // Regular expression untuk validasi format email
        string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        return Regex.IsMatch(email, pattern);
    }

    private static bool ContainsNumber(string text)
    {
        return text.Any(char.IsDigit);
    }

    private static bool HasSpecialCharacter(string text)
    {
        return text.Any(c => !char.IsLetterOrDigit(c));
    }

    private static bool HasUppercaseLetter(string text)
    {
        return text.Any(char.IsUpper);
    }
    private static bool AccountExists(string username)
    {
        return accounts.Exists(account => account.username == username);
    }
    private static bool EmailExists(string email)
    {
        return accounts.Exists(account => account.email == email);
    }
    public static Account GetAccount(string username)
    {
        return accounts.Find(account => account.username == username);
    }
}

public class ManajerLogin : MonoBehaviour
{
    [SerializeField] private GameObject panelSignUp, panelSignIn;
    [SerializeField] List<Account> accountList = new List<Account>();
    [SerializeField] private TMP_InputField usernameRegInput, usernameLogInput;
    [SerializeField] private TMP_InputField passwordRegInput,passwordLogInput;
    [SerializeField] private Button toggleRegPassword, toggleLogPassword;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton, logOutButton;
    [SerializeField] private TextMeshProUGUI validationRegText, validationLogText;
    [SerializeField] private CharacterSO characterSO;
    [SerializeField] private List<NewCharacter> listJob;
    [SerializeField] private NewCharacter selectedCharacter;
    [SerializeField] private GameObject showCharacter;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject go;
    [SerializeField] private Button startGame;
    private bool isVisible = false;

    private void Start()
    {
        panelSignIn.SetActive(true);
        panelSignUp.SetActive(false);
        AccountDatabase.LoadData();
        accountList = AccountDatabase.accounts;

        // Menambahkan event listener untuk tombol register
        registerButton.onClick.AddListener(RegisterAccount);

        // Menambahkan event listener untuk tombol login
        loginButton.onClick.AddListener(LoginAccount);

        // Menambahkan event listener untuk tombol logout
        logOutButton.onClick.AddListener(LogoutAccount);

        toggleLogPassword.onClick.AddListener(() => TogglePassword(passwordLogInput));
        toggleRegPassword.onClick.AddListener(() => TogglePassword(passwordRegInput));
        startGame.onClick.AddListener(() => StartGame());   
    }

    private void RegisterAccount()
    {
        // Memanggil fungsi RegisterAccount dari AccountDatabase
        bool success = AccountDatabase.SignUpAccount(usernameRegInput.text, passwordRegInput.text, emailInput.text);
        validationRegText.text = AccountDatabase.errorText;
        if (success)
        {
            // Melakukan sesuatu jika pendaftaran berhasil

            // Setelah pendaftaran, kita dapat langsung melakukan login
            //LoginAccount();
            panelSignUp.SetActive(false);
            panelSignIn.SetActive(false);
            startGame.gameObject.SetActive(true);
            DisplayCharacterSelector();
            SelectCharacter(listJob[0]);     
        }
    }

    private void LoginAccount()
    {
        string username = usernameLogInput.text;
        string password = passwordLogInput.text;
        // Memanggil fungsi LoginAccount dari AccountDatabase
        bool success = AccountDatabase.LoginAccount(username, password);
        validationLogText.text = AccountDatabase.errorText;
        if (success)
        {
            // Memanggil fungsi login dari NetworkManager jika berhasil login
            ContinueGame(username);
            panelSignIn.SetActive(false);
            panelSignUp.SetActive(false);
        }
    }

    private void LogoutAccount()
    {
        string username = usernameLogInput.text;
        AccountDatabase.LogoutAccount(username);
        validationLogText.text = AccountDatabase.errorText;

    }

    private void ContinueGame(string username)
    {
        Debug.Log("Connected to server!");
        Account account = AccountDatabase.GetAccount(username);
        OverrideCharacterSO(characterSO, account.characterSO);
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private void TogglePassword(TMP_InputField inputField)
    {
        isVisible = !isVisible;
        if(isVisible)
        {
            inputField.contentType = TMP_InputField.ContentType.Standard;
        } else
        {
            inputField.contentType = TMP_InputField.ContentType.Password;    
        }
        // Mengganti karakter tersembunyi jika menggunakan Content Type Password
        inputField.ForceLabelUpdate();    
    }

    private void DisplayCharacterSelector()
    {
        for(int i = 0; i < listJob.Count; i++)
        {
            GameObject slot = Instantiate(showCharacter, slotParent.transform.position, Quaternion.identity);
            slot.transform.SetParent(slotParent.transform);
            slot.transform.localScale = Vector3.one;
            Button selectCharacter = slot.transform.Find("Button").GetComponent<Button>();
            Image icon = slot.transform.Find("RoleIcon").transform.Find("Icon").GetComponent<Image>();
            Image bgIcon = slot.transform.Find("Background").GetComponent<Image>();
            TextMeshProUGUI nameText =  slot.transform.Find("Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText =  slot.transform.Find("Desc").GetComponent<TextMeshProUGUI>();
            nameText.text = listJob[i].name;
            descText.text = listJob[i].description;
            icon.sprite = listJob[i].sprite;
            int index = i;
            selectCharacter.onClick.RemoveAllListeners();
            selectCharacter.onClick.AddListener(() => SelectCharacter(listJob[index]));   
            bgIcon.sprite = listJob[i].backgroundSprite;         
        }
    }
    private void SelectCharacter(NewCharacter selected)
    {
        selectedCharacter = selected;
        if(go != null)
        {
            Destroy(go.gameObject);
        }
        go = Instantiate(Resources.Load<GameObject>(selected.modelPrefab), new Vector3(1.5f,0f,-7f), Quaternion.identity);
        go.transform.Rotate(new Vector3(0,200,0));
        OverrideCharacterSO(characterSO, selectedCharacter.characterSO);
        AccountDatabase.SaveData();
    }
    
    private void OverrideCharacterSO(CharacterSO asIs, CharacterSO newCharacterSO)
    {
        asIs.x = newCharacterSO.x;
        asIs.y = newCharacterSO.y;
        asIs.z = newCharacterSO.z;
        asIs.chanceRate = newCharacterSO.chanceRate;
        asIs.criticalRate = newCharacterSO.criticalRate;
        asIs.curDef = newCharacterSO.curDef;
        asIs.curMagic = newCharacterSO.curMagic;
        asIs.currentHealth = newCharacterSO.currentHealth;
        asIs.currentManaPoints = newCharacterSO.currentManaPoints;
        asIs.curStr= newCharacterSO.curStr;
        asIs.defMultiplier = newCharacterSO.defMultiplier;    
        asIs.experiencePoints = newCharacterSO.experiencePoints;
        asIs.expRate = newCharacterSO.expRate;
        asIs.fireAtk = newCharacterSO.fireAtk;
        asIs.fireDef= newCharacterSO.fireDef;
        asIs.gold = newCharacterSO.gold;  
        asIs.goldRate = newCharacterSO.goldRate;
        asIs.healthMultiplier = newCharacterSO.healthMultiplier;
        asIs.hitAtk = newCharacterSO.hitAtk;
        asIs.hitDef = newCharacterSO.hitDef;
        asIs.iceAtk = newCharacterSO.iceAtk;
        asIs.iceDef = newCharacterSO.iceDef;
        asIs.job = newCharacterSO.job;
        asIs.level = newCharacterSO.level;
        asIs.magicMultiplier = newCharacterSO.magicMultiplier;
        asIs.manaMultiplier = newCharacterSO.manaMultiplier;
        asIs.maxManaPoints = newCharacterSO.maxManaPoints;
        asIs.maxHealth = newCharacterSO.maxHealth;
        asIs.modelPrefab = newCharacterSO.modelPrefab;
        //asIs.particleCritical = newCharacterSO.particleCritical;
        //asIs.particleMiss = newCharacterSO.particleMiss;
        //asIs.playerDamageText = newCharacterSO.playerDamageText;
        asIs.regenDelay = newCharacterSO.regenDelay;
        asIs.regenRate = newCharacterSO.regenRate;
        asIs.skillPointLeft = newCharacterSO.skillPointLeft;
        asIs.skillPointUsed = newCharacterSO.skillPointUsed;
        asIs.soulAtk = newCharacterSO.soulAtk;
        asIs.soulDef = newCharacterSO.soulDef;
        asIs.startDef = newCharacterSO.startDef;
        asIs.startHealth = newCharacterSO.startHealth;
        asIs.startMagic = newCharacterSO.startMagic;
        asIs.startMana = newCharacterSO.startMana;
        asIs.startStr = newCharacterSO.startStr;
        asIs.strMultiplier = newCharacterSO.strMultiplier;
        asIs.thunderAtk = newCharacterSO.thunderAtk;
        asIs.thunderDef = newCharacterSO.thunderDef;
        asIs.userName = newCharacterSO.userName;
    }
    private void StartGame()
    {
        AccountDatabase.CreateAccount(usernameRegInput.text,passwordRegInput.text,emailInput.text,selectedCharacter.characterSO);
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
}

[System.Serializable]
public class NewCharacter
{
    public string modelPrefab;
    public CharacterSO characterSO;
    public string name;
    public string description;
    public Sprite sprite;
    public Sprite backgroundSprite;
}