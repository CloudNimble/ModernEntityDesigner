// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Data.Entity.Design.EntityDesigner.View.ContextMenu
{
    /// <summary>
    /// Converts null or empty strings to Collapsed visibility, otherwise Visible.
    /// </summary>
    internal class NullToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (value is string str && string.IsNullOrEmpty(str))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a count greater than 0 to Visible, otherwise Collapsed.
    /// </summary>
    internal class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && count > 0)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts the first item in an ItemsControl to Collapsed, all others to Visible.
    /// Used to hide the separator before the first item.
    /// </summary>
    internal class FirstItemToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ContentPresenter presenter)
            {
                var itemsControl = ItemsControl.ItemsControlFromItemContainer(presenter);
                if (itemsControl != null)
                {
                    int index = itemsControl.ItemContainerGenerator.IndexFromContainer(presenter);
                    return index == 0 ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Selects the appropriate DataTemplate for menu items based on their type.
    /// </summary>
    internal class MenuItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the template for command items.
        /// </summary>
        public DataTemplate CommandTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for separator items.
        /// </summary>
        public DataTemplate SeparatorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MenuSeparatorDefinition)
            {
                return SeparatorTemplate;
            }

            if (item is MenuCommandDefinition)
            {
                return CommandTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
