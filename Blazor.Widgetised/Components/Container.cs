﻿using System;
using Blazor.Widgetised.Logging;
using Blazor.Widgetised.Presenters;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Blazor.Widgetised.Components
{
    public class Container : ComponentBase, IRenderingContainer, IDisposable
    {
        private RenderFragment? content;
        private string? registeredKey;

        public Container()
        {
            Management = NotAssigned.WidgetContainerManagement;
            Logger = NotAssigned.Logger;
        }   

        [Inject]
        private IWidgetContainerManagement Management { get; set; }

        [Inject]
        private ILogger Logger { get; set; }

        [Parameter]
        private string? Key { get; set; }

        [Parameter]
        private RenderFragment? ChildContent { get; set; }

        public void SetKey(string newKey)
        {
            if (IsKeySameLastPrevious(newKey))
            {
                return;
            }

            Key = newKey;
            RegisterKey(newKey);
            StateHasChanged();
        }

        public void SetContent(RenderFragment newContent)
        {
            content = newContent;
            StateHasChanged();
        }

        public void ClearContent()
        {
            content = null;
            StateHasChanged();
        }

        public void Dispose()
        {
            UnregisterPreviousKey();
        }

        protected override void OnParametersSet()
        {
            RegisterKey(Key);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenElement(0, "div");
            bool hasContent = content != null;

            if (!string.IsNullOrEmpty(Key))
            {
                builder.AddAttribute(2, "data-key", Key);
            }

            if (hasContent)
            {
                Logger.Trace($"Rendering filled container [{registeredKey ?? "no-key"}]");
                builder.AddAttribute(1, "class", "system-container");
                builder.AddContent(3, content);
            }
            else
            {
                Logger.Trace($"Rendering empty container [{registeredKey ?? "no-key"}]");
                builder.AddAttribute(1, "class", "system-container empty");

                if (ChildContent != null)
                {
                    builder.AddContent(3, ChildContent);
                }
            }

            builder.CloseElement();
        }

        private void RegisterKey(string? key)
        {
            if (IsKeySameLastPrevious(key))
            {
                return;
            }

            UnregisterPreviousKey();

            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Management.Register(key, this);
            registeredKey = key;
            Logger.Debug($"Container with key [{key}] has been registered.");
        }

        private void UnregisterPreviousKey()
        {
            if (registeredKey == null)
            {
                return;
            }

            Management.Unregister(registeredKey);
            registeredKey = null;
        }

        private bool IsKeySameLastPrevious(string? key)
        {
            return string.Equals(key, registeredKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}
