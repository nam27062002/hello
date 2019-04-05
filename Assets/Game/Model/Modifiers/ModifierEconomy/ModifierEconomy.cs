public abstract class ModifierEconomy : Modifier {
    //------------------------------------------------------------------------//
    // CONSTANTS                                                              //
    //------------------------------------------------------------------------//
    public const string TYPE_CODE = "economy";


    //------------------------------------------------------------------------//
    // FACTORY METHODS                                                        //
    //------------------------------------------------------------------------//
    #region Factory

    public new static ModifierEconomy CreateFromDefinition(DefinitionNode _def) {
        string target = _def.Get("target");

        switch (target) {
            case ModEconomyDragonPrice.TARGET_CODE: return new ModEconomyDragonPrice(_def);
        }

        return null;
    }

    public new static ModifierEconomy CreateFromJson(SimpleJSON.JSONNode _data) {
        string target = _data["target"];

        switch (target) {
            case ModEconomyDragonPrice.TARGET_CODE: return new ModEconomyDragonPrice(_data);
        }

        return null;
    }

    #endregion


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES                                                 //
    //------------------------------------------------------------------------//



    //------------------------------------------------------------------------//
    // METHODS                                                                //
    //------------------------------------------------------------------------//
    protected ModifierEconomy(string _target) : base(TYPE_CODE, _target) { }
    protected ModifierEconomy(string _target, DefinitionNode _def) : base(TYPE_CODE, _target, _def) { }
    protected ModifierEconomy(string _target, SimpleJSON.JSONNode _data) : base(TYPE_CODE, _target, _data) { }

}