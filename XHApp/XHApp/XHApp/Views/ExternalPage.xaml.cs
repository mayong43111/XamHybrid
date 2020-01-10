
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XHApp.Actions;
using XHApp.ViewModels;

namespace XHApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExternalPage : ContentPage
    {
        private ExternalViewModel viewModel;

        public ExternalPage(ExternalViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this.viewModel = viewModel;
        }
    }
}