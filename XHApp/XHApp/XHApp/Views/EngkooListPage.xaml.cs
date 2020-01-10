using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XHApp.Models;
using XHApp.ViewModels;

namespace XHApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EngkooListPage : ContentPage
    {
        EngkooListViewModel vm;

        public EngkooListPage()
        {
            InitializeComponent();
            vm = BindingContext as EngkooListViewModel;
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as ToolsItem;
            if (item == null)
                return;

            if (!string.IsNullOrWhiteSpace(item.Link))
            {
                await Navigation.PushAsync(new ExternalPage(new ExternalViewModel(item.Text, item.Link)));
            }

            ItemsListView.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (vm.Items.Count == 0)
                vm.LoadItemsCommand.Execute(null);
        }
    }
}