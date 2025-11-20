# Leaderboards

This Unity Building Block proposes an implementation for an Leaderboard UI using the Leaderboard service.

## Services and Usage

This block makes use of four services
- [Authentication][auth]
- [Leaderboards][leaderboards]
- [Cloud Code][cloud-code]
- [Access Control][access]

### Authentication
The [Authentication service][auth] is required for most services as it provides a unique identity to the player.
This unique indentifier allows their data and experience to be personalized.

This Block uses [Authentication][auth] to sign in anonymously.

### Leaderboards
The [Leaderboards][leaderboards] service provides the backend for storing and retrieving player scores in as a leaderboard format.

This block uses [Leaderboards][leaderboards] to store your player scores and display it with a drag & drop prefab.

### Cloud Code
The [Cloud Code service][cloud-code] allows you to create code that will be run in the cloud.
This code has the ability authenticate as either the server or the client depending on your needs and interact with other services.

This block uses [Cloud Code][cloud-code] to demonstrate server authoritative actions to store & retrieve player scores.

### Access Control
The [Access Control][access] service allows you to determine authentication requirements for interacting with other services.

This block uses [Access Control][access] to restrict players from being able to write to Leaderboards. 
In order to still write to Cloud Save, you can switch to using the trusted client, which uses Cloud Code Modules to authenticate the calls as a trusted source.

## Prerequisites

Here are a few prerequisites in order to have a proper onboarding experience with the Leaderboards block.

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

The deployable contents for this block can be found in the `Blocks/Leaderboards/Deployment` folder.
Deploying them can be done like so:

1. Open the Deployment window using the top menu: either `Window -> Deployment` or `Services -> Deployment` depending
   on the version of Unity used.
2. In the Deployment window, check `Leaderboards.lb` and the top-level `Blocks.CloudCode` items.
3. Click `Deploy Selected`.

Any modifications made to these requires them to be deployed once again in order to have the changes appear in-game.

[env-mgmt]:https://docs.unity.com/ugs/manual/overview/manual/service-environments#environment-management-tooling

#### Leaderboards.lb

The `Leaderboards.lb` file is a configuration file for the Leaderboards service. 
It allows you to define the rules of your leaderboard in the Editor either via the inspector or modifying the file directly (uses JSON format).

#### LeaderboardsAccessControl.ac

The `LeaderboardsAccessControl.ac` is configured to deny access to the leaderboard service for any calls using player authentication.
Deploying this file will require you to submit scores using the Cloud Code modules below. 
To do so, make sure the `Game Client Type` on the `NumberGame` GameObject in the test scene is set to the value `TRUSTED`.

#### Cloud code modules

This block makes use of Cloud Code C# modules to interface with the services.
These modules can be found at `Assets/CloudCode/BlocksAdminModule.ccmr` and `Assets/CloudCode/BlocksGameModule.ccmr`.

### Prefab

This block contains a single prefab: `Leaderboard.prefab`. This is located in the `Assets/Blocks/Leaderboards/Prefabs` folder.

This prefab is a drag & drop solution to view your Leaderboard defined in your `Leaderboards.lb` file. 
By default it is empty. Once scores are submitted, they will be available to view the prefab.
The leaderboard has 2 tabs: `Global` for all scores and `Self` to display your score and the entries immediately above & below.

### Test Scene

The `Leaderboards.unity` scene located at `Assets/Blocks/Leaderboards/TestsScenes/` provides a location to
load and display the leaderboard using the `Leaderboards` prefab.

The scene starts with a game that asks you to guess a number and will score you based on how close your guess is. 
This score will be submitted to the leaderboard and will then be viewable.

The `Populate Leaderboard` button will fill your leaderboard with randomly generated scores so you can get a better view
of what the leaderboard looks like when populated.

The button in the top right allows you to swap between the "game" view and the leaderboard view.

## Add Leaderboards to a scene

In order to add the leaderboards to a new or existing scene you will need the following
1) Make sure Unity Services are initialized and Authentication service has been signed in
    1) The `UnityServices` prefab at `Assets/Blocks/Common/UnityServices.prefab` will initialize on start and sign in anonymously
    2) You can also do this via code with `UnityServices.Instance.InitializeAsync()` and `AuthenticationService.Instance.SignInAnonymouslyAsync()`
2) Add the `LeaderboardPrefab` to the scene
   1) The default setup will automatically initialize this prefab 

## Leaderboard Clients

The Leaderboards block comes with 2 types of clients:
- Leaderboard clients 
- Number Game clients

### Leaderboard Clients

The Leaderboard clients provide the required interface to fetch and set scores in the Leaderboard service.
These clients are part of the `LeaderboardsObserver` singleton and are used to display and update the scores in the `LeaderboardPrefab`.

To choose which client you would like to use, set the value you want on the `Use Trusted Client` checkbox on the `LeaderboardPrefab`.

### Number Game Clients

The Number Game clients are used to play the number guessing game in the `Leaderboards` test scene.
These clients showcase how to interact with the Leaderboard client either locally or through the `BlocksGameModule` Cloud Code Module.

To choose which Client you would like to use, change the value of `Game Client` on the `NumberGame` GameObject in the test scene.

> [!NOTE]
> The Number Game client will override the selected option for the `LeaderboardPrefab`.

> [!NOTE]
> `LOCAL` Client will not work if you have deployed the `LeaderboardsAccessControl.ac` file. 
> To undo this, you can right click the `LeaderboardsAccessControl.ac` file in the Deployment window and select the `Delete Remote` option.

## Code Examples

Here are some examples to help you integrate the `LeaderboardPrefab` into your own project.

### Submitting a score to the leaderboard

Submitting a score is the basic requirement for seeing scores appear in the `LeaderboardPrefab`.  

```csharp
// This option uses with the `LeaderboardsObserver` singleton

async Task SubmitScore(int score)
{
    await LeaderboardsObserver.Instance.AddPlayerScoreAsync(score);
}
```

```csharp
// This option uses the `LeaderboardPrefab` to add a score
// Note that this option will invoke the AddPlayerScoresAsync from the singleton, it is a convenience if you have a reference to the prefab

LeaderboardPrefab m_LeaderboardPrefab;

async Task SubmitScore(int score)
{
    await m_LeaderboardPrefab.AddPlayerScoreAsync(score);
}
```

### Updating the scores in the leaderboard

The block will automatically refresh the scores after a score is submitted. 
If you would like to update the scores outside of that timing, you can do so with the following:

```csharp
// This option uses with the `LeaderboardsObserver` singleton

async Task UpdateLeaderboards()
{
    await LeaderboardsObserver.Instance.UpdateScoresAsync();
}
```

```csharp
// This option uses the `LeaderboardPrefab` to add a score
// Note that this option will invoke the UpdateScoresAsync from the singleton, it is a convenience if you have a reference to the prefab

LeaderboardPrefab m_LeaderboardPrefab;

async Task UpdateLeaderboards()
{
    await m_LeaderboardPrefab.UpdateScoresAsync();
}
```

### Choosing which leaderboard to show

In the event your product has many leaderboards, it is possible to choose which leaderboard to fetch and submit scores to.

```csharp
public void SetLeaderboardId(string leaderboardId)
{
    LeaderboardsObserver.Instance.LeaderboardId = leaderboardId;
}
```

### Using the `LeaderboardPrefab`

The `LeaderboardPrefab` is set up to be able to run on `Awake()` which will use its `Leaderboard Id` and `Use Trusted Client` values to get running.
If you'd like to manually initialize the leaderboard yourself, there are a few options available to you.

```csharp
// Initializing with only client choice
// this will use the m_LeaderboardId from the `LeaderboardPrefab` script
// This option is used in the Test Scene to override the client choice on the prefab with the client choice from the test scene

LeaderboardPrefab m_LeaderboardPrefab;
bool m_UseTrustedClient;

void InitializeLeaderboard()
{
    m_LeaderboardPrefab.Initialize(m_UseTrustedClient);
}
```

```csharp
// Initializing with no parameters
// This will use the m_LeaderboardId and m_UseTrustedClient from `LeaderboardPrefab`
// Should be used if you do not want to rely on Awake to Initialize the leaderboard for you

void InitializeLeaderboard()
{
    m_LeaderboardPrefab.Initialize();
}
```

[auth]:https://docs.unity.com/ugs/en-us/manual/authentication/manual/get-started
[leaderboards]:https://docs.unity.com/ugs/en-us/manual/leaderboards/manual/leaderboards
[cloud-code]:https://docs.unity.com/ugs/en-us/manual/cloud-code/manual
[access]:https://docs.unity.com/ugs/en-us/manual/overview/manual/access-control
