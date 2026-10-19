namespace DIG.GBLXAPI
{
    public class GBLConfig
    {
        public const string StandardsUserPath = "data/GBLxAPI_Vocab_User";
        public readonly LrsConfig lrsConfig;

        public GBLConfig(LrsConfig lrsConfig)
        {
            this.lrsConfig = lrsConfig;
        }
    }
}
