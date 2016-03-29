﻿/******************************************************************************* 
  * Copyright 2015 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/ 

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CoordinateSystemAddin.UI {
    /// <summary>
    /// Interaction logic for CoordinateSystemPicker.xaml
    /// </summary>
    /// <remarks>Handy reference on how to style the TreeView:
    /// <a href="http://blogs.msdn.com/b/mikehillberg/archive/2009/10/30/treeview-and-hierarchicaldatatemplate-step-by-step.aspx"/>
    /// </remarks>
    public partial class CoordSysPickerView : UserControl {
        public CoordSysPickerView() {
            InitializeComponent();
        }

        private void TreeViewItem_BringIntoView(object sender, RoutedEventArgs e) {
            TreeViewItem item = e.OriginalSource as TreeViewItem;

            var count = VisualTreeHelper.GetChildrenCount(item);
            if (0 < count) {
                for (int i = count - 1; i >= 0; --i) {
                    var childItem = VisualTreeHelper.GetChild(item, i);
                    ((FrameworkElement)childItem).BringIntoView();
                }
            }
            else
                item.BringIntoView();

            // Make sure item has focus
            if (Keyboard.FocusedElement != item) {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => {
                    Keyboard.Focus(item);
                    item.Focus();
                }));
            }
        }

        private void CoordSystemTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            SelectedItemHelper.Content = e.NewValue;
        }
    }
}
