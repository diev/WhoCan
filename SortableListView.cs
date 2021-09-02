#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov
// Source https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------
// (c) http://www.thejoyofcode.com/Sortable_ListView_in_WPF.aspx
//------------------------------------------------------------------------------
#endregion License

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WhoCan
{
    public class SortableListView : ListView
    {
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public SortableListView() =>
            // register a handler for the GridViewColumnHeader's Click Event
            //this.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

        private void Sort(string sortBy, ListSortDirection direction)
        {
            // refer to our *OWN* ItemsSource
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.ItemsSource);

            // check the dataView isn't null
            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            ListSortDirection direction;

            if (e.OriginalSource is GridViewColumnHeader headerClicked && headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                direction = headerClicked != _lastHeaderClicked || _lastDirection != ListSortDirection.Ascending
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;

                // see if we have an attached SortPropertyName value
                string sortBy = GetSortPropertyName(headerClicked.Column);

                if (string.IsNullOrEmpty(sortBy))
                {
                    // otherwise use the column header name
                    sortBy = headerClicked.Column.Header as string;
                }

                Sort(sortBy, direction);

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;
            }
        }

        public static readonly DependencyProperty SortPropertyNameProperty = DependencyProperty.RegisterAttached("SortPropertyName", typeof(string), typeof(SortableListView));

        public static string GetSortPropertyName(GridViewColumn obj)
        {
            return (string)obj.GetValue(SortPropertyNameProperty);
        }

        public static void SetSortPropertyName(GridViewColumn obj, string value)
        {
            obj.SetValue(SortPropertyNameProperty, value);
        }
    }
}
