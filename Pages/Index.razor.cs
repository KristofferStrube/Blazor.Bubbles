using Blazor.Bubbles.Services;
using Microsoft.AspNetCore.Components;
using System.Text.Json;
using static System.Random;

namespace Blazor.Bubbles.Pages
{
    public partial class Index
    {
        [Parameter, SupplyParameterFromQuery]
        public double? Amount { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public double? MinRadius { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public double? MaxRadius { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public double? MaxSpeed { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public double? MinSpeed { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public string? Colors { get; set; }

        private List<string> colors = new() { "#007A30", "#00C44E", "#00DB57", "#00A843", "#00913A", "#00B749" };

        [Inject]
        protected BoundingBoxSubscriberService BoundingBoxSubscriberService { get; set; }

        protected ElementReference SVG { get; set; }

        protected BoundingBox BoundingBox { get; set; } = new();

        protected List<Bubble> Bubbles = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                BoundingBoxSubscriber? subscriber = await BoundingBoxSubscriberService.CreateSubscriberForChangeAsync();
                await subscriber.Subscribe(SVG, (b) =>
                {
                    BoundingBox = b;
                    StateHasChanged();
                });

                if (!Amount.HasValue)
                    Amount = 30;

                if (!MinRadius.HasValue)
                    MinRadius = Math.Max(BoundingBox.Width, BoundingBox.Height) / 20 / 10;

                if (!MaxRadius.HasValue)
                    MaxRadius = Math.Max(BoundingBox.Width, BoundingBox.Height) / 20;

                if (MinRadius > MaxRadius)
                    throw new ArgumentException("MinRadius needs to be lower than or equal to MaxRadius. MinRadius is 0 by default and MaxRadius is one 20th of the widest of the height or width of the screen.");

                if (!MinSpeed.HasValue)
                    MinSpeed = 0;

                if (!MaxSpeed.HasValue)
                    MaxSpeed = 1;
                
                if (MinSpeed > MaxSpeed)
                    throw new ArgumentException("MinSpeed needs to be lower than or equal to MaxSpeed. MinSpeed is 0 by default and MaxSpeed is 1.");

                if (Colors is not null)
                {
                    colors = JsonSerializer.Deserialize<List<string>>(Colors) ?? colors;
                }

                for (int i = 0; i < Amount; i++)
                {
                    var newBubble = new Bubble()
                    {
                        X = Shared.NextDouble() * BoundingBox.Width,
                        Y = Shared.NextDouble() * BoundingBox.Height
                    };
                    RandomizeBubble(newBubble);
                    Bubbles.Add(newBubble);
                }

                await Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(1);
                        foreach (Bubble b in Bubbles)
                        {
                            b.X += b.VX;
                            b.Y += b.VY;
                            CollideWithOuterBox(b);
                        }
                        foreach (Bubble b1 in Bubbles)
                        {
                            foreach (Bubble b2 in Bubbles)
                            {
                                if (b1 == b2)
                                {
                                    continue;
                                }

                                CollideBubbles(b1, b2);
                            }
                        }

                        StateHasChanged();
                    }
                });
            }
        }

        protected void RandomizeBubble(Bubble b)
        {
            b.Color = colors[Shared.Next(colors.Count)];
            b.Radius = MinRadius.Value + Shared.NextDouble() * (MaxRadius.Value - MinRadius.Value);
            b.VX = (MinSpeed.Value + Shared.NextDouble() * (MaxSpeed.Value - MinSpeed.Value)) * (Shared.Next(0, 2) == 0 ? 1 : -1);
            b.VY = (MinSpeed.Value + Shared.NextDouble() * (MaxSpeed.Value - MinSpeed.Value)) * (Shared.Next(0, 2) == 0 ? 1 : -1);
        }

        private void CollideWithOuterBox(Bubble b)
        {
            if (b.X - b.Radius < 0)
            {
                b.X -= b.X - b.Radius;
                b.VX = Math.Abs(b.VX);
            }
            if (b.X + b.Radius > BoundingBox.Width)
            {
                b.X -= b.X + b.Radius - BoundingBox.Width;
                b.VX = -Math.Abs(b.VX);
            }
            if (b.Y - b.Radius < 0)
            {
                b.Y -= b.Y - b.Radius;
                b.VY = Math.Abs(b.VY);
            }
            if (b.Y + b.Radius > BoundingBox.Height)
            {
                b.Y -= b.Y + b.Radius - BoundingBox.Height;
                b.VY = -Math.Abs(b.VY);
            }
        }

        private void CollideBubbles(Bubble b1, Bubble b2)
        {
            double dist = Math.Sqrt(Math.Pow(b1.X - b2.X, 2) + Math.Pow(b1.Y - b2.Y, 2));
            if (dist > b1.Radius + b2.Radius)
            {
                return;
            }

            b1.X += (b1.X - b2.X) / dist * (b1.Radius + b2.Radius - dist);
            b1.Y += (b1.Y - b2.Y) / dist * (b1.Radius + b2.Radius - dist);
            b2.X += (b2.X - b1.X) / dist * (b1.Radius + b2.Radius - dist);
            b2.Y += (b2.Y - b1.Y) / dist * (b1.Radius + b2.Radius - dist);
        }

        protected class Bubble
        {
            public string Color { get; set; }

            public double Radius { get; set; }

            public double X { get; set; }

            public double Y { get; set; }

            public double VX { get; set; }

            public double VY { get; set; }
        }
    }
}