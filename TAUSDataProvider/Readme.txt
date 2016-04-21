Multilingual App Toolkit (MAT) translation provider sample using TAUS DATA APIs.

This sample is built using Microsoft.Multilingual.Translation.dll v4.0.1605.0. If you are 
targeting a different build of MAT, you will need to build against that version of the 
DLL as well as adjust for any API changes.

Once the project is built, you will need to add the provider it to the MAT Providers 
configuration file.  Following the steps below will enable this provider with your 
MAT v4.0 installation.

1. Copy TAUSDATAProvider.dll to "%CommonProgramFiles(x86)%\Multilingual App Toolkit"
   (and any satellite DLLS in the related sub folder. E.g.: fr-FR\TAUSDataProvider.resources.dll).
2. Update "%ALLUSERSPROFILE%\Multilingual App Toolkit\TranslationManager.xml
   Enable the TAUS provider by adding the following XML configuration (Requires admin rights).  
   Note: Matching translation results are prioritized based on the order of the providers 
   in the configuration file.  It is recommended to place this provider as the first provider 
   to ensure it is given priority during translation and suggestion selection.
   ...
    <Provider>
      <ID>3AB3DC49-4A77-4371-AFBB-21876E25A40B</ID>
      <Name>TAUSDataProvider</Name>
      <ConfigFile>TAUSDataProvider\TAUSDataSettings.xml</ConfigFile>
      <AssemblyPath>TAUSDataProvider.dll</AssemblyPath>
    </Provider>
   ...

3. Create the folder "%ALLUSERSPROFILE%\Multilingual App Toolkit\TAUSDataProvider"
3. Copy TAUSDataSetting.xml to "%ALLUSERSPROFILE%\Multilingual App Toolkit\TAUSDataProvider" 
   This holds the TAUS DATA Provider configuration setting used to access the Windows 
   Credential Manager store.
4. Create a Generic Windows Credential to store your TAUS DATA* user account and password
   a) Control Panel -> Credential Manager -> Windows Credentials -> Add a generic credential
   b) Internet or network address: https://www.tausdata.org/api
   c) User name: <The registered TAUS data user account> 
   d) Password: <The registered TAUS data user account's password> 

* Note: Only registered users are allowed to access the TAUS DATA APIs. You can register for free at https://www.tausdata.org.

To test the provider after it is built and configured:
1. Create a MAT enabled project using en-US as the source language
2. Add the source string "Do you want to save changes?"
3. Added fr-FR as the target language.
4. Build
5. Open the French XLF file in the Multilingual Editor.  
6. Highlight the resource string "Do you want to save changes?" and select Suggest from the ribbon (It should display the TAUS DATA image)

That should get you working.

Trouble shooting:
Q: How can I tell if the provider is loaded?
A: The quickest way is to ensure the provider is listed first in the configuration file as the 
   editor only displays the first supported provider’s image based on language pairs (e.g.: en-US -> fr-FR).

Q: Everything is installed, but the provider is not loading
A: The provider needs to be compiled against the same build as the 
   Microsoft.Multilingual.Translation.dll installed on your system.  If you try to 
   translate a resource, the load error will be displayed in the Editor Message tab or
   in Visual Studio's output panel.  The message should provide the details of the error.

Common configuration errors as seen in the Editor Message panel when the XLF file is loaded: 

Error: "An error was encountered while loading the translation providers. One or more 
        translation providers will be unavailable until the problem is corrected. Message:
        System.IO.DirectoryNotFoundException: Could not find a part of the path 
        'C:\ProgramData\Multilingual App Toolkit\TAUSDataProvider\TAUSDataSettings.xml'. 
		..." 
Fix:   Ensure TAUSDataSetting.xml is in the correct folder (Step #3 & #4 from above)

Error: "An error was encountered while loading the translation providers. One or more 
        translation providers will be unavailable until the problem is corrected. Message: 
		Microsoft.Multilingual.Translation.InvalidProviderConfigurationException: The 
		required credentials were not found in the Credential Manager service. at 
		Microsoft.Sample.Multilingual.Provider.TAUSDataProvider..ctor(String configFile)"
Fix:   Ensure the created TAUS DATA account information was added to the Credential 
       Manager (Step #5 from above)

Error: "An error was encountered while loading the translation providers. One or more 
        translation providers will be unavailable until the problem is corrected. Message: 
		System.Net.WebException: The remote server returned an error: (401) Unauthorized. 
		..."
Fix:   Ensure the correct TAUS DATA account information was added to the Credential 
       Manager (Step #5 from above)

Additional information
Links related to the Multilingual App Toolkit.
Installation: http://aka.ms/matinstall
Blog: http://aka.ms/matblog
User Voice: http://aka.ms/matvoice

Links related to TAUS
Main site: http://www.taus.net
TAUS DATA APIs: https://www.tausdata.org.  Registration is required, but is free.
