/*
using Content.Shared._RMC14.Input;
using Robust.Shared.Input.Binding;

namespace Content.Shared._RMC14.Pinging;

public sealed class PingingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        CommandBinds.Builder
            .Bind(CMKeyFunctions.RMCXenoPing,
                InputCmdHandler.FromDelegate(session =>
                {
                    if (session?.AttachedEntity != null)
                        ; //handle pressed
                }, handle: false))
            .Register<PingingSystem>();
    }



}
*/
