﻿using PriceGas.Client.ComponentesGenericos.Utilities.BindingConverters;
using PriceGas.Client.ComponentesGenericos.Extensions;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PriceGas.Client.ComponentesGenericos.Base
{
    public class SimBooleanInput<T> : SimFormComponent<T, bool?>
    {
        public SimBooleanInput() : base(new BoolConverter<T>()) { }

        /// <summary>
        /// If true, the input will be disabled.
        /// </summary>
        [Parameter] public bool Disabled { get; set; }

        /// <summary>
        /// If true, the input will be read only.
        /// </summary>
        [Parameter] public bool ReadOnly { get; set; }

        /// <summary>
        /// The state of the component
        /// </summary>
        [Parameter]
        public T Checked
        {
            get => _value;
            set => _value = value;
        }

        /// <summary>
        /// Fired when Checked changes.
        /// </summary>
        [Parameter] public EventCallback<T> CheckedChanged { get; set; }

        protected bool? BoolValue => Converter.Set(Checked);

        protected virtual Task OnChange(ChangeEventArgs args)
        {
            Touched = true;
            return SetBoolValueAsync((bool?)args.Value);
        }

        protected Task SetBoolValueAsync(bool? value)
        {
            return SetCheckedAsync(Converter.Get(value));
        }

        protected async Task SetCheckedAsync(T value)
        {
            if (Disabled)
                return;
            if (!EqualityComparer<T>.Default.Equals(Checked, value))
            {
                Checked = value;
                await CheckedChanged.InvokeAsync(value);
                BeginValidate();
            }
        }

        protected override bool SetConverter(Utilities.BindingConverters.Converter<T, bool?> value)
        {
            var changed = base.SetConverter(value);
            if (changed)
                SetBoolValueAsync(Converter.Set(Checked)).AndForget();

            return changed;
        }

        /// <summary>
        /// A value is required, so if not checked we return ERROR.
        /// </summary>
        protected override bool HasValue(T value)
        {
            return (BoolValue == true);
        }
    }
}
