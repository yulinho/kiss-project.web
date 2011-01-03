﻿using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using Kiss.Utils;

namespace Kiss.Web.Controls
{
    /// <summary>
    /// Encapsulated rendering of style based on the selected skin.
    /// </summary>
    public class Style : Control, IContextAwaredControl
    {
        private string _media;
        /// <summary>
        /// Property Media (string)
        /// </summary>
        [DefaultValue("screen")]
        public virtual String Media
        {
            get
            {
                if (string.IsNullOrEmpty(_media))
                    return "screen";
                return _media;
            }
            set
            {
                _media = value;
            }
        }

        public bool IncludeYUI { get; set; }

        public bool IncludeButtonStyle { get; set; }

        private string _href;
        /// <summary>
        /// Property Href (string)
        /// </summary>
        public virtual String Href
        {
            get
            {
                if (!string.IsNullOrEmpty(_href))
                {
                    if (_href.StartsWith("/") || _href.StartsWith(".") || _href.StartsWith("~"))
                        return Utility.FormatCssUrl(CurrentSite, ResolveUrl(_href));

                    if (_href.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                        return _href;

                    return StringUtil.CombinUrl(Utility.FormatCssUrl(CurrentSite, string.Format(CurrentSite.CssRoot, JContext.Current.Theme)), _href);
                }
                else
                    return string.Empty;
            }
            set
            {
                _href = value;
            }
        }

        private StyleRelativePosition _relativePosition = StyleRelativePosition.First;
        /// <summary>
        /// Property RelativePosition (StyleRelativePosition) 
        /// This is used when enqueue for rendering in the CS Head
        /// </summary>
        public virtual StyleRelativePosition RelativePosition
        {
            get
            {
                return _relativePosition;
            }
            set
            {
                _relativePosition = value;
            }
        }

        private bool _enqueue = true;
        /// <summary>
        /// Property Enqueue (Bool) 
        /// if true, the control does not render, but enques itself
        /// to be rendered in the JC:Head control
        /// </summary>
        public virtual bool Enqueue
        {
            get
            {
                return _enqueue;
            }
            set
            {
                _enqueue = value;
            }
        }

        const string linkFormat = "<link rel=\"stylesheet\" href=\"{0}\" type=\"text/css\" media=\"{1}\" />";

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ISite site = CurrentSite ?? JContext.Current.Site;

            string href = Href;
            if (!string.IsNullOrEmpty(href))
            {
                if (!site.CombinCss)
                {
                    if (href.Contains("?"))
                        href += ("&v=" + site.CssVersion);
                    else
                        href += ("?v=" + site.CssVersion);
                }

                Head.AddStyle(site, href,
                    this.Media,
                    HttpContext.Current,
                    this.RelativePosition,
                    _enqueue);
            }

            if (IncludeYUI)
                ClientScriptProxy.Current.RegisterCssResource("Kiss.Web.jQuery"
                    , "Kiss.Web.jQuery.yui.css");

            if (IncludeButtonStyle)
                ClientScriptProxy.Current.RegisterCssResource("Kiss.Web.jQuery"
                    , "Kiss.Web.jQuery.btn.css");
        }

        protected override void Render(HtmlTextWriter output)
        {
        }

        private ISite _site;
        public ISite CurrentSite { get { return _site ?? JContext.Current.Site; } set { _site = value; } }
    }

    /// <summary>
    /// 定义Render Css的顺序
    /// </summary>
    public enum StyleRelativePosition
    {

        /// <summary>
        /// rendered before unspecified and last items
        /// </summary>
        First = 1,

        /// <summary>
        /// rendered after first and unspecified items
        /// </summary>
        Last = 2,

        /// <summary>
        /// The default render location..  renderes between first and last items
        /// </summary>
        Unspecified = 3
    }

    class StyleQueueItem
    {
        public StyleRelativePosition Position;
        public string StyleTag;
        public string Url { get; set; }
        public ISite Site { get; set; }
        public bool ForceCombin { get; set; }

        public StyleQueueItem(ISite site, string styleTag, StyleRelativePosition position, string url, bool forceCombin)
        {
            Site = site;
            StyleTag = styleTag;
            Position = position;
            Url = url;
            ForceCombin = forceCombin;
        }
    }
}
