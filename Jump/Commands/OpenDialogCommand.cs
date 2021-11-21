namespace Jump
{
    [Command(PackageIds.OpenDialogCommand)]
    internal sealed class OpenDialogCommand : BaseCommand<OpenDialogCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                using var dialog = new JumpDialogWindow();
                dialog.ShowDialog();
            }
            catch (Exception)
            {
                await VS.MessageBox.ShowErrorAsync("Jump", "Somthing went wrong");
            }
        }
    }
}
