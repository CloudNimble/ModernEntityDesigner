// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View.Export
{
    using System;
    using System.Drawing;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.Modeling;
    using Xunit;
    using Xunit.Extensions;

    public class SvgShapeRendererTests : IDisposable
    {
        private readonly SvgIconManager _iconManager;
        private readonly SvgShapeRenderer _renderer;
        private readonly Store _store;

        public SvgShapeRendererTests()
        {
            _iconManager = new SvgIconManager();
            _renderer = new SvgShapeRenderer(_iconManager);
            _store = DslTestHelper.CreateStore();
        }

        public void Dispose()
        {
            if (_store != null)
            {
                _store.Dispose();
            }
        }

        #region RenderCompartmentHeader Tests

        [Fact]
        public void RenderCompartmentHeader_renders_dark_gray_background_rectangle()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", true, 10, 50, 200);

            var result = sb.ToString();
            Assert.Contains("<rect", result);
            Assert.Contains("x=\"10.00\"", result);
            Assert.Contains("y=\"50.00\"", result);
            Assert.Contains("width=\"200.00\"", result);
            Assert.Contains("class=\"header-compartment\"", result);
        }

        [Fact]
        public void RenderCompartmentHeader_renders_Collapse_icon_when_expanded()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", isExpanded: true, 10, 50, 200);

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-Collapse\"", result);
        }

        [Fact]
        public void RenderCompartmentHeader_renders_Expand_icon_when_collapsed()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", isExpanded: false, 10, 50, 200);

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-Expand\"", result);
        }

        [Fact]
        public void RenderCompartmentHeader_renders_text_label_with_css_class()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", true, 10, 50, 200);

            var result = sb.ToString();
            Assert.Contains("<text", result);
            Assert.Contains("class=\"text-base text-compartment\"", result);
            Assert.Contains(">Properties</text>", result);
        }

        [Fact]
        public void RenderCompartmentHeader_renders_NavigationProperties_label()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Navigation Properties", true, 10, 50, 200);

            var result = sb.ToString();
            Assert.Contains(">Navigation Properties</text>", result);
        }

        [Fact]
        public void RenderCompartmentHeader_escapes_special_xml_characters_in_header_text()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Props & Nav <test>", true, 10, 50, 200);

            var result = sb.ToString();
            Assert.Contains("&amp;", result);
            Assert.Contains("&lt;", result);
            Assert.Contains("&gt;", result);
        }

        [Theory]
        [InlineData(0, 0, 100)]
        [InlineData(50, 100, 250)]
        [InlineData(123.5, 456.7, 300)]
        public void RenderCompartmentHeader_positions_elements_correctly(double x, double y, double width)
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", true, x, y, width);

            var result = sb.ToString();
            Assert.Contains(string.Format("x=\"{0}\"", SvgStylesheetManager.FormatDouble(x)), result);
            Assert.Contains(string.Format("y=\"{0}\"", SvgStylesheetManager.FormatDouble(y)), result);
            Assert.Contains(string.Format("width=\"{0}\"", SvgStylesheetManager.FormatDouble(width)), result);
        }

        [Fact]
        public void RenderCompartmentHeader_chevron_is_positioned_with_padding()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", true, 10, 50, 200);

            var result = sb.ToString();
            // Chevron X should be x + chevronPadding (2.0) = 12.0
            // Chevron Y should be GetCenteredIconY(50, 24, 14) = 50 + (24 - 14) / 2 = 55.0
            Assert.Contains("x=\"12.00\"", result);
            Assert.Contains("y=\"55.00\"", result);
        }

        [Fact]
        public void RenderCompartmentHeader_text_is_positioned_after_chevron()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", true, 10, 50, 200);

            var result = sb.ToString();
            // Text X should be x + chevronSize (14) + 6 = 10 + 14 + 6 = 30
            Assert.Contains("x=\"30.00\"", result);
            // Text Y should be GetCenteredTextBaselineY(50, 24, 11) = 50 + 12 + 3.85 = 65.85
            Assert.Contains("y=\"65.85\"", result);
        }

        [Fact]
        public void RenderCompartmentHeader_uses_css_classes_for_text_styling()
        {
            var sb = new StringBuilder();
            _renderer.RenderCompartmentHeader(sb, "Properties", true, 10, 50, 200);

            var result = sb.ToString();
            // Should use CSS classes instead of inline styles
            Assert.Contains("class=\"text-base text-compartment\"", result);
            // Should NOT contain inline font styles
            Assert.DoesNotContain("font-family=", result);
            Assert.DoesNotContain("font-size=", result);
        }

        #endregion

        #region GetOutlineColor Tests

        [Fact]
        public void GetOutlineColor_returns_lighter_color_for_dark_fill()
        {
            var darkColor = Color.FromArgb(50, 50, 50);
            var outlineColor = _renderer.GetOutlineColor(darkColor);

            // Should be lighter than input
            var brightness = (outlineColor.R + outlineColor.G + outlineColor.B) / 3.0;
            var originalBrightness = (darkColor.R + darkColor.G + darkColor.B) / 3.0;
            Assert.True(brightness > originalBrightness);
        }

        [Fact]
        public void GetOutlineColor_returns_darker_color_for_light_fill()
        {
            var lightColor = Color.FromArgb(200, 200, 200);
            var outlineColor = _renderer.GetOutlineColor(lightColor);

            // Should be darker than input
            var brightness = (outlineColor.R + outlineColor.G + outlineColor.B) / 3.0;
            var originalBrightness = (lightColor.R + lightColor.G + lightColor.B) / 3.0;
            Assert.True(brightness < originalBrightness);
        }

        [Fact]
        public void GetOutlineColor_handles_pure_black()
        {
            var outlineColor = _renderer.GetOutlineColor(Color.Black);

            // Should return a valid color (slightly lighter)
            Assert.NotEqual(Color.Black, outlineColor);
        }

        [Fact]
        public void GetOutlineColor_handles_pure_white()
        {
            var outlineColor = _renderer.GetOutlineColor(Color.White);

            // Should return a valid color (slightly darker)
            Assert.NotEqual(Color.White, outlineColor);
        }

        #endregion

        #region GetPropertyDisplayText Tests

        [Fact]
        public void GetPropertyDisplayText_with_null_item_returns_property_fallback()
        {
            var result = _renderer.GetPropertyDisplayText(null, isScalarProperty: true, showTypes: false);

            Assert.Equal("Property", result);
        }

        [Fact]
        public void GetPropertyDisplayText_with_scalar_property_returns_name()
        {
            var prop = DslTestHelper.CreateScalarProperty(_store, "CustomerName", "String", false);

            var result = _renderer.GetPropertyDisplayText(prop, isScalarProperty: true, showTypes: false);

            Assert.Equal("CustomerName", result);
        }

        [Fact]
        public void GetPropertyDisplayText_with_scalar_property_and_show_types_returns_name_and_type()
        {
            var prop = DslTestHelper.CreateScalarProperty(_store, "CustomerName", "String", false);

            var result = _renderer.GetPropertyDisplayText(prop, isScalarProperty: true, showTypes: true);

            Assert.Equal("CustomerName : String", result);
        }

        [Fact]
        public void GetPropertyDisplayText_with_navigation_property_returns_name()
        {
            var prop = DslTestHelper.CreateNavigationProperty(_store, "Orders");

            var result = _renderer.GetPropertyDisplayText(prop, isScalarProperty: false, showTypes: false);

            Assert.Equal("Orders", result);
        }

        [Fact]
        public void GetPropertyDisplayText_with_complex_property_returns_name()
        {
            var prop = DslTestHelper.CreateComplexProperty(_store, "Address");

            var result = _renderer.GetPropertyDisplayText(prop, isScalarProperty: true, showTypes: false);

            Assert.Equal("Address", result);
        }

        #endregion

        #region RenderPropertyIcon Tests

        [Fact]
        public void RenderPropertyIcon_uses_default_Property_icon_for_unknown_type()
        {
            var sb = new StringBuilder();

            _renderer.RenderPropertyIcon(sb, 10, 20, new object());

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-Property\"", result);
        }

        [Fact]
        public void RenderPropertyIcon_uses_PropertyKey_icon_for_entity_key()
        {
            var prop = DslTestHelper.CreateScalarProperty(_store, "Id", "Int32", isKey: true);
            var sb = new StringBuilder();

            _renderer.RenderPropertyIcon(sb, 10, 20, prop);

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-PropertyKey\"", result);
        }

        [Fact]
        public void RenderPropertyIcon_uses_Property_icon_for_non_key_scalar()
        {
            var prop = DslTestHelper.CreateScalarProperty(_store, "Name", "String", isKey: false);
            var sb = new StringBuilder();

            _renderer.RenderPropertyIcon(sb, 10, 20, prop);

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-Property\"", result);
        }

        [Fact]
        public void RenderPropertyIcon_uses_ComplexProperty_icon_for_complex_property()
        {
            var prop = DslTestHelper.CreateComplexProperty(_store, "Address");
            var sb = new StringBuilder();

            _renderer.RenderPropertyIcon(sb, 10, 20, prop);

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-ComplexProperty\"", result);
        }

        [Fact]
        public void RenderPropertyIcon_uses_NavigationProperty_icon_for_navigation_property()
        {
            var prop = DslTestHelper.CreateNavigationProperty(_store, "Orders");
            var sb = new StringBuilder();

            _renderer.RenderPropertyIcon(sb, 10, 20, prop);

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-NavigationProperty\"", result);
        }

        #endregion

        #region RenderEntityIcon Tests

        [Fact]
        public void RenderEntityIcon_uses_Class_icon()
        {
            var sb = new StringBuilder();

            _renderer.RenderEntityIcon(sb, 10, 20, Color.White);

            var result = sb.ToString();
            Assert.Contains("href=\"#icon-Class\"", result);
        }

        [Fact]
        public void RenderEntityIcon_appends_icon_reference_to_builder()
        {
            var sb = new StringBuilder();

            _renderer.RenderEntityIcon(sb, 10, 20, Color.White);

            var result = sb.ToString();
            Assert.Contains("<use href=\"#icon-Class\"", result);
        }

        #endregion

        #region Vertical Centering Tests

        [Theory]
        [InlineData(0, 30, 16, 7)]      // Entity header: (30-16)/2 = 7
        [InlineData(0, 24, 14, 5)]      // Compartment header: (24-14)/2 = 5
        [InlineData(0, 18, 14, 2)]      // Property row: (18-14)/2 = 2
        [InlineData(100, 40, 20, 110)]  // Offset container: 100 + (40-20)/2 = 110
        public void GetCenteredIconY_returns_correct_position(
            double containerY, double containerHeight, double iconSize, double expectedY)
        {
            var result = SvgShapeRenderer.GetCenteredIconY(containerY, containerHeight, iconSize);
            Assert.Equal(expectedY, result, precision: 2);
        }

        [Theory]
        [InlineData(0, 30, 12, 19.2)]   // Entity header: 15 + (12*0.7/2) = 15 + 4.2 = 19.2
        [InlineData(0, 24, 11, 15.85)]  // Compartment header: 12 + (11*0.7/2) = 12 + 3.85 = 15.85
        [InlineData(0, 18, 11, 12.85)]  // Property row: 9 + (11*0.7/2) = 9 + 3.85 = 12.85
        public void GetCenteredTextBaselineY_returns_correct_position(
            double containerY, double containerHeight, double fontSizePx, double expectedY)
        {
            var result = SvgShapeRenderer.GetCenteredTextBaselineY(containerY, containerHeight, fontSizePx);
            Assert.Equal(expectedY, result, precision: 2);
        }

        [Fact]
        public void Centering_formulas_align_icon_and_text_to_same_centerline()
        {
            const double containerHeight = 30;
            const double iconSize = 16;
            const double fontSizePx = 12;
            const double capHeightRatio = 0.70;

            var iconY = SvgShapeRenderer.GetCenteredIconY(0, containerHeight, iconSize);
            var textBaselineY = SvgShapeRenderer.GetCenteredTextBaselineY(0, containerHeight, fontSizePx);

            // Icon visual center
            var iconCenter = iconY + iconSize / 2;

            // Text visual center (capHeight/2 above baseline)
            var capHeight = fontSizePx * capHeightRatio;
            var textVisualCenter = textBaselineY - capHeight / 2;

            // Both should equal containerHeight / 2
            Assert.Equal(containerHeight / 2, iconCenter, precision: 2);
            Assert.Equal(containerHeight / 2, textVisualCenter, precision: 2);
        }

        #endregion
    }
}
