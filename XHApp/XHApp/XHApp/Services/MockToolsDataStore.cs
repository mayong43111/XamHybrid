using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XHApp.Actions;
using XHApp.Models;

namespace XHApp.Services
{
    public class MockToolsDataStore : IDataStore<ToolsItem>
    {
        readonly List<ToolsItem> items;

        public MockToolsDataStore()
        {
            items = new List<ToolsItem>()
            {
                new ToolsItem { Id="001", Text = "情景模拟", Description="This is an item description.",ImageSource="tool_mn.png",Link=CommonDefine.EnglishAssistantScenarioLessonUrl },
                new ToolsItem {  Id="002",  Text = "口语特训", Description="This is an item description.",ImageSource="tool_ky.png",Link="" },
                new ToolsItem {  Id="003",  Text = "单词修炼", Description="This is an item description.",ImageSource="tool_dc.png",Link="" },
                new ToolsItem { Id="004",   Text = "看脸起名", Description="This is an item description.",ImageSource="tool_qm.png",Link="" }
            };
        }

        public async Task<bool> AddItemAsync(ToolsItem item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(ToolsItem item)
        {
            var oldItem = items.Where((ToolsItem arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((ToolsItem arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<ToolsItem> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<ToolsItem>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}
