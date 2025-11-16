# twitch-streaming-tools

Tools to aid twitch streamers.

## To Install Additional Voices

To install additional voices you need to install the language in Windows 11:
1. Open the start menu and search for `Language Settings`

![The language settings menu](assets/images/language-settings.png)

2. In the "Preferred languages" row, click "Add a language"

![Add a language button](assets/images/add-a-language.png)

3. In the "Choose a language to install" dialog, select the language with "Text-to-speech" icon.
   * **Note:** Only items with the text-to-speech icon will show up in the application once installed.

![Choose language dialog](assets/images/choose-language.png)

4. In the "Install language features" dialog, you only need to install the "Text-to-speech" for the language and click install. 
   * **Note:** All other boxes may be **unchecked**.

![Install language features dialog](assets/images/install-language-features.png)

Once installed (it may take some time), restart the application and they should be displayed in the settings menu under
TTS Voices.

![TTS settings](assets/images/tts-voices.png)

## Design

### Account Management

Relationship between the view model and the account manager used to keep Twitch OAuth credentails up-to-date

```mermaid
classDiagram
    AccountViewModel o-- IAccountManager
    IAccountManager
    class AccountViewModel{
        -IAccountManager _accountManager
        +LaunchOAuthBrowser()
        +DeleteCredentials()
    }
    class IAccountManager{
        +bool CredentialsAreValid
        +Action<bool> OnCredentialStatusChanged
        +void UpdateCredentials(string bearer, string refresh, DateTime expires)
        +void DeleteCredentials()
        -void CheckCredentials()
    }
```