using Microsoft.JSInterop;

namespace SvHofkirchenWasm.Services;

public class EncryptionService
{
    private readonly IJSRuntime _js;
    private string? _sessionPassword;

    public EncryptionService(IJSRuntime js)
    {
        _js = js;
    }

    // Speichert das Passwort für die Dauer der Browsersitzung im RAM
    public void SetSessionPassword(string password)
    {
        _sessionPassword = password;
    }

    public async Task<string> Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(_sessionPassword) || string.IsNullOrEmpty(plainText)) 
            return plainText;
            
        return await _js.InvokeAsync<string>("encryptionHelper.encryptData", plainText, _sessionPassword);
    }

    public async Task<string> Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(_sessionPassword) || string.IsNullOrEmpty(encryptedText)) 
            return encryptedText;
            
        return await _js.InvokeAsync<string>("encryptionHelper.decryptData", encryptedText, _sessionPassword);
    }
}