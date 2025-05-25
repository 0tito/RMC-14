using System.Diagnostics;
using System.Linq;
using Content.Client._RMC14.Xenonids.UI;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Strain;
using Content.Shared._RMC14.Xenonids.Watch;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.LineEdit;

namespace Content.Client._RMC14.Xenonids.Watch;

[UsedImplicitly]
public sealed class XenoWatchBui : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;

    private static readonly Comparer<EntityPrototype> EntityComparer =
        Comparer<EntityPrototype>.Create(static (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

    private static readonly Color XenoColor = Color.FromHex("#800080");
    private static readonly Color XenoHighlightColor = Color.FromHex("#9E379E");


    private SortedDictionary<int, SortedSet<EntityPrototype>> tiers = new();
    private Dictionary<string, bool> ShownXenos = new();
    private Dictionary<string, int> XenoCounts = new();

    private bool UsingSearchBar = false;
    private string SearchBarText = string.Empty;

    [ViewVariables]
    private XenoWatchWindow? _window;

    private readonly SpriteSystem _sprite;

    public XenoWatchBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _sprite = EntMan.System<SpriteSystem>();
    }

    protected override void Open()
    {
        base.Open();
        _window = EnsureWindow();


        foreach (var entity in _prototype.EnumeratePrototypes<EntityPrototype>())
        {
            if (entity.Abstract || !entity.TryGetComponent(out XenoComponent? xeno, _compFactory))
                continue;

            if (entity.TryGetComponent(out XenoStrainComponent? strain, _compFactory) || xeno.Hidden)
                continue;

            if (!tiers.TryGetValue(xeno.Tier, out var xenos))
            {
                xenos = new SortedSet<EntityPrototype>(EntityComparer);
                tiers.Add(xeno.Tier, xenos);
            }
            xenos.Add(entity);
        }

        foreach (var (tier, xenos) in tiers.Reverse())
        {
            if (tier > 3) // i dont think queen shows on the ss13 one
                continue;

            var row = new XenoTierRow();

            if (tier == 2 || tier == 3)
                row.SlotsLeft.Visible = true;

            row.TierButton.OnToggled += OnTierToggled;
            row.SetInfo(0, tier);
            row.Tier = tier;
            foreach (var xeno in xenos)
            {
                if (xeno.TryGetComponent(out XenoComponent? xenocomp, _compFactory))
                    if (!xenocomp.ShowInWatchWindowCounts)
                        continue;

                var xenocontrol = new XenoHiveCountControl();
                var xenobutton = new Button();

                xenobutton.Text = xeno.Name;
                xenobutton.Name = xeno.Name.Replace(" ", "");
                xenobutton.ToggleMode = true;
                xenobutton.ModulateSelfOverride = Color.Purple;
                xenobutton.AddStyleClass("ButtonSquare");

                xenobutton.OnToggled += OnButtonToggled;
                xenocontrol.XenoButton.AddChild(xenobutton);

                row.XenosContainer.AddChild(xenocontrol);
                ShownXenos.Add(xeno.Name.Replace(" ", ""), false);
                XenoCounts.Add(xeno.Name.Replace(" ", ""), 0);

            }
            _window.RowContainer.AddChild(row);
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not XenoWatchBuiState s)
            return;

        _window = EnsureWindow();
        _window.BurrowedLarvaLabel.Text = $"Burrowed Larva: {s.BurrowedLarva}";
        _window.XenoCount.Text = $"Total Sisters: {s.XenoCount}";
        _window.XenoContainer.DisposeAllChildren();

        var icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_RMC14/Interface/xeno_leader.rsi/"), "hudxenoleader");
        var iconTexture = _sprite.Frame0(icon);

        var xenolist = s.Xenos
            .OrderByDescending(a => a.RoleId.Equals("CMXenoQueen"))
            .ThenBy(a => a.RoleId)
            .ThenByDescending(a => a.Leader);

        foreach (var key in XenoCounts.Keys)
        {
            XenoCounts[key] = 0;
        }


        var burrowed = Math.Sqrt(s.BurrowedLarva * s.BurrowedLarvaSlotFactor);
        FixedPoint2 burrowedweight = Math.Min(burrowed, s.BurrowedLarva);
        var total = s.XenoCount + burrowedweight;


        var tier2Slots = Math.Max((float)(total * 0.5f - s.TierTwoAmount * s.TierThreeAmount), 0);
        var tier3Slots = Math.Max((float)(total * 0.2f - s.TierThreeAmount), 0);


        foreach (var xeno in xenolist)
        {
            Texture? texture = null;
            if (xeno.Id != null &&
                _prototype.TryIndex(xeno.Id.Value, out var evolution))
            {
                texture = _sprite.Frame0(evolution);
            }


            var name = xeno.Name;
            var control = new XenoChoiceControl();
            control.Set(name, texture);

            if (XenoCounts.ContainsKey(name))
            {
                XenoCounts[name] += 1;
            }


            control.SetHealth((float)xeno.Health);
            control.SetPlasma((float)xeno.Plasma);
            control.SetEvo((int)xeno.Evo);

            if (xeno.Leader)
                control.SetLeader(iconTexture);

            control.Button.OnPressed += _ => SendPredictedMessage(new XenoWatchBuiMsg(xeno.Entity));

            control.HealButton.OnPressed += _ => SendPredictedMessage(new XenoWatchBuiHealingMsg(xeno.Entity));
            control.PlasmaButton.OnPressed += _ => SendPredictedMessage(new XenoWatchBuiTransferPlasmaMsg(xeno.Entity));
            if (s.IsQueen)
            {
                control.QueenButtons.Visible = true;
                control.NameLabel.SetWidth = _window.Width - 310;
            }

            _window.XenoContainer.AddChild(control);
        }

        foreach (var control in _window.RowContainer.Children)
        {
            if (control is not XenoTierRow row)
                continue;

            var slots = row.Tier switch
            {
                2 => tier2Slots,
                3 => tier3Slots,
                _ => 0
            };
            if (row.Tier is (2 or 3))
            {
                row.SlotsLeft.Text = $"Slots left: {slots}";
            }

            foreach (var child in row.XenosContainer.Children)
            {
                if (child is not XenoHiveCountControl hive)
                    continue;

                string name = "";

                foreach (var button in hive.XenoButton.Children)
                {
                    if (button is not Button xenobutton)
                        continue;

                    name  = xenobutton.Text??"";
                }

                if (XenoCounts.TryGetValue(name.Replace(" ","")??"", out var count))
                {
                  hive.Count.Text = $"{count}";
                }
            }
        }
        UpdateList();
    }

    private XenoWatchWindow EnsureWindow()
    {
        if (_window != null)
            return _window;

        _window = this.CreateWindow<XenoWatchWindow>();
        _window.SearchBar.OnTextChanged += OnSearchBarChanged;
        return _window;
    }

    private void OnSearchBarChanged(LineEditEventArgs args)
    {
        if (_window is not { Disposed: false })
            return;

        SearchBarText = args.Text;

        foreach (var child in _window.XenoContainer.Children)
        {

            if (child is not XenoChoiceControl control)
                continue;

            if (string.IsNullOrWhiteSpace(args.Text))
            {
                control.Visible = true;
                UsingSearchBar = false;
                UpdateList();
            }
            else
            {
                UsingSearchBar = true;
                control.Visible =
                    control.NameLabel.GetMessage()?.Contains(args.Text, StringComparison.OrdinalIgnoreCase) ?? false;
            }
        }
    }

    private void OnButtonToggled(BaseButton.ButtonEventArgs args)
    {
        string name = args.Button.Name ?? string.Empty; // button name is emtpy, what i need is the text
        if (name == string.Empty)
        {
            return;
        }
        if (ShownXenos.ContainsKey(name))
        {
            ShownXenos[name] = !ShownXenos[name];
            args.Button.ModulateSelfOverride = UpdateButtonColor(ShownXenos[name]);
        }
        ClearSearchBar();
        UpdateList();
    }


    private void OnTierToggled(BaseButton.ButtonEventArgs args)
    {
        args.Button.ModulateSelfOverride = UpdateButtonColor(args.Button.Pressed);
        if (args.Button.Name != null && int.TryParse(args.Button.Name.Split()[1], out var number)) // get the tier number frmo the button name
        {
            foreach (var xeno in tiers[number])
            {
                ShownXenos[xeno.Name.Replace(" ","")] = args.Button.Pressed;
            }
        }
        UpdateTierChilds();
        ClearSearchBar();
        UpdateList();
    }

   private void UpdateTierChilds()
    {
        if (_window is not { Disposed: false })
            return;

        var buttons = _window.RowContainer.Children
            .OfType<XenoTierRow>()
            .SelectMany(row => row.XenosContainer.Children)
            .OfType<XenoHiveCountControl>()
            .SelectMany(countControl => countControl.XenoButton.Children)
            .OfType<Button>();

        foreach (var button in buttons)
        {
            var name = button.Name ?? "";
            if (name != "")
                button.ModulateSelfOverride = UpdateButtonColor(ShownXenos[name]);
        }
    }

    private Color UpdateButtonColor(bool state)
    {
        return state ? XenoHighlightColor : XenoColor;
    }



    private void ClearSearchBar()
    {
        if (_window is not { Disposed: false })
            return;

        _window.SearchBar.Clear();
        UsingSearchBar = false;
    }

    private void UpdateList()
    {
        if (_window is not { Disposed: false })
            return;

        foreach (var child in _window.XenoContainer.Children)
            {
                if (child is not XenoChoiceControl control)
                    continue;

                if (UsingSearchBar)
                {
                    control.Visible = FilterLesserVisibility(control.NameLabel.GetMessage());
                }
                else
                {
                    if (!ShownXenos.ContainsValue(true))
                    {
                        control.Visible = true;
                        continue;
                    }

                    control.Visible = false;

                    if (ShownXenos.TryGetValue(control.Button.Name ?? "", out var xeno))
                    {
                        control.Visible = xeno;
                    }
                }
            }
    }

    private bool FilterLesserVisibility(string? nameLabel)
    {
        if (nameLabel == null)
            return false;

        if (!nameLabel.Contains(SearchBarText, StringComparison.OrdinalIgnoreCase))
            return false;

        var isLesserPartiallySearched = "lesser".Contains(SearchBarText, StringComparison.OrdinalIgnoreCase);
        var isLesser = nameLabel.Contains("lesser", StringComparison.OrdinalIgnoreCase);
        var isLesserSearched = SearchBarText.Contains("lesser", StringComparison.OrdinalIgnoreCase);
        var searchingForLesser = isLesserPartiallySearched || isLesserSearched;

        if (isLesser && !searchingForLesser)
            return false;

        return true;
    }
}
