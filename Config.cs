using BepInEx.Configuration;
namespace OnTheCase.Config
{
    internal class CaseConfig
    {
        internal CaseConfig(ConfigFile config)
        {
            CaseMod.Instance.Config.SaveOnConfigSet = false;
            CosmeticsConfig(config);
            CaseMod.Instance.Config.Save();
            CaseMod.Instance.Config.SaveOnConfigSet = true;
        }
        internal void CosmeticsConfig(ConfigFile config)
        {

        }
    }
}