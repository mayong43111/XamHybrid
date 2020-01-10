using System;
using Xamarin.Forms;

namespace XHApp.ViewModels
{
    public class ExternalViewModel : BaseViewModel
    {
        public string Uri { get; set; }

        public ExternalViewModel(string title, string uri)
        {
            this.Title = title;
            this.Uri = CalculateUri(uri);
        }

        private string CalculateUri(string uri)
        {
            UriBuilder source = new UriBuilder(uri);
            source.Query += string.IsNullOrWhiteSpace(source.Query) ? string.Empty : "&";

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    source.Query += "origin=ios-xinfangxiang";
                    break;
                case Device.Android:
                    source.Query += "origin=android-xinfangxiang";
                    break;
                default:
                    break;
            }

            return source.ToString();
        }
    }
}
