using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services.Xamarin
{
    public class Navigator : INavigator
    {
        public object RootFrame { get; set; }
        public object Frame { get; set; }

        public event EventHandler<ContentPageType> ContentPageChanged;

        public Task CloseCompactOverlayAsync(CompactViewMode closingOverlayMode)
        {
            throw new NotImplementedException();
        }

        public string GetContentPageName()
        {
            throw new NotImplementedException();
        }

        public void GoBack(string sourcePage = null)
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(ContentPageType contentPage)
        {
            throw new NotImplementedException();
        }

        public void ToCatalogue()
        {
            throw new NotImplementedException();
        }

        public void ToCompact()
        {
            throw new NotImplementedException();
        }

        public Task ToCompactOverlayAsync(CompactViewMode requestedOverlayMode)
        {
            throw new NotImplementedException();
        }

        public void ToScreensaver()
        {
            throw new NotImplementedException();
        }

        public void ToUploadPage()
        {
            throw new NotImplementedException();
        }
    }
}
