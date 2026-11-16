using System;

public class ConfirmationWindowController : UIControllerBase<ConfirmationWindowViewBase>
{
    public event Action SubmitSelected;
    public event Action CancelSelected;

    public bool WindowActive { get; private set; }

    public void ShowWindow()
    {
        View.ShowView();
    }

    protected override void OnViewOpened()
    {
        WindowActive = true;
    }

    public void Submit()
    {
        SubmitSelected?.Invoke();
        CloseWindow();
    }

    public void Cancel()
    {
        CancelSelected?.Invoke();
        CloseWindow();
    }

    private void CloseWindow()
    {
        WindowActive = false;
        View.HideView();
    }
}
