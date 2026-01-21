// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View.Export
{
    using Xunit;

    public class SvgStylesheetManagerTests
    {
        private readonly SvgStylesheetManager _manager;

        public SvgStylesheetManagerTests()
        {
            _manager = new SvgStylesheetManager();
        }

        #region GetStyleDefinitions Tests

        [Fact]
        public void GetStyleDefinitions_returns_style_element()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains("<style>", result);
            Assert.Contains("</style>", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_icon_size_classes()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".icon {", result);
            Assert.Contains("width: 16px", result);
            Assert.Contains("height: 16px", result);
            Assert.Contains(".icon-sm {", result);
            Assert.Contains("width: 14px", result);
            Assert.Contains("height: 14px", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_icon_fill_classes()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".icon-shadow {", result);
            Assert.Contains(".icon-fill {", result);
            Assert.Contains(".icon-accent-shadow {", result);
            Assert.Contains(".icon-accent {", result);
            Assert.Contains(".icon-blue {", result);
            Assert.Contains(".icon-muted {", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_text_base_class()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".text-base {", result);
            Assert.Contains("font-family: Segoe UI", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_text_header_class()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".text-header {", result);
            Assert.Contains("font-size: 11px", result);
            Assert.Contains("fill: #FFFFFF", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_text_entity_class()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".text-entity {", result);
            Assert.Contains("font-size: 12px", result);
            Assert.Contains("font-weight: bold", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_text_property_class()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".text-property {", result);
            Assert.Contains("font-size: 11px", result);
            Assert.Contains("fill: #000000", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_text_mult_class()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".text-mult {", result);
            Assert.Contains("font-size: 11px", result);
            Assert.Contains("font-weight: bold", result);
        }

        [Fact]
        public void GetStyleDefinitions_includes_compartment_header_class()
        {
            var result = _manager.GetStyleDefinitions();

            Assert.Contains(".header-compartment {", result);
            Assert.Contains("fill: #E0E0E0", result);
            Assert.Contains("height: 24px", result);
        }

        [Fact]
        public void GetStyleDefinitions_uses_proper_indentation()
        {
            var result = _manager.GetStyleDefinitions();

            // Should start with proper indentation for embedding in SVG defs
            Assert.True(result.StartsWith("    <style>"));
        }

        #endregion
    }
}
