using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RoboMarketPro.Components
{
    public class SearchBox<TItem> : ComponentBase
    {
        [Parameter]
        public IEnumerable<TItem> Items { get; set; }

        [Parameter]
        public Func<TItem, string> SearchFunc { get; set; }

        [Parameter]
        public RenderFragment<TItem> ItemTemplate { get; set; }

        [Parameter]
        public EventCallback<TItem> OnItemSelected { get; set; }

        private string searchTerm = string.Empty;
        private IEnumerable<TItem> filteredItems => Items?.Where(item => SearchFunc(item).Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        private void SelectItem(TItem item)
        {
            searchTerm = SearchFunc(item);
            OnItemSelected.InvokeAsync(item);
        }

        private void OnInput(ChangeEventArgs e)
        {
            searchTerm = e.Value.ToString();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");

            builder.OpenElement(1, "input");
            builder.AddAttribute(2, "placeholder", "Search...");
            builder.AddAttribute(3, "value", searchTerm);
            builder.AddAttribute(4, "oninput", EventCallback.Factory.Create<ChangeEventArgs>(this, OnInput));
            builder.CloseElement();

            builder.OpenElement(5, "ul");

            if (filteredItems?.Any() == true)
            {
                foreach (var item in filteredItems)
                {
                    builder.OpenElement(6, "li");
                    builder.AddAttribute(7, "onclick", EventCallback.Factory.Create(this, () => SelectItem(item)));
                    builder.AddContent(8, ItemTemplate(item));
                    builder.CloseElement();
                }
            }
            else if (!string.IsNullOrEmpty(searchTerm))
            {
                builder.OpenElement(9, "li");
                builder.AddContent(10, "No items found");
                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();
        }
    }
}
