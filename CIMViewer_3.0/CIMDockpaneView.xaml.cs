﻿//   Copyright 2016 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace CIMViewer {
    /// <summary>
    /// Interaction logic for CIMDockpaneView.xaml
    /// </summary>
    public partial class CIMDockpaneView : UserControl {
        public CIMDockpaneView() {
            InitializeComponent();
            this.IsVisibleChanged += CIMDockpaneView_IsVisibleChanged;
        }

        private void CIMDockpaneView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (this.IsVisible) {
                System.Diagnostics.Debug.WriteLine("CIMDockpaneView IsVisibleChanged: {0}", this.IsVisible);
            }
        }
    }
}
