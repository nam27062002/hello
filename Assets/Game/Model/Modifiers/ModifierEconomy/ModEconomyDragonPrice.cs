
public class ModEconomyDragonPrice : ModifierEconomy {
    public const string TARGET_CODE = "dragon_price";


    //------------------------------------------------------------------------//
    private string m_sku;
    private float m_percentage;


    //------------------------------------------------------------------------//
    public ModEconomyDragonPrice(DefinitionNode _def) : base(_def) {
        m_sku = _def.Get("param1");
        m_percentage = _def.GetAsFloat("param2");
        BuildTextParams(m_sku, m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public ModEconomyDragonPrice(SimpleJSON.JSONNode _data) : base(_data) {
        m_sku = _data["param1"];
        m_percentage = _data["param2"].AsFloat;
        BuildTextParams(m_sku, m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public override void Apply() {
        IDragonData dragonData = DragonManager.GetDragonData(m_sku);
        if (dragonData != null) {
            dragonData.AddPriceModifer(m_percentage);
            Messenger.Broadcast<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, dragonData);
        }
    }

    public override void Remove() {
        IDragonData dragonData = DragonManager.GetDragonData(m_sku);
        if (dragonData != null) {
            dragonData.AddPriceModifer(-m_percentage);
            Messenger.Broadcast<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, dragonData);
        }
    }

    protected override SimpleJSON.JSONClass __ToJson() {
        SimpleJSON.JSONClass data = base.__ToJson();

        data.Add("param1", m_sku);
        data.Add("param2", m_percentage);

        return data;
    }
}