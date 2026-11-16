public class LoadingEffectsController : UIControllerBase<LoadingEffectsView>
{
    public void ShowLoadingBanner()
    {
        View.ShowView();
    }

    public void HideLoadingBanner()
    {
        View.HideView();
    }
}
