﻿// Copyright © 2017 Paddy Xu
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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using QuickLook.Common.Helpers;
using QuickLook.Common.Plugin;
using QuickLook.Common.ExtensionMethods;

namespace QuickLook.Plugin.TextViewer
{
    public class TextViewerPanel : TextEditor, IDisposable
    {
        private readonly ContextObject _context;
        private bool _disposed;

        public TextViewerPanel(string path, ContextObject context)
        {
            _context = context;

            Background = new SolidColorBrush(Color.FromArgb(0xAA, 255, 255, 255));
            FontSize = 14;
            ShowLineNumbers = true;
            WordWrap = true;
            IsReadOnly = true;
            IsManipulationEnabled = true;
            Options.EnableEmailHyperlinks = false;
            Options.EnableHyperlinks = false;

            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem
                {Header = TranslationHelper.Get("Editor_Copy"), Command = ApplicationCommands.Copy});
            ContextMenu.Items.Add(new MenuItem
                {Header = TranslationHelper.Get("Editor_SelectAll"), Command = ApplicationCommands.SelectAll});

            ManipulationInertiaStarting += Viewer_ManipulationInertiaStarting;
            ManipulationStarting += Viewer_ManipulationStarting;
            ManipulationDelta += Viewer_ManipulationDelta;

            PreviewMouseWheel += Viewer_MouseWheel;

            FontFamily = new FontFamily(TranslationHelper.Get("Editor_FontFamily"));

            TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());

            LoadFileAsync(path);
        }

        public void Dispose()
        {
            _disposed = true;
        }

        private void Viewer_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            e.TranslationBehavior = new InertiaTranslationBehavior
            {
                InitialVelocity = e.InitialVelocities.LinearVelocity,
                DesiredDeceleration = 10.0 * 96.0 / (1000.0 * 1000.0)
            };
        }

        private void Viewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            ScrollToVerticalOffset(VerticalOffset - e.Delta);
        }

        private void Viewer_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;

            var delta = e.DeltaManipulation;
            ScrollToVerticalOffset(VerticalOffset - delta.Translation.Y);
        }

        private void Viewer_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.Mode = ManipulationModes.Translate;
        }

        private class TruncateLongLines : VisualLineElementGenerator
        {
            const int maxLength = 10000;
            const string ellipsis = "……………";

            public override int GetFirstInterestedOffset(int startOffset)
            {
                var line = CurrentContext.VisualLine.LastDocumentLine;
                if (line.Length > maxLength)
                {
                    int ellipsisOffset = line.Offset + maxLength - ellipsis.Length;
                    if (startOffset <= ellipsisOffset)
                        return ellipsisOffset;
                }
                return -1;
            }

            public override VisualLineElement ConstructElement(int offset)
            {
                return new FormattedTextElement(ellipsis, CurrentContext.VisualLine.LastDocumentLine.EndOffset - offset);
            }
        }

        private void LoadFileAsync(string path)
        {
            Task.Run(() =>
            {
                const int maxLength = 5 * 1024 * 1024;
                
                var buffer = new MemoryStream();
                bool tooLong;
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var lb = new byte[8192];
                    tooLong = fs.Length > maxLength;
                    while (fs.Position < fs.Length && buffer.Length < maxLength)
                    {
                        if (_disposed)
                            break;
                        int len = fs.Read(lb, 0, lb.Length);
                        buffer.Write(lb, 0, len);
                    }
                }
                if (_disposed)
                    return;

                if (tooLong)
                    _context.Title += " (0 ~ 5MB)";

                var bufferCopy = buffer.ToArray();
                buffer.Dispose();

                //edit by gh -   
                //使用NChardet解决大部分编码识别问题
                var encoding = EncodingExtensions.GetEncoding(path, bufferCopy.Length);
                //var encoding = EncodingExtensions.GetEncoding_utf(bufferCopy);
                //-----------

                var doc = new TextDocument(encoding.GetString(bufferCopy));
                doc.SetOwnerThread(Dispatcher.Thread);

                if (_disposed)
                    return;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Encoding = encoding;
                    SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(path));
                    Document = doc;

                    _context.IsBusy = false;
                }), DispatcherPriority.Render);
            });
        }

    }
}
