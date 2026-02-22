namespace Application.Common.SQL.ResponseModels
{
    public class CompareResponse
    {
        public bool DataIsEqual { get; set; }
        public List<Dictionary<string, object>> QueryData1 { get; set; } = new List<Dictionary<string, object>>();

        public List<Dictionary<string, object>> QueryData2 { get; set; } = new List<Dictionary<string, object>>();

    }
}
