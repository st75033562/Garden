using System;
using DataAccess;

namespace Gameboard
{
    public class ObjectNameGenerator
    {
        private readonly ObjectNameValidator m_nameValidator;

        public ObjectNameGenerator(Gameboard gameboard, VariableManager varManager)
        {
            m_nameValidator = new ObjectNameValidator(null, gameboard, varManager);
        }

        public string Generate(ObjectAssetInfo assetInfo)
        {
            if (assetInfo == null)
            {
                throw new ArgumentNullException("assetInfo");
            }

            var assetData = BundleAssetData.Get(assetInfo.assetId);
            if (assetData == null)
            {
                throw new ArgumentException("invalid asset");
            }

            string prefix = assetData.localizedName.Localize();
            string name;
            do
            {
                name = string.Format(DataAccess.Constants.GameboardObjectNameFormat, prefix, assetInfo.nextObjectNum++);
            }
            while (m_nameValidator.IsDuplicate(name));
            return name;
        }
    }
}
