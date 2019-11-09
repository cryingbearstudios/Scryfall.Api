
namespace Scryfall.Api
{
    public partial class BulkDataList
    {
        public BulkData this[string key] => Data.Find(d => d.Type == key);

        public BulkData Rulings => this["rulings"];
        public BulkData DefaultCards => this["default_cards"];
        public BulkData AllCards => this["all_cards"];
        public BulkData OracleCards => this["oracle_cards"];
    }
}