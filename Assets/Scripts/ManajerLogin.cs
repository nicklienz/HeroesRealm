using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
//using Mirror;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

[System.Serializable]
public class Account
{
    public string username;
    public string passwordHash;
    public int loginAttempts;
    public bool isOnline;
    public string email;
    public string loginCooldownEndTime;
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
    private const int LoginCooldownMinutes = 10;
    public static string errorText;
    private static string saveFilePath = Path.Combine(Application.dataPath, "accountData.json");
    public static void SaveData()
    {
        string jsonData = JsonConvert.SerializeObject(accounts);
        File.WriteAllText(saveFilePath, jsonData);
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

    public static bool RegisterAccount(string username, string password, string email)
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
        // Menambahkan akun baru ke dalam database
        Account newAccount = new Account(username, password, email);
        newAccount.passwordHash = newAccount.HashPassword(password);
        accounts.Add(newAccount);
        SaveData();
        Debug.Log("Account registered successfully!");
        return true;
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
            errorText = "Account is in cooldown. Please try again later after " + account.loginCooldownEndDateTime;
            return false;
        } else
        {
            account.loginAttempts = 0;
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
            errorText = $"Username {username}  not found!";
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
    private static Account GetAccount(string username)
    {
        return accounts.Find(account => account.username == username);
    }
}

public class ManajerLogin : MonoBehaviour
{
    [SerializeField] List<Account> accountList = new List<Account>();
    [SerializeField] private TMP_InputField usernameRegInput, usernameLogInput;
    [SerializeField] private TMP_InputField passwordRegInput,passwordLogInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton, logOutButton;
    [SerializeField] private TextMeshProUGUI validationRegText, validationLogText;

    //private NetworkManager networkManager;

    private void Start()
    {
        AccountDatabase.LoadData();
        accountList = AccountDatabase.accounts;
        // Mendapatkan instance NetworkManager
        //networkManager = NetworkManager.singleton;

        // Menambahkan event listener untuk tombol register
        registerButton.onClick.AddListener(RegisterAccount);

        // Menambahkan event listener untuk tombol login
        loginButton.onClick.AddListener(LoginAccount);

        // Menambahkan event listener untuk tombol logout
        logOutButton.onClick.AddListener(LogoutAccount);
    }

    private void RegisterAccount()
    {
        string username = usernameRegInput.text;
        string password = passwordRegInput.text;
        string email = emailInput.text;
        // Memanggil fungsi RegisterAccount dari AccountDatabase
        bool success = AccountDatabase.RegisterAccount(username, password, email);
        validationRegText.text = AccountDatabase.errorText;
        if (success)
        {
            // Melakukan sesuatu jika pendaftaran berhasil

            // Setelah pendaftaran, kita dapat langsung melakukan login
            //LoginAccount();
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
            ConnectToServer();
        }
    }

    private void LogoutAccount()
    {
        string username = usernameLogInput.text;
        AccountDatabase.LogoutAccount(username);
        validationLogText.text = AccountDatabase.errorText;
        //networkManager.StopClient();

    }
    private void ConnectToServer()
    {
        // Memanggil fungsi login dari NetworkManager
        //networkManager.StartClient();

        Debug.Log("Connected to server!");
    }
}