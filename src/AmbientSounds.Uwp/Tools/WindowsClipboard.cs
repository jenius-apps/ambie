using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace AmbientSounds.Tools.Uwp;

public class WindowsClipboard : IClipboard
{
    public Task<bool> CopyToClipboardAsync(string text)
    {
        try
        {
            DataPackage dataPackage = new()
            {
                RequestedOperation = DataPackageOperation.Copy
            };

            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
        catch
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
