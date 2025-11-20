# Achievements

This Unity Building Block proposes an implementation for an achievement system powered by the Unity Gaming Services.
The achievements are synchronized and persisted in the cloud, recoverable, cross-device, and cross-platform.

## Services and Usage

This block makes use of five services
- [Authentication][auth]
- [Cloud Save][cloud-save]
- [Cloud Code][cloud-code]
- [Remote Config][remote-config]
- [Access Control][access]

### Authentication
The [Authentication service][auth] is required for most services as it provides a unique identity to the player.
This unique indentifier allows their data and experience to be personalized.

This block uses [Authentication][auth] to sign in anonymously.

### Cloud Save
The [Cloud Save][cloud-save] service provides the capacity to persist JSON data, and game files.
It also allows to save arbitrary game data that can be read by any user, which can be leveraged to create
features such as clans, guilds, and other global game state.

This block uses [Cloud Save] to store your progress on which achievements you have claimed.
See more in the [Cloud Save data](#cloud-save-data) section.

### Cloud Code
The [Cloud Code service][cloud-code] allows you to create code that will be run in the cloud. 
This code has the ability authenticate as either the server or the client depending on your needs and interact with other services. 

This block uses [Cloud Code][cloud-code] to demonstrate server authoritative actions to claim achievements.

### Remote Config
The [Remote Config service][remote-config] allows you to store values as key-value pairs on the cloud as JSON file.

This block uses [Remote Config][remote-config] to define the achievements on the cloud under the `"achievements"` key.
See more in the [Remote Config data](#remote-config-data) section.

### Access Control
The [Access Control][access] service allows you to determine authentication requirements for interacting with other services.

This block uses [Access Control][access] to restrict players from being able to write to Cloud Save.
In order to still write to Cloud Save, you can switch to using the trusted client, which uses Cloud Code Modules to authenticate the calls as a trusted source.

## Prerequisites

Here are a few prerequisites in order to have a proper onboarding experience with the Achievements block.

### Project linked to a dashboard project

The project must be linked to a Unity Cloud project. To do so:

1. Open the project settings
2. Navigate to the `Services` section
3. If no project is linked here, please link it to an existing project or create a new project in your organization.

### Environment set

When using Unity Cloud functionalities in general, the application will target a specific environment
within the Cloud project linked. To select a target environment:

1. Make sure a project is linked (see [above](#project-linked-to-a-dashboard-project))
2. Open the project settings
3. Navigate to the `Services/Environments` section
4. Select the desired environment 

Environment management such as creation and deletion must be done from the Cloud dashboard.

## Block Contents

### Deployment

Deployment is the act of creating or updating service configuration which can be found in the Unity Cloud.
This an important operation in managing environments with [configuration as code][env-mgmt].

The deployable contents for this block can be found in the `Blocks/Achievements/Deployment` folder. 
Deploying them can be done like so:

1. Open the Deployment window using the top menu: either `Window -> Deployment` or `Services -> Deployment` depending
   on the version of Unity used.
2. In the Deployment window, check the top-level `Blocks.Achievements` and `Blocks.CloudCode` items.
3. Click `Deploy Selected`.

Any modifications made to these requires them to be deployed once again in order to have the changes appear in-game.

[env-mgmt]:https://docs.unity.com/ugs/manual/overview/manual/service-environments#environment-management-tooling

#### Achievements.ach

The `Achievements.ach` file is an implementation of a Remote Config (`.rc`) file, with a custom inspector.
Although the extension is different, this file belongs to the Remote Config service and will act as other Remote Config files in terms of deployment.

This result is achieved by implementing the following:
- `AchievementAsset`: the scriptable object in the project
- `AchievementFile`: the representation of the contents of the file
- `AchievementAssetImporter`: the custom importer which allows us to have a custom inspector
- `AchievementAssetInspector`: the custom inspector

#### AchievementsAccessControl.ac

The `AchievementsAccessControl.ac` is configured to deny access to the Cloud Save service for any calls using player authentication.
Deploying this file will require you to submit scores using the Cloud Code modules below.

In the test scene: make sure the `Use Trusted Client` on the `AchievementsPreview` GameObject is set to `true`.
Outside of the test scene: make sure the   

#### Cloud Code modules

This block makes use of Cloud Code C# modules to interface with the services. 
These modules can be found at `Assets/CloudCode/BlocksAdminModule.ccmr` and `Assets/CloudCode/BlocksGameModule.ccmr`.

### Prefabs

This block contains two prefabs: `AchievementsPrefab` and `AchievementsNotificationPrefab`.
These are located in the `Assets/Blocks/Achievements/Prefabs` folder.

`AchievementsPrefab` is a drag & drop solution to view your collection of achievements defined in your `Achievements.ach` file.
This prefab has 3 driving properties that can be selected on the prefab instance in the inspector:
- `Init on Awake`: should this prefab initialize itself on awake? If so, it will use the below properties to do so
- `Development Mode`: when toggled on, achievement cards will contain controls helpful for testing the achievements such as unlocking and increasing/decreasing progress
- `Use Trusted Client`: creates `LocalAchievementsClient` when toggled off, uses `TrustedAchievementClient` when toggled on
- `Icons`: The icons that are available to use in the achievement cards

`AchievementsNotificationPrefab` is a drag & drop solution for a notification when an achievement is unlocked/claimed.

### Test Scene

The `AchievementsTestScene.unity` scene located at `Assets/Blocks/Achievements/TestsScenes/` provides a location to 
load and display the achievements using the `AchievementPrefab`.

To start, a loading screen is displayed until the achievements are retrieved from the Cloud Services. 
The loading screen will then disappear, and reveal your achievements as defined in your deployed `Achievements.ach` file.

Each element represents an achievement which you can view. If the achievement has not yet been claimed, you can unlock with the `Unlock` button.
If the achievement is defined as "hidden", then the icon will be different and the description omitted until it is claimed.

The `Reset Achievements` button in the top left allows you to iterate with your achievements and test them out easily.

## Add Achievements to a scene

In order to add the achievements to a new or existing scene you will need the following
1) Make sure Unity Services are initialized and Authentication service has been signed in
   1) The `UnityServices` prefab at `Assets/Blocks/Common/UnityServices.prefab` will initialize on start and sign in anonymously
   2) You can also do this via code with `UnityServices.Instance.InitializeAsync()` and `AuthenticationService.Instance.SignInAnonymouslyAsync()`
2) Add the `AchievementsPrefab` to the scene
3) (Optional) Add the `AchievementsNotificationPrefab` to the scene
   1) Note, you may want this specific prefab to be part of different hierarchy/scene in order for it to persist outside of the achievements scene

## Modifying the existing achievements

The achievements are configured in the `Achievements.ach` configuration file located at `Assets/Blocks/Achievements/Deployment/Achievements.ach`.
This file is written in JSON, and can be modified directly or using the custom inspector.

> [!NOTE]
> The changes to the Achievements definition won't be taken into account unless this file is saved then deployed to the
> current environment.

## Remote Config data

The `Achievement.ach` file that is driving the definition of the achievements is located at `Assets/Blocks/Achievements/Deployment/Achievements.ach`.
It follows the [Remote Config file specification](https://docs.unity3d.com/Packages/com.unity.remote-config@3.3/manual/Authoring/remote_config_files.html) (reminder, the `.ach` file is really an `.rc` file with a specific inspector). 

Its payload is a key value store, and in this specific file it consists of only one key: `"achievements"`.
The `achievements` data represents an array of items that must fit the `AchievementDefinition` data model defined both on the game side and on
the Cloud Code side:

- Game side: `Assets/Blocks/Achievements/Scripts/Runtime/AchievementDefinition.cs`
- Cloud Code: `Assets/CloudCode/BlocksGameModule~/Project/Achievements/AchievementDefinition.cs`

> [!NOTE]
> It is imperative that both model match exactly, and that the deployed Remote Config file is matching the schema.

The current schema is the following:

```cs
public class AchievementDefinition
{
    public string Icon;         // name of the icon that illustrates the achievement
    public string Id;           // ID of the achievement (must be unique within a project/environment)
    public string Title;        // title displayed in the header of the achievement UI element
    public string Description;  // description of the achievement
    public bool IsHidden;       // is the achievement visible by the player when locked
    public int ProgressTarget;  // is the achievement progress trackable, and if so, what is the max amount of progress
}
```

> [!NOTE]
> The actual data stored is an array of objects of type `AchievementDefinition`

## Cloud Save data

To store and track achievement progress for each player the Cloud Save service is used. For each known player a key/value store will be assigned and saved to protected store.
For each player data will be stored in the `"achievements"` key.
Here is an example of achievement progress stored for a player:

```json
[
  {
    "Id": "first_achievement", // the ID of the achievement
    "Unlocked": true // the unlocked status of the achievement for the current player"
    "ProgressCount": 0 // the current progress relative to the ProgressTarget of the definition
  }
]
```

This data is also driven by a class, both present on the runtime side and the Cloud Code side:

- Runtime model: `Assets/Blocks/Achievements/Scripts/Runtime/AchievementRecord.cs`
- Cloud Code model: `Assets/CloudCode/BlocksGameModule~/Project/Achievements/AchievementRecord.cs`

> [!NOTE]
> The actual data stored is an array of objects of type `AchievementRecord`

## Data Access

As mentioned in the [Access Control](#achievementsaccesscontrolac) section, this will block your player clients from being able to write to Cloud Save.
This is done to keep the data in the same location independent of the client you are chooisng to use.

Alternatively, you could choose to write to the Protected [Access Class](https://docs.unity.com/ugs/en-us/manual/cloud-save/manual/concepts/player-data#access-classes) instead.
This Access Class always requires server authentication in order to write, which would bypass the need for the Access Control policy altogether.

## Code Examples

To help integrate the `AchievementsPrefab` into your project, here are some examples on how to interact with it in code

### Initializing the prefab

The `AchievementsPrefab` can be set to initialize on `Awake` .
Otherwise, you can choose to initialize the prefab yourself.

```csharp
// Initializing with the settings from the prefab instance

AchievementsPrefab m_AchievementsPrefab;

public void InitializeAchievementsPrefab()
{
    m_AchievementsPrefab.Initialize();    
}
```

```csharp
// Initializing with client choice and different root UI
// This is the initialization done in the Test Scene to make sure the client selection from the `NumberGameManager` is the same on the `AchievementsPrefab`

AchievementsPrefab m_AchievementsPrefab;
bool m_UseTrustedClient;
UIDocument m_RootUI;

public void InitializeAchievementsPrefab()
{
    m_AchievementsPrefab.Initialize(m_UseTrustedClient, m_RootUI.rootVisualElement);    
}
```

### Unlocking an achievement

Unlocking an achievement is required to show the progress your user has made.
Doing so requires either the ID of the achievement or a reference to the achievement directly.

```csharp
// If you know the ID of the achievement that should be unlocked, you can use the ID

public async Task UnlockAchievmeentAsync()
{
    await AchievementsObserver.Instance.UnlockAchievementAsync("MyAchievementId");
}
```

```csharp
// If you have a reference to the specific achievement, you can use the reference

public async Task UnlockAchievementAsync()
{
    AchievementsObserver.Instance.UnlockAchievementAsync(achievement);
}
```



[cloud-save]:https://docs.unity.com/ugs/en-us/manual/cloud-save/manual
[auth]:https://docs.unity.com/ugs/en-us/manual/authentication/manual/get-started
[cloud-code]:https://docs.unity.com/ugs/en-us/manual/cloud-code/manual
[remote-config]:https://docs.unity.com/ugs/en-us/manual/remote-config/manual/WhatsRemoteConfig
[access]:https://docs.unity.com/ugs/en-us/manual/overview/manual/access-control
