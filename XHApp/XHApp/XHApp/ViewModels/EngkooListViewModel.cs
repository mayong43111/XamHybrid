using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XHApp.Models;
using XHApp.Services;

namespace XHApp.ViewModels
{
    public class EngkooListViewModel : BaseViewModel
    {
        public IDataStore<ToolsItem> ToolsDataStore => DependencyService.Get<IDataStore<ToolsItem>>();
        public ObservableCollection<ToolsItem> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public EngkooListViewModel()
        {
            Title = "小英助手";
            Items = new ObservableCollection<ToolsItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await ToolsDataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Message", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
