﻿using Blazor.Core.Logging;
using Blazor.Core.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Blazor.Core.Widgets
{
    public class WidgetFactory : IWidgetFactory
    {
        private readonly IServiceProvider provider;
        private readonly IDictionary<string, WidgetVariant> map;

        public WidgetFactory(IServiceProvider provider)
        {
            this.provider = provider;
            map = new Dictionary<string, WidgetVariant>();
        }

        public void Register(string variantKey, WidgetVariant variant)
        {
            map[variantKey] = variant;
        }

        public object Build(string variantKey)
        {
            if (!map.TryGetValue(variantKey, out WidgetVariant variant))
            {
                ConsoleLogger.Debug($"WARNING: No widget variant for the key '{variantKey}'.");
                return null;
            }

            return BuildMediator(variant);
        }

        public object Build(WidgetVariant variant)
        {
            if (variant == null)
            {
                return null;
            }

            return BuildMediator(variant);
        }

        private object BuildMediator(WidgetVariant variant)
        {
            Type mediatorType = variant.MediatorType;
            object mediator = provider.GetService(mediatorType);

            TryFillMediatorContract(mediator, variant);
            TryInitialise(mediator);

            if (mediator == null)
            {
                ConsoleLogger.Debug($"ERROR: No mediator for a widget of type '{variant.MediatorType.Name}'.");
            }

            return mediator;
        }

        private void TryFillMediatorContract(object mediator, WidgetVariant variant)
        {
            if (!(mediator is IWidgetBuildContract contract))
            {
                return;
            }

            contract.SetMessageBus(provider.GetService<IMessageBus>());
            contract.SetCustomisation(variant.Customisation);

            if (TryGetState(variant.StateType, out object state))
            {
                contract.SetState(state);
            }

            if (!TryGetPresenter(variant.PresenterType, out IWidgetPresenter presenter)
                && mediator is IWidgetPresenterProvider presenterProvider)
            {
                // TODO: solve this in more elegant way
                presenter = presenterProvider.Presenter;
            }

            TryFillPresenterContract(presenter);
            TryInitialise(presenter);

            if (presenter == null)
            {
                ConsoleLogger.Debug($"WARNING: No presenter for a widget of type '{variant.MediatorType.Name}'.");
            }

            contract.SetPresenter(presenter);
        }

        private void TryFillPresenterContract(IWidgetPresenter presenter)
        {
            if (!(presenter is IWidgetPresenterBuildContract contract))
            {
                return;
            }

            contract.SetWidgetContainerManagement(provider.GetService<IWidgetContainerManagement>());
        }

        private bool TryGetState(Type stateType, out object state)
        {
            if (stateType == null)
            {
                state = null;
                return false;
            }

            state = provider.GetService(stateType);
            return true;
        }

        private bool TryGetPresenter(Type presenterType, out IWidgetPresenter presenter)
        {
            if(presenterType == null)
            {
                presenter = null;
                return false;
            }

            presenter = (IWidgetPresenter)provider.GetService(presenterType);
            return true;
        }

        private void TryInitialise(object subject)
        {
            if (!(subject is IInitialisable initialisable))
            {
                return;
            }

            initialisable.Initialise();
        }

        
    }
}
