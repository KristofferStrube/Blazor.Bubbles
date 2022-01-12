using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json.Serialization;

namespace Blazor.Bubbles.Services
{
    public class BoundingBoxSubscriberService : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public BoundingBoxSubscriberService(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./js/Blazor.Bubbles.BoundingBoxSubscriber.js").AsTask());
        }

        public async Task<BoundingBoxSubscriber> CreateSubscriberForChangeAsync()
        {
            IJSObjectReference? module = await moduleTask.Value;

            return new BoundingBoxSubscriber(module);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                IJSObjectReference? module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }

    public class BoundingBoxSubscriber
    {
        private IJSObjectReference Module { get; init; }

        private Action<BoundingBox> Callback { get; set; }

        private DotNetObjectReference<BoundingBoxSubscriber> objRef { get; set; }

        public BoundingBoxSubscriber(IJSObjectReference module)
        {
            Module = module;
            objRef = DotNetObjectReference.Create(this);
        }

        public async Task Subscribe(ElementReference elementReference, Action<BoundingBox> callback)
        {
            Callback += callback;

            await Module.InvokeVoidAsync("SubscribeForChange", elementReference, objRef);
        }

        [JSInvokable("InvokeCallback")]
        public void InvokeCallback(BoundingBox boundingBox)
        {
            Callback?.Invoke(boundingBox);
        }
    }

    public class BoundingBox
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("Height")]
        public double Height { get; set; }
    }
}
