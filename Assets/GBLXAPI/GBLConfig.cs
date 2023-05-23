namespace DIG.GBLXAPI
{
    public class GBLConfig
    {
        public const string StandardsDefaultPath = "data/GBLxAPI_Vocab_Default";
        public const string StandardsUserPath = "data/GBLxAPI_Vocab_User";

        public string lrsURL;

        // Fill in these fields for GBLxAPI setup.
        public string lrsUser;
        public string lrsPassword;

        public GBLConfig(string lrsURL, string lrsUser, string lrsPassword)
        {
            this.lrsURL = lrsURL;
            this.lrsUser = lrsUser;
            this.lrsPassword = lrsPassword;
        }
    }
}
