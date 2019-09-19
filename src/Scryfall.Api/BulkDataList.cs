
namespace Scryfall.Api
{
    public partial class BulkDataList
    {
        public BulkData this[BulkDataType key] => Data.Find(d => d.Type == key);

        public BulkData Rulings => this[BulkDataType.Rulings];
        public BulkData DefaultCards => this[BulkDataType.DefaultCards];
        public BulkData AllCards => this[BulkDataType.AllCards];
        public BulkData OracleCards => this[BulkDataType.OracleCards];
    }
}