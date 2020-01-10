using System;
using System.Collections.Generic;
using System.Text;

namespace XHApp.ViewModels
{
    public class ExternalViewModel : BaseViewModel
    {
        public string Uri { get; set; }

        public ExternalViewModel(string title, string uri)
        {
            this.Title = title;
            this.Uri = uri;
        }
    }
}
