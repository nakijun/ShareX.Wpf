﻿using ShareX.PluginsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ShareX.UploadersLib
{
    public interface IShareXUploaderPlugin : IShareXPluginBase
    {
        string Location { get; set; }

        UserControl UI { get; }

        void LoadSettings(string filePath);

        void SaveSettings();
    }
}