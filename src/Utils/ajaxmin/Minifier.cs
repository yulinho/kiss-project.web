// Minifier.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Kiss.Web.Utils.ajaxmin
{
    /// <summary>
    /// Minifier class for quick minification of JavaScript or Stylesheet code without needing to
    /// access or modify any abstract syntax tree nodes. Just put in source code and get our minified
    /// code as strings.
    /// </summary>
    public class Minifier
    {
        #region Properties

        /// <summary>
        /// Warning level threshold for reporting errors.
        /// Default value is zero: syntax/run-time errors.
        /// </summary>
        public int WarningLevel
        {
            get; set;
        }

        /// <summary>
        /// File name to use in error reporting.
        /// Default value is null: use Minify... method name.
        /// </summary>
        public string FileName
        {
            get; set;
        }

        /// <summary>
        /// Collection of ContextError objects found during minification process
        /// </summary>
        public ICollection<ContextError> ErrorList { get { return m_errorList; } }
        private List<ContextError> m_errorList; // = null;

        /// <summary>
        /// Collection of any error strings found during the crunch process.
        /// </summary>
        public ICollection<string> Errors
        {
            get 
            { 
                var errorList = new List<string>(ErrorList.Count);
                foreach (var error in ErrorList)
                {
                    errorList.Add(error.ToString());
                }
                return errorList;
            }
        }

        #endregion

        #region JavaScript methods

        /// <summary>
        /// MinifyJavaScript JS string passed to it using default code minification settings.
        /// The ErrorList property will be set with any errors found during the minification process.
        /// </summary>
        /// <param name="source">source Javascript</param>
        /// <returns>minified Javascript</returns>
        public string MinifyJavaScript(string source)
        {
            // just pass in default settings
            return MinifyJavaScript(source, new CodeSettings());
        }

        /// <summary>
        /// Crunched JS string passed to it, returning crunched string.
        /// The ErrorList property will be set with any errors found during the minification process.
        /// </summary>
        /// <param name="source">source Javascript</param>
        /// <param name="codeSettings">code minification settings</param>
        /// <returns>minified Javascript</returns>
        public string MinifyJavaScript(string source, CodeSettings codeSettings)
        {
            // default is an empty string
            var crunched = string.Empty;

            // reset the errors builder
            m_errorList = new List<ContextError>();

            // create the parser from the source string.
            // pass null for the assumed globals array
            var parser = new JSParser(source);

            // file context is a property on the parser
            parser.FileContext = FileName;

            // hook the engine error event
            parser.CompilerError += OnJavaScriptError;

            try
            {
                if (codeSettings != null && codeSettings.PreprocessOnly)
                {
                    // just run through the preprocessor only
                    crunched = parser.PreprocessOnly(codeSettings);
                }
                else
                {
                    // parse the input
                    var scriptBlock = parser.Parse(codeSettings);
                    if (scriptBlock != null)
                    {
                        // we'll return the crunched code
                        if (codeSettings != null && codeSettings.Format == JavaScriptFormat.JSON)
                        {
                            // we're going to use a different output visitor -- one
                            // that specifically returns valid JSON.
                            var sb = new StringBuilder();
                            using (var stringWriter = new StringWriter(sb, CultureInfo.InvariantCulture))
                            {
                                if (!JSONOutputVisitor.Apply(stringWriter, scriptBlock))
                                {
                                    m_errorList.Add(new ContextError(
                                        true,
                                        0,
                                        null,
                                        null,
                                        null,
                                        this.FileName,
                                        0,
                                        0,
                                        0,
                                        0,
                                        JScript.InvalidJSONOutput));
                                }
                            }

                            crunched = sb.ToString();
                        }
                        else
                        {
                            // just use the normal output visitor
                            crunched = scriptBlock.ToCode();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_errorList.Add(new ContextError(
                    true,
                    0,
                    null,
                    null,
                    null,
                    this.FileName,
                    0,
                    0,
                    0,
                    0,
                    e.Message));
                throw;
            }

            return crunched;
        }

        #endregion

        #region CSS methods

#if !JSONLY
        /// <summary>
        /// MinifyJavaScript CSS string passed to it using default code minification settings.
        /// The ErrorList property will be set with any errors found during the minification process.
        /// </summary>
        /// <param name="source">source Javascript</param>
        /// <returns>minified Javascript</returns>
        public string MinifyStyleSheet(string source)
        {
            // just pass in default settings
            return MinifyStyleSheet(source, new CssSettings(), new CodeSettings());
        }

        /// <summary>
        /// Minifies the CSS stylesheet passes to it using the given settings, returning the minified results
        /// The ErrorList property will be set with any errors found during the minification process.
        /// </summary>
        /// <param name="source">CSS Source</param>
        /// <param name="settings">CSS minification settings</param>
        /// <returns>Minified StyleSheet</returns>
        public string MinifyStyleSheet(string source, CssSettings settings)
        {
            // just pass in default settings
            return MinifyStyleSheet(source, settings, new CodeSettings());
        }

        /// <summary>
        /// Minifies the CSS stylesheet passes to it using the given settings, returning the minified results
        /// The ErrorList property will be set with any errors found during the minification process.
        /// </summary>
        /// <param name="source">CSS Source</param>
        /// <param name="settings">CSS minification settings</param>
        /// <param name="scriptSettings">JS minification settings to use for expression-minification</param>
        /// <returns>Minified StyleSheet</returns>
        public string MinifyStyleSheet(string source, CssSettings settings, CodeSettings scriptSettings)
        {
            // initialize some values, including the error list (which shoudl start off empty)
            string minifiedResults = string.Empty;
            m_errorList = new List<ContextError>();

            // create the parser object and if we specified some settings,
            // use it to set the Parser's settings object
            CssParser parser = new CssParser();
            parser.FileContext = FileName;

            if (settings != null)
            {
                parser.Settings = settings;
            }

            if (scriptSettings != null)
            {
                parser.JSSettings = scriptSettings;
            }

            // hook the error handler
            parser.CssError += new EventHandler<CssErrorEventArgs>(OnCssError);

            // try parsing the source and return the results
            try
            {
                minifiedResults = parser.Parse(source);
            }
            catch (Exception e)
            {
                m_errorList.Add(new ContextError(
                    true,
                    0,
                    null,
                    null,
                    null,
                    this.FileName,
                    0,
                    0,
                    0,
                    0,
                    e.Message));
                throw;
            }
            return minifiedResults;
        }
#endif
        #endregion

        #region Error-handling Members

#if !JSONLY
        private void OnCssError(object sender, CssErrorEventArgs e)
        {
            ContextError error = e.Error;
            if (error.Severity <= WarningLevel)
            {
                m_errorList.Add(error);
            }
        }
#endif

        private void OnJavaScriptError(object sender, JScriptExceptionEventArgs e)
        {
            ContextError error = e.Error;
            if (error.Severity <= WarningLevel)
            {
                m_errorList.Add(error);
            }
        }

        #endregion
    }
}