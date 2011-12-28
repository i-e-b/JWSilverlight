using System;

namespace ComposerCore.InnerWorkings
{
    public struct CaptionItem
    {
		public TimeSpan Time { get; set; }
		public TimeSpan End { get; set; }
        public string Text { get; set; }
    }
}
