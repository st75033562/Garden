namespace DataAccess
{
    public static class Initialization
    {
        public static void Init(IDataSource source)
        {
            // make sure this is initialized first
            Constants.Load(source);

            AchieveData.Load(source);
            MedalData.Load(source);
            BundleAssetData.Load(source);
            CertificateData.Load(source);
            TrophyData.Load(source);
            TrophyResultData.Load(source);

            GameboardThemeData.Load(source);
            GameboardThemeBundleData.Load(source);
            GameboardTemplateData.Load(source);

            ARObjectDataSource.Load(source);

            SoundAssetData.Load(source);
            SoundBundleData.Load(source);
        }

        public static void InitNodeData(IDataSource source)
        {
            NodeTemplateData.Load(source);
            NodeData.Load(source);
            NodePluginData.Load(source);
            NodeLegacyIdMapping.Load(source);

            // cache after loading
            NodeTemplateData.Cache();
        }
    }
}
