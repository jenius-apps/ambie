using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services.Xamarin
{
    public class Navigator : INavigator
    {
        public object RootFrame { get; set; }
        public object Frame { get; set; }

        public void GoBack(string sourcePage = null)
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
