// Copyright © 2017 Paddy Xu
// 
// This file is part of QuickLook program.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CsvHelper;
using QuickLook.Common.ExtensionMethods;
using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using CsvHelper.Configuration;
using System.Diagnostics;

namespace QuickLook.Plugin.CsvViewer
{
    /// <summary>
    ///     Interaction logic for CsvViewerPanel.xaml
    /// </summary>
    public partial class CsvViewerPanel : UserControl
    {
        private readonly ContextObject _context;
        public CsvViewerPanel(string path, ContextObject context)
        {
            _context = context;
            InitializeComponent();
            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem
            { Header = TranslationHelper.Get("Editor_Copy"), Command = ApplicationCommands.Copy });
            ContextMenu.Items.Add(new MenuItem
            { Header = TranslationHelper.Get("Editor_SelectAll"), Command = ApplicationCommands.SelectAll });
            LoadFile(path);
            context.IsBusy = false;
        }

        public List<string[]> Rows { get; private set; } = new List<string[]>();

        public void LoadFile(string path)
        {
            const int limit = 10000;
            var binded = false;

            using (var sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), EncodingExtensions.GetEncoding(path)))
            {

                //edit by gh
                //var conf = new CsvHelper.Configuration.Configuration() {MissingFieldFound = null, BadDataFound = null};
                CsvConfiguration conf = new CsvConfiguration(CultureInfo.CurrentCulture);
                using (var csv = new CsvReader(sr, conf))
                {
                    var i = 0;
                    while (csv.Read())
                    {
                        List<string> result = new List<string>();
                        int k = 0;
                        for (k = 0; csv.TryGetField<string>(k, out string value); k++)
                        {
                            result.Add(value);
                        }
                        if (!binded)
                        {
                            SetupColumnBinding(result.Count+1);
                            binded = true;
                        } else
                        {
                            //补位
                            for(;k < dataGrid.Columns.Count; k++)
                            {
                                result.Add("");
                            }
                        }
                        var row = Concat(new[] { $"{i++ + 1}".PadLeft(6) }, result.ToArray());
                        if (i > limit)
                        {
                            Rows.Add(Enumerable.Repeat("...", row.Length).ToArray());
                            break;
                        }

                        Rows.Add(row);
                    }
                }
                /*
                using (var parser = new CsvParser(sr, conf))
                {
                    var i = 0;
                    while (true)
                    {
                        var row = parser.Read();
                        if (row == null)
                            break;
                        row = Concat(new[] { $"{i++ + 1}".PadLeft(6) }, row);

                        if (!binded)
                        {
                            SetupColumnBinding(row.Length);
                            binded = true;
                        }

                        if (i > limit)
                        {
                            Rows.Add(Enumerable.Repeat("...", row.Length).ToArray());
                            break;
                        }

                        Rows.Add(row);
                    }
                }
                */
                //-------------------//
            }
        }

        private void SetupColumnBinding(int rowLength)
        {
            for (var i = 0; i < rowLength; i++)
            {
                var col = new DataGridTextColumn
                {
                    //FontFamily = new FontFamily("Consolas"),
                    FontFamily = new FontFamily(TranslationHelper.Get("Editor_FontFamily")),
                    FontWeight = FontWeight.FromOpenTypeWeight(i == 0 ? 700 : 400),
                    Binding = new Binding($"[{i}]")
                };
                dataGrid.Columns.Add(col);
            }
        }

        public static T[] Concat<T>(T[] x, T[] y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            var oldLen = x.Length;
            Array.Resize(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }
    }
}