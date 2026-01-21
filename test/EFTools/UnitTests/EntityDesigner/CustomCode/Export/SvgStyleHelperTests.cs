// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View.Export
{
    using System.Drawing;
    using Xunit;

    public class SvgStyleHelperTests
    {
        [Fact]
        public void ToSvgColor_converts_color_to_hex()
        {
            Assert.Equal("#FF0000", SvgStylesheetManager.ToSvgColor(Color.Red));
            Assert.Equal("#00FF00", SvgStylesheetManager.ToSvgColor(Color.Lime));
            Assert.Equal("#0000FF", SvgStylesheetManager.ToSvgColor(Color.Blue));
            Assert.Equal("#000000", SvgStylesheetManager.ToSvgColor(Color.Black));
            Assert.Equal("#FFFFFF", SvgStylesheetManager.ToSvgColor(Color.White));
        }

        [Fact]
        public void ToSvgColor_returns_none_for_transparent()
        {
            Assert.Equal("none", SvgStylesheetManager.ToSvgColor(Color.Transparent));
        }

        [Fact]
        public void FormatDouble_uses_invariant_culture()
        {
            Assert.Equal("123.46", SvgStylesheetManager.FormatDouble(123.456));
            Assert.Equal("0.00", SvgStylesheetManager.FormatDouble(0));
            Assert.Equal("-50.50", SvgStylesheetManager.FormatDouble(-50.5));
        }

        [Fact]
        public void GetTextColorForFill_returns_white_for_dark_fills()
        {
            Assert.Equal(Color.White, SvgStylesheetManager.GetTextColorForFill(Color.Black));
            Assert.Equal(Color.White, SvgStylesheetManager.GetTextColorForFill(Color.DarkBlue));
            Assert.Equal(Color.White, SvgStylesheetManager.GetTextColorForFill(Color.DarkGreen));
        }

        [Fact]
        public void GetTextColorForFill_returns_black_for_light_fills()
        {
            Assert.Equal(Color.Black, SvgStylesheetManager.GetTextColorForFill(Color.White));
            Assert.Equal(Color.Black, SvgStylesheetManager.GetTextColorForFill(Color.Yellow));
            Assert.Equal(Color.Black, SvgStylesheetManager.GetTextColorForFill(Color.LightGray));
        }

        [Fact]
        public void GetDashArray_creates_comma_separated_values()
        {
            var pattern = new float[] { 5, 3 };
            Assert.Equal("5.00,3.00", SvgStylesheetManager.GetDashArray(pattern));
        }

        [Fact]
        public void GetDashArray_returns_null_for_empty_pattern()
        {
            Assert.Null(SvgStylesheetManager.GetDashArray(new float[0]));
            Assert.Null(SvgStylesheetManager.GetDashArray(null));
        }

        [Fact]
        public void EscapeXml_escapes_special_characters()
        {
            Assert.Equal("&amp;", SvgStylesheetManager.EscapeXml("&"));
            Assert.Equal("&lt;", SvgStylesheetManager.EscapeXml("<"));
            Assert.Equal("&gt;", SvgStylesheetManager.EscapeXml(">"));
            Assert.Equal("&quot;", SvgStylesheetManager.EscapeXml("\""));
            Assert.Equal("&apos;", SvgStylesheetManager.EscapeXml("'"));
        }

        [Fact]
        public void EscapeXml_escapes_combined_special_characters()
        {
            Assert.Equal("&lt;div class=&quot;test&quot;&gt;A &amp; B&lt;/div&gt;",
SvgStylesheetManager.EscapeXml("<div class=\"test\">A & B</div>"));
        }

        [Fact]
        public void EscapeXml_returns_input_for_null_or_empty()
        {
            Assert.Null(SvgStylesheetManager.EscapeXml(null));
            Assert.Equal(string.Empty, SvgStylesheetManager.EscapeXml(string.Empty));
        }

        [Fact]
        public void EscapeXml_preserves_normal_text()
        {
            Assert.Equal("Hello World", SvgStylesheetManager.EscapeXml("Hello World"));
            Assert.Equal("CustomerOrder", SvgStylesheetManager.EscapeXml("CustomerOrder"));
        }

        [Fact]
        public void GetFontFamily_returns_default_for_null()
        {
            Assert.Equal("Segoe UI, Arial, sans-serif", SvgStylesheetManager.GetFontFamily(null));
        }

        [Fact]
        public void GetFontSize_returns_default_for_null()
        {
            Assert.Equal("12", SvgStylesheetManager.GetFontSize(null));
        }

        [Fact]
        public void GetFontWeight_returns_normal_for_null()
        {
            Assert.Equal("normal", SvgStylesheetManager.GetFontWeight(null));
        }

        [Fact]
        public void GetFontStyle_returns_normal_for_null()
        {
            Assert.Equal("normal", SvgStylesheetManager.GetFontStyle(null));
        }

        [Fact]
        public void ToSvgColorWithOpacity_handles_semi_transparent_colors()
        {
            var semiTransparent = Color.FromArgb(128, 255, 0, 0);
            string opacity;
            var color = SvgStylesheetManager.ToSvgColorWithOpacity(semiTransparent, out opacity);

            Assert.Equal("#FF0000", color);
            Assert.NotNull(opacity);
            Assert.Equal("0.50", opacity);
        }

        [Fact]
        public void ToSvgColorWithOpacity_returns_null_opacity_for_opaque_colors()
        {
            string opacity;
            var color = SvgStylesheetManager.ToSvgColorWithOpacity(Color.Red, out opacity);

            Assert.Equal("#FF0000", color);
            Assert.Null(opacity);
        }
    }
}
