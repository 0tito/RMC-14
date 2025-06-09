using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.Pinging;

public sealed partial class PingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;


    /// <summary>
    ///     Localization string for the ping name. Displayed in the radial UI.
    /// </summary>
    [DataField(required: true)]
    public string Name = default!;

    /// <summary>
    ///     Different ping categories may be handled by different systems.
    ///     Also may be used for filtering.
    /// </summary>
    [DataField]
    public PingCategory Category = PingCategory.General;

    /// <summary>
    ///     An icon used to visually represent the ping in radial UI.
    /// </summary>
    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new("/Textures/_RMC14/Actions/pings.rsi/question.png"));


    /// <summary>
    ///     Determines conditions to this ping be available to use
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    ///     Determines conditions to this ping be unavailable to use
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;


}

/// <summary>
///     Ping category. Might be used later
/// </summary>
[Flags]
[Serializable, NetSerializable]
public enum PingCategory : byte
{
    General,
    Leader,
    Queen,
}
