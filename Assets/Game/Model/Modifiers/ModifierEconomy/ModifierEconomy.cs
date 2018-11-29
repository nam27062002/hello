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
            case ModEconomyDragonDiscount.TARGET_CODE: return new ModEconomyDragonDiscount(_def);
        }

        return null;
    }

    public new static ModifierEconomy CreateFromJson(SimpleJSON.JSONNode _data) {
        string target = _data["target"];

        switch (target) {
            case ModEconomyDragonDiscount.TARGET_CODE: return new ModEconomyDragonDiscount(_data);
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
    protected ModifierEconomy() : base(TYPE_CODE) { }
    protected ModifierEconomy(DefinitionNode _def) : base(TYPE_CODE, _def) { }
    protected ModifierEconomy(SimpleJSON.JSONNode _data) : base(TYPE_CODE, _data) { }

}