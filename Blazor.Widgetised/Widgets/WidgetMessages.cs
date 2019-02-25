﻿using System;
using Blazor.Widgetised.Messaging;

namespace Blazor.Widgetised
{
    public abstract class WidgetMessage : ISystemMessage, IWidgetIdentifier
    {
        public string VariantName { get; set; }

        public string Position { get; set; }

        public string WidgetKey { get; set; }

        public Guid WidgetId { get; set; }

        string IWidgetIdentifier.Name => VariantName;

        string IWidgetIdentifier.Position => Position;

        string IWidgetIdentifier.GetKey()
        {
            return new WidgetIdentifier(VariantName, Position).GetKey();
        }

        public class Build : WidgetMessage
        {
            public WidgetVariant Variant { get; set; }

            public object Customisation { get; set; }
        }

        public class Build<TCustomisation> : Build
            where TCustomisation : new()
        {
            public Build()
            {
                Customisation = new TCustomisation();
            }

            public new TCustomisation Customisation { get; set; }
        }

        public class Activate : WidgetMessage
        {
            // no member ( message type )
        }

        public class Deactivate : WidgetMessage
        {
            // no member ( message type )
        }

        public class Destroy : WidgetMessage
        {
            // no member ( message type )
        }
    }
}
