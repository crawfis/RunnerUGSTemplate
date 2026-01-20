# Clone Repository and Configure Unity Services and Git
#### [Made by Roger Crawfis with Scribe](https://scribehow.com/shared/Clone_Repository_and_Configure_Unity_Services_and_Git__RBAsMImDRqOJkfXoDVB3SA)


1\. Go to <https://github.com/crawfis/RunnerUGSTemplate>

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/55c9fde0-5346-45da-a020-73cb7cac3740/ascreenshot_f22429bb5fff44dd8e9766de81501b84_text_export.jpeg)


2\. Click the "Use this template" to automatically create your own repo based on this.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/76f6037b-3d7f-44a5-90f3-a89855c394e7/ascreenshot_f26124cbc2464d259c96d6a0959cd588_text_export.jpeg)


3\. Click "Create a new repository"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/d221cb9a-4f6e-4878-8bb4-19a79855a3ff/ascreenshot_09e3676f05274d8ca6c300df5dfcd846_text_export.jpeg)


4\. Click "Repository name *"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/d3e59389-316e-42e8-beb7-a3bd1c7f2352/ascreenshot_436e7f361582413b96566a7975fa5da5_text_export.jpeg)


5\. Click "Create repository"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/62657962-27a4-4a8e-a7a3-d5d6ebef9ba2/ascreenshot_a02066b7f2eb4dbc81906e71ebcde49b_text_export.jpeg)


6\. Open your IDE (Visual Studio 2026 shown here) and clone your new repository

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/f70a3900-1955-4287-94bb-6cf2c52bec90/ascreenshot_0081dbc066c94cf29eea74f71988285a_text_export.jpeg)


7\. In the Unity Hub add this repo as an existing project and open Unity (6.3 shown here)

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/4cde1176-d3d2-4363-8253-866e438bf063/ascreenshot_73ae0578f4ed4c58aeb5caea93d10109_text_export.jpeg)


8\. I add ProjectVersion.txt to my .gitignore file to prevent minor version changes prompting for changing versions when working with a team, but you need to do it once after you clone.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/4cdc1e58-c68a-41f5-ab88-1a2af4693ce3/ascreenshot_30e73d92a4c14ea49272d67dfbf6fdfc_text_export.jpeg)


9\. Unity is annoying and makes you confirm twice! Three times if you told it to open it with a specific version in the Hub :-(

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/cbac4431-f34a-4511-86c6-969ab2b9769b/ascreenshot_5fe2d3a513f14842a8d422fc6167374d_text_export.jpeg)


10\. Click on "File"-&gt; "Build Profiles" to bring up the Build Window

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/bb368482-20ad-470b-b188-84fb24d0fe42/ascreenshot_bc27a6cc9a2943edb431782bc6724ceb_text_export.jpeg)


11\. Select the "Windows" Build Profile (or create a similar one if on MacOS or Linux).

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/9b59b964-e6fc-4132-becb-1d88df6cf874/ascreenshot_90b9f57bc06e40428cd2cb97df340e44_text_export.jpeg)


12\. Click "Switch Profile" and then close this window when done.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/d463bc17-e73d-4d54-8615-ee33bc6f6c7b/ascreenshot_d51b9ec5d78c486898b786766c13404f_text_export.jpeg)


13\. Make sure that "CrawfisSoftware-&gt;Play Scene 0 Always" is checked. This will load the first scene in the build profile whenever you hit play. This is the same behavior as in a real build. It will restore your current scene when you exit play mode. The script is in the project under "Third Party/CrawfisSoftware/Editor".

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/b47d83af-34c1-48d7-b690-b2ccd6bd0d4f/ascreenshot_ac02407097d44a73a37f4866f7ab692a_text_export.jpeg)


14\. Also enable the "CrawfisSoftware-&gt;Events-&gt;Log Events" menu. This will print out any events from the underlying CrawfisSoftware/EventsPublisher package.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/4fb9612d-bd63-49b5-b1cf-3bb0649a4ebd/ascreenshot_a839a67c278d459cae06a2d910d185dc_text_export.jpeg)


15\. Test the game only framework.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/7fea3ab8-5fad-4acd-9ed2-7adb6b6dd027/ascreenshot_771b834134fb4909b1d83ae3765cc8bd_text_export.jpeg)


16\. Only "Play" and "Quit" work here. "Options" is a job for you (e.g., select a game difficulty and common options). "Sign Out" will work once we add in the Unity Gaming Services (UGS).

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/24c97808-1e49-4184-bc5d-9971620edd3b/ascreenshot_5178fbdba1814f748049e082a8705550_text_export.jpeg)


17\. Open Project Settings and change the Company name and Project name

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/e833b6df-2638-4e63-b0d8-3dfa6693b3de/ascreenshot_60d82dbf6de0413fbb5bc82d6cb0c3d9_text_export.jpeg)


18\. Also notice any #Define Symbols

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/a05ecad3-4aae-4ee4-ad8f-3c794fe0e40e/ascreenshot_18a6b120f2c04167ab222166a908025e_text_export.jpeg)


19\. Change your Root namespace to your desired namespace.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/8aa47ee8-3c0a-43a0-9f07-0a74b843c88e/ascreenshot_58dea6540b374f3eadc5eda9b3e28373_text_export.jpeg)


20\. Open the 0_BootStrap_Game_Only scene in the Test subfolder

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/20a961c5-c11a-473b-91ef-d30bd93f41f9/ascreenshot_61d5760c5be940a283ce7c2892168d14_text_export.jpeg)


21\. Notice how this scene loads other scenes.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/5bf51985-9f0a-4a40-bc5d-ee0d3285b02b/ascreenshot_ed09cee5494a41689562dc01cbd850a9_text_export.jpeg)


22\. Also notice the event logs from your test run

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/e1d09443-04cc-46be-bf7f-cb7876289c64/ascreenshot_e7e536cfec56443aa6881b41049c41bb_text_export.jpeg)


23\. Open the "Event Publisher Menu"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/eaef1401-0afd-420b-a7ce-c8d6f1b2959d/ascreenshot_cb9104cf33cf48058d9bdc587958844c_text_export.jpeg)


24\. Click "CrawfisSoftware.Events.Editor.EventMenu"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/d9d015b2-96ef-4b04-8d84-034b94f8142b/ascreenshot_2a7a03d52d1d4f129436742c5a476f7c_text_export.jpeg)


25\. Once you hit Play, the drop down list is populated with all of the possible (registered) events. You can select one and then hit the "Publish Event" button to fire the event.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/80d8ddcb-c7c8-41be-a3d8-604486698ef8/ascreenshot_6e7ce4a96c2f4651ac27b8463adef8fa_text_export.jpeg)


26\. Try "LeftTurnRequested" and notice it in the Console.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/9aa49095-2ea6-4ef3-a4fe-09e309ce4106/ascreenshot_0d76a4f2399a4f32852620634de90fbd_text_export.jpeg)


27\. The "QuitRequested" event may be particularly useful (image is slightly out of date).

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/a6072c5b-8bfb-40f3-8ff4-cb1d81e19984/ascreenshot_12b83e4275154629bf259686199b94be_text_export.jpeg)


28\. Create a new git branch for your changes, commit your changes and ask for a Pull Request from your teammates. Get used to this process even for simple changes like this. If you right-click in your git Changes panel on a file you can see the changes it is committing. Make sure you are not adding any files that should not be in the Repo. 

Also, you may need to change your .gitignore and remove the ProjectSettings.asset if you want to share your Unity project, organization and id.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/8c038d58-f516-4a29-ad3b-8eba7eb7d638/ascreenshot_59140a555f814819a908a48121a28b3f_text_export.jpeg)


29\. After the Pull Request, make sure you Fetch and Pull the changes and THEN switch back to main.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/ca1deae9-acec-40d6-a45d-ecfeb2ef14c3/ascreenshot_bc9a99b7a061487d8fc58a05936b6dbd_text_export.jpeg)


30\. Open Projects Settings and Select the "Services" item. This will show the General Settings for Unity Gaming Services.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/1b38dfbc-bedc-460c-8288-0748edbb4e35/ascreenshot_06e7e9b53b75446b9ea54d86a3195f2b_text_export.jpeg)


31\. Select your organization and Create a new Cloud Project.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/204bc956-c90a-4fce-8b77-81ff9e318b2b/ascreenshot_1fcff07c1f86420ab62ba8ec1c60257e_text_export.jpeg)


32\. Select the Environments and notice that there is one environment called production by default. We will create two more environments.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/5c7017cf-dbf9-4443-8f6a-06bb23dbf048/ascreenshot_4853f3dc86d44913a32c772a02b17bea_text_export.jpeg)


33\. Click the "Manage environments" to go to [cloud.unity.com](https://cloud.unity.com) in your web browser

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/fe9a5d06-e870-4a4f-bd30-d834fdd51954/ascreenshot_a0a76e26328945649f3b2334d05efb0b_text_export.jpeg)


34\. Click "Add environment"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/e7155bfe-58d5-4f5e-a21f-ae2618358cbc/ascreenshot_793e8a88b2ab4357ad19d41e7896c087_text_export.jpeg)


35\. Create an environment called "initial-development". Use this name exactly as that is what the code is initially looking for (or you will need to change it in the scenes that reference it).

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/59ad026d-7613-4181-bc96-948740d1ef01/ascreenshot_a301246e9e2548a49e9f2e6f1316a134_text_export.jpeg)


36\. Repeat to add an environment called development

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/36d40a5a-3d51-44f6-ae97-75be0dd24be7/ascreenshot_37152ac81e4244b98708a7b132dcc1ce_text_export.jpeg)


37\. Select "initial-development" to make it the current focus.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/4dfe695e-5f46-4436-b72b-e8745d65fedb/ascreenshot_5f77f66045364288a9e32e918716a5e3_text_export.jpeg)


38\. Back in Unity if you select the "Refresh" button you can see the new environments.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/6eddaf9a-d558-4d3f-8646-0ac9f7cb9237/ascreenshot_b0b661b423f74f6eb81fa6fc4fcfdff1_text_export.jpeg)


39\. Select "initial-development"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/d373705c-ded7-415c-9061-c3772d9b2a57/ascreenshot_40f0cd8bdae2468681b7f363d1db028d_text_export.jpeg)


40\. Open the UGS_Boot_0_Initialization scene

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/7cfe6365-489b-4a94-a42b-02ac147466e9/ascreenshot_a2e2218a9f3241019962c43a34771a91_text_export.jpeg)


41\. Select the InitializeServices GameObject

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/018d4495-c359-4930-9017-d77c2c94782a/ascreenshot_c22fe1c85b1749808d262d1e4a79fe17_text_export.jpeg)


42\. This GameObject has a Unity provided Component called ServicesInitialization. Make sure the current environment is "initial-development"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/53469c59-5a57-4d7f-95fb-4f082682670d/ascreenshot_45e6169ed76c4a1f892902d0da85498e_text_export.jpeg)


43\. You can see the Unity events that are wired up in this component.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/6203d3fe-b6e5-467f-809c-06f8e46d9c83/ascreenshot_1b4e0729173d4a9ca2fd58413901188c_text_export.jpeg)


44\. This project uses some Cloud Code to securely communicate with the leaderboards and claim achievements. To "deploy" this code, we need to generate solution files. Select the BlocksAdminModule in the CloudCode folder.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/222d2192-9bcd-46e1-8f96-7eaa03d7fb9f/ascreenshot_4cffab6274a34864b5f8135f8fb53459_text_export.jpeg)


45\. Click on the "Generate Solution" button.

Repeat for the BlocksGameModule.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/da95b018-9e9c-4158-ac95-48950c4d7ad3/ascreenshot_140d8702dadb4b9fb23994d3c3306c04_text_export.jpeg)

# Part 2 Continuation: Creating a Leaderboard
This from a second session with Scribe, but it reset the set numbers.

1\. Click "Deployment"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/59061be6-cc77-4fc4-be2e-f8bf44590d08/ascreenshot_d00547a25ef64dc8b774b46b671703be_text_export.jpeg)


2\. Click "UnityEditor.InspectorWindow"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/e4e3be92-8796-44ed-a3c5-1609322eb0d6/ascreenshot_7713e248c03a49ed946d2b321a8ac95d_text_export.jpeg)


3\. Click "C# Modules"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/9165d1ec-cb90-4fd0-bccb-071a2d9628af/ascreenshot_7b862ec283c84c739765d29d263b5468_text_export.jpeg)


4\. Click "TempleRunUGSCloud"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/b481ddef-2a19-4f04-a7c3-c487776f2ff6/ascreenshot_74de62469d594549946d369b92419b76_text_export.jpeg)


5\. Click here

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/4de313f0-589a-4778-bb56-5c20fa5e8002/ascreenshot_4b6484f01be84c7f8bdaed8e5777f407_text_export.jpeg)


6\. Click "BlocksAdminModule"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/3e96f432-f6a0-4532-9cee-2d78aba61efb/ascreenshot_78dd31b38a9a4603a6ee3e2eeb405733_text_export.jpeg)


7\. Click here

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/fbc259fd-f268-4706-92d2-b2dab1ebad0a/ascreenshot_d47b44c4ffdf4288b5ca006d911cb8e7_text_export.jpeg)


8\. Click "BlocksGameModule"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/cd149777-11bc-4924-84be-f0ad5217882e/ascreenshot_3c8855f3fb2c49df868cd647c126a5f8_text_export.jpeg)


9\. Click "AddPlayerScoreAsync"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/80056de2-ef6f-4e83-8dc5-4f62aa2abcc0/ascreenshot_9264ffaf3a7d43b78238e88765e3d064_text_export.jpeg)


10\. Click "GetAchievements"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/0e86527b-a310-4b85-8ccd-e656d21729e9/ascreenshot_a0d2759c068e4cddbd247a7b25b7d849_text_export.jpeg)


11\. Click "UnityEditor.PackageManager.UI.PackageManagerWindow"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/59444125-dc1a-4033-a9cb-2819cdaf7682/ascreenshot_7eb9ecee586e4c4893bded99acc4e19f_text_export.jpeg)


12\. Click "UnityEditor.PackageManager.UI.PackageManagerWindow"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/8f740e43-494b-499e-a765-8e6f21931ecb/ascreenshot_121763cf362a4ce2af15657bc118e32c_text_export.jpeg)


13\. Right click "Change - Example.cs \[\] - added"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/e9578382-7099-49e1-9e01-9062282a2a21/ascreenshot_946d953638ef410180f5a6ac9aeb21a6_text_export.jpeg)


14\. Click "Delete"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/96a45e25-3a78-4280-95d2-ec0f0b8ae1b8/ascreenshot_febbec3d50fb42ee8711ff5361c33a69_text_export.jpeg)


15\. Click "Comm_it All"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/6d76065b-ee20-4650-8fce-3b41d5db30e0/ascreenshot_0218bd6688324975a826208775642958_text_export.jpeg)


16\. Click "Comment"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/ad1532a2-3a62-43c6-b73c-942315a3ed42/ascreenshot_d72cfbd3c62a4b9f80e9ce67b6aa7daa_text_export.jpeg)


17\. Click "Services"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/4e212115-abb7-46fd-a888-01e1162496da/ascreenshot_703ddb011a364f98a83284c4b7d16c05_text_export.jpeg)


18\. Click "Authentication"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/2ec5f574-f0a5-4f78-a972-7d99014a354c/ascreenshot_95a4328bcc724661bb8cf0b72a8a75d9_text_export.jpeg)


19\. Click "Configure"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/217d0e4e-5638-4284-899b-3dd0dee0ad7e/ascreenshot_0b26c3f1b773453f8d795c318496227c_text_export.jpeg)


20\. Click "Username and Password"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/ff383208-81f3-41bd-aaa9-49b7a7c4f2e3/ascreenshot_c4145fa9448a4f37a550d3d1b4bf0b5e_text_export.jpeg)


21\. Click "Add" to add this authentication type to your project. We also want Unity login's, but that does not show up here.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/6e662917-6b66-4218-bf06-98a59910d419/ascreenshot_8e8a4c87fd85475897b257c6fdf5001c_text_export.jpeg)


22\. Click "Save"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/ce4a91a6-c9be-4923-9e09-9b4cecfbe58e/ascreenshot_742114dd247c4006b1ab2efb5028a79d_text_export.jpeg)


23\. Go back to your cloud dashboard and click "Add Identity Provider" in the Player Authentication page. You should see we have the Username & Password provider.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/c2659b44-1777-4b6a-a1ff-911d61033e1f/ascreenshot_fd9c1a212ff7451f8aed610edc04f62f_text_export.jpeg)


24\. Click "Unity Player Accounts"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/f1d7d9f0-603a-4c54-8538-353c6d314963/ascreenshot_4c572e9318ab46d1bca423ce1a2f57ba_text_export.jpeg)


25\. Click "iOS / Android"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/47eb1597-3517-470a-a6d8-811767e70168/ascreenshot_7c9c4751a83d45ab8b679758aa3ed079_text_export.jpeg)


26\. Click "PC"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/64f39d78-81cc-45ed-8767-867c7520581e/ascreenshot_a6392ead47c340a4a25756ba1caff7d4_text_export.jpeg)


27\. In order to publish you will need to provide a "Terms of Service link" and "Privacy Policy link"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/96b42655-3471-4529-8de3-d8f5f9ad7041/ascreenshot_79f1bb10a8264bc9a68b1223ac56ddc8_text_export.jpeg)


28\. Click "Add provider"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/f26bf4ec-8db0-4b54-a9d0-249b63d108f5/ascreenshot_c9887ea1d66c4378b3111151808ec72b_text_export.jpeg)


29\. If you do not see Player Authentication on your Shortcuts, click the "+" to add it.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/96363f7d-bba0-49e0-b16f-bbf13175a019/ascreenshot_d533528439214e72ada4035bee635dfa_text_export.jpeg)


30\. Click "Player Authentication"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/865f0d06-8a43-4a59-8bba-f96699bb8f16/ascreenshot_b9f13fb2ead648ada86975e4b187e7b8_text_export.jpeg)


31\. Switch the Build Profile to the "Test_UGS_Windows" profile and notice the difference in the Scene List.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/bc8df501-f849-4ddb-8954-2c48422370f9/ascreenshot_abe8f370f9db4b2e92192a83f4fc85fd_text_export.jpeg)


32\. Play will now go through the Unity Gaming Services with the following 3 login approaches. The "Sign in with Unity" will also let you sign in with Google or Apple. Note: these are different than Google Play and Apple Play.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/613c1f48-6bef-4fc2-ba17-4f47eea57147/ascreenshot_a0e3b7ba14bf405f8e4bc2aab5382d9a_text_export.jpeg)


33\. A Leaderboard scene should now load and then unload after 2 seconds. There will not be a score here yet since we haven't set one up yet. The "Self" is if you are not in the top 15 you can see where you stand.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/b62cab47-76b6-4eb6-a99e-4a4c0268181e/ascreenshot_22825043cda742beac5db54e5383f580_text_export.jpeg)


34\. The Achievements will also display for 2 seconds.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/13c6dce3-5925-4cb2-89c8-ac4e339f2a2a/ascreenshot_0c93d302b35648babd740b782a665044_text_export.jpeg)


35\. You should see this error in the Console.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/acba9870-48cf-44b8-b498-f595e777b6e6/ascreenshot_f52e8726319d4fc283e2b1d7e905b55c_text_export.jpeg)


36\. You can test "Sign Out" button and logging in with different providers.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/e4ea5b54-958d-4b59-b714-f5b5ca74f9a7/ascreenshot_c3b3ac6679364a328a3420276d81dd55_text_export.jpeg)


37\. Test "Sign in with Unity"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/da878dd2-6bfc-4c6a-9f04-56d43e1596bc/ascreenshot_7af209511159466f936fa8f670148169_text_export.jpeg)


38\. Unity's page for signing in.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-20/e01fe978-3a85-4cc3-a5b5-3170bc5a20c6/user_cropped_screenshot_55de71b437164466983fc52aca90d0df_text_export.jpeg)


39\. You should see the Unity Player Account and Username and Password in the ID Providers. It may take some for the provider to show up and work (at least for me).

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/149dc932-7a05-449d-b616-77a4731e6b57/ascreenshot_61f160e2a0fd4d35922e84a6ee59b004_text_export.jpeg)


40\. The Username & Password dialogue is a little funky. You put in your information and then click "Sign Up".

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/67bc01e5-a7df-451d-b488-26af508e5da3/ascreenshot_c8c66f7226704b8c9140840b58d64146_text_export.jpeg)


41\. They do have built in password restrictions, but this may be in the sample code.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/a1c70851-c166-4bc7-ac99-c6617baeb824/ascreenshot_409f3ab860f04c6896708de60d604279_text_export.jpeg)


42\. On you cloud dashboard go to the Leaderboards page. You should see a Simple Leaderboard already. This was created in the Deployment but is not used. You can look at the deployment file within the Unity Leaderboard Building Block code. Before we create our own, make sure the initial-development environment is selected.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/57f3f528-0e90-42f1-a6d9-48dad72abeea/ascreenshot_30bbdd2de4974bd6afe47a9ab1ed46e2_text_export.jpeg)


43\. Click "Add Leaderboard"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/301100bc-7737-4c06-94ee-fcc61887a93c/ascreenshot_21e6345c8cae4d718a31c8650cfd6de5_text_export.jpeg)


44\. Name this DailyDistance and click "Next"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/ae0a05bd-0c75-4056-9429-d436076b8312/ascreenshot_62a47e08ab48446aa46fb68dcc0174f0_text_export.jpeg)


45\. Click "Highest to lowest"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/86bd2d36-d583-492a-946f-940f002ca44a/ascreenshot_7c8aab54abf840d480387cb80a0fdbc6_text_export.jpeg)


46\. Click "Best score" which updates scores when a new score is submitted with a better value (higher or lower depending on the selected sort order)."

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/ebf3cfa6-6dbf-4aa8-abc7-c1664d5bc7f5/ascreenshot_351bfaa0a6e141ada1688246972d52ee_text_export.jpeg)


47\. Select Yes for bucketing the leaderboards. In the "Max players per bucket" set this to something like 200

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/a88696ef-96e3-4a51-bf1f-5239deabb186/ascreenshot_bb38b166ded74236b3d291e0ad75a1dd_text_export.jpeg)


48\. Click "Next"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/1838a9e6-4cd2-4e1f-a486-9fac30332db8/ascreenshot_d275cfff04964cbc841353aaf08e8866_text_export.jpeg)


49\. For Scheduled Resets, select "Yes"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/9c9fe246-6e81-4510-a8dc-125743e1bea1/ascreenshot_f46cb690b4be49b8a72cb596f08cb011_text_export.jpeg)


50\. Click "Recurring reset Leaderboard will continuously reset."

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/9c1d31f7-cf62-464c-b378-3ac31c95ef1e/ascreenshot_2567341dd30f45b2853e1f3d7ceaaa4c_text_export.jpeg)


51\. For testing we will make this a Monthly reset and Monthly high score. "Choose date" and set it sometime in the future. Click "Next"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/f33653de-548f-4894-a9b3-270ce2c809c0/ascreenshot_a3be8b54e1f24cdd8595ae8951e041f2_text_export.jpeg)


52\. Click "Finish"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/dc4a7efd-c782-4720-ad54-9f4ef89ba051/ascreenshot_88d8acb96e7b4485bc721aa6b3c96222_text_export.jpeg)


53\. Go back to Unity and start the app and then select Play

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/30ef9070-4231-4d70-8a3c-6cb57b0ead0b/ascreenshot_de750fb71c2942c9af6551cca39cccd4_text_export.jpeg)


54\. Leaderboards may still not work and you may see an error about access violation. We deployed the access control earlier and now need to disable it.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/a05a782f-db43-43ec-8cd4-56bf7e89fbeb/ascreenshot_571ceb4071c540bb9ef6b9bf652765d7_text_export.jpeg)

# Part 3 Continuation: Delete Remote Deployment from Unity Services

1\. Open the "Deployment" window

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/65e4bf9f-ef11-43e8-a313-eef97b70849b/ascreenshot_5cc2e85d9e924123b045638b9c8fc3e3_text_export.jpeg)


2\. Right mouse-click on the LeaderboardAccessControl and click "Delete Remote"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/9d3742aa-1ec2-47a9-b952-6646e202a84f/ascreenshot_662b166ce750442fb8804c586c8b6d3d_text_export.jpeg)


3\. Click "Yes"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/c071b3c7-b65b-41b4-8cb5-b45daae9740e/ascreenshot_5efdde81443047e9999f9a7027f894d2_text_export.jpeg)


4\. Now when you play you start to see some scores.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/9f394df2-edc3-4114-b034-1ed59646df9a/ascreenshot_b93ddb1c141c40e6a94065c4ebbf9916_text_export.jpeg)


5\. Click "UnityEditor.GameView"

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/3e6ae55d-4dfd-4f63-8eeb-636861648565/ascreenshot_e909ce62a74040299c0bc91a36405f30_text_export.jpeg)


6\. Sign in Anonymously

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/f77ad2f9-ddd8-4866-bd0a-d21fbc469163/ascreenshot_b26707cf8da54edd93033da4e54c01f5_text_export.jpeg)


7\. And Play again

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/5f37c386-7eed-4ef2-8b95-99a818168e04/ascreenshot_f42441bd781c414fa0f42528f34b3974_text_export.jpeg)


8\. Now you should see multiple players scores.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/d046fa2f-1628-4aef-8f52-4cb0e3f9484f/ascreenshot_f29b066b08644d66a0941a7070b9d2e6_text_export.jpeg)


9\. The Console will have events to update the score.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/766b500e-886b-4bc8-b902-48a4f62604ed/ascreenshot_94d8ef005d79478ba06205450da056ab_text_export.jpeg)


10\. With many of the events, if you select it in the Console, the next line will list some data associated with it. In this case, we have the player name, score and player id for the ScoreUpdated event.

![](https://colony-recorder.s3.amazonaws.com/files/2026-01-18/31324d44-cc3a-4431-b5b1-7be65cd79d05/ascreenshot_30c5384f714547fb93468482fab79a4a_text_export.jpeg)
#### [Made with Scribe](https://scribehow.com/shared/Delete_Remote_Deployment_from_Unity_Services__aB4pKrB5Ta6LXpS3bbV9qQ)



#### [Made with Scribe](https://scribehow.com/shared/Clone_Repository_and_Configure_Unity_Services_and_Git__RBAsMImDRqOJkfXoDVB3SA)


