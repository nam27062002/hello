
public class ModEconomyDragonPrice : ModifierEconomy {
    public const string TARGET_CODE = "dragon_price";


    //------------------------------------------------------------------------//
    private string m_sku;
    private float m_percentage;
    private string m_currency;

    //------------------------------------------------------------------------//
    public ModEconomyDragonPrice(DefinitionNode _def) : base(_def) {
        m_sku = _def.Get("param1");
        m_percentage = _def.GetAsFloat("param2");
        m_currency = _def.Get("param3");
        BuildTextParams(m_sku, m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public ModEconomyDragonPrice(SimpleJSON.JSONNode _data) : base(_data) {
        m_sku = _data["param1"];
        m_percentage = _data["param2"].AsFloat;
        m_currency = _data["param3"];
        BuildTextParams(m_sku, m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public override void Apply() {
        IDragonData dragonData = DragonManager.GetDragonData(m_sku);
        if (dragonData != null) {
            switch(m_currency) {
                case "sc": dragonData.AddPriceSCModifer(m_percentage); break;
                case "hc": dragonData.AddPricePCModifer(m_percentage); break;
            }
            Messenger.Broadcast<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, dragonData);
        }
    }

    public override void Remove() {
        IDragonData dragonData = DragonManager.GetDragonData(m_sku);
        if (dragonData != null) {
            switch (m_currency) {
                case "sc": dragonData.AddPriceSCModifer(-m_percentage); break;
                case "hc": dragonData.AddPricePCModifer(-m_percentage); break;
            }
            Messenger.Broadcast<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, dragonData);
        }
    }

    protected override SimpleJSON.JSONClass __ToJson() {
        SimpleJSON.JSONClass data = base.__ToJson();

        data.Add("param1", m_sku);
        data.Add("param2", m_percentage);
        data.Add("param3", m_currency);

        return data;
    }
}