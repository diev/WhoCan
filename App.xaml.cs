﻿#region License
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
#endregion

using System;
using System.Reflection;
using System.Windows;

namespace WhoCan
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Title;
        public static Version Version;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var asm = Assembly.GetExecutingAssembly().GetName();
            Title = asm.Name;
            Version = asm.Version;

            //var window = new MainWindow
            //{
            //    Title = Title
            //};
            //window.Version.Text = "v" + Version.ToString(3);

            //window.Show();
        }
    }
}
