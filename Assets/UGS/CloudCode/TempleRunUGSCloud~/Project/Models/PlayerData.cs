using Newtonsoft.Json;
namespace TempleRunUGSCloud.Models;

public class PlayerData
{
    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }

    public PlayerData(string displayName)
    {
        DisplayName = displayName;
    }

    public PlayerData() : this("") { }
}

public class ProfilePicture
{
    [JsonProperty("type")]
    public string? Type { get; set; } = "pre-made";
    [JsonProperty("imageData")]
    public string? ImageData { get; set; }
    [JsonProperty("imageId")]
    public int ImageId { get; set; }
}

public class ProfilePictureChangeRequest
{
    public string? Type { get; set; }
    public string? ImageData { get; set; }
    public int ImageId { get; set; }
}

/// <summary>
/// Consolidated response for player initialization that includes all necessary data
/// to set up the player's game state in a single call.
/// </summary>
public class PlayerInitializationResponse
{
    [JsonProperty("playerData")]
    public PlayerData PlayerData { get; set; } = new PlayerData();

    [JsonProperty("economyData")]
    public PlayerEconomyData EconomyData { get; set; } = new PlayerEconomyData();

    [JsonProperty("profilePicture")]
    public ProfilePicture? ProfilePicture { get; set; }

    [JsonProperty("isNewPlayer")]
    public bool IsNewPlayer { get; set; }

    [JsonProperty("initializationTimestamp")]
    public long InitializationTimestamp { get; set; }
}

/// <summary>
/// Lightweight response for when only basic player data is needed
/// (used for connectivity sync, simple updates, etc.)
/// </summary>
public class PlayerDataSyncResponse
{
    [JsonProperty("playerData")]
    public PlayerData PlayerData { get; set; } = new PlayerData();

    [JsonProperty("economyData")]
    public PlayerEconomyData EconomyData { get; set; } = new PlayerEconomyData();

    [JsonProperty("lastUpdateTimestamp")]
    public long LastUpdateTimestamp { get; set; }
}