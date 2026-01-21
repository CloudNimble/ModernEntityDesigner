// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View.Export
{
    using Xunit;

    public class SvgConnectorRendererTests
    {
        private readonly SvgConnectorRenderer _renderer;

        public SvgConnectorRendererTests()
        {
            _renderer = new SvgConnectorRenderer();
        }

        #region GetMarkerDefinitions Tests

        [Fact]
        public void GetMarkerDefinitions_returns_defs_element()
        {
            var result = _renderer.GetMarkerDefinitions();

            Assert.Contains("<defs>", result);
            Assert.Contains("</defs>", result);
        }

        [Fact]
        public void GetMarkerDefinitions_includes_diamond_marker()
        {
            var result = _renderer.GetMarkerDefinitions();

            Assert.Contains("id=\"diamond\"", result);
            Assert.Contains("<polygon", result);
        }

        [Fact]
        public void GetMarkerDefinitions_includes_inheritance_arrow_marker()
        {
            var result = _renderer.GetMarkerDefinitions();

            Assert.Contains("id=\"inheritance-arrow\"", result);
        }

        #endregion

        #region GetMarkerDefinitionsContent Tests

        [Fact]
        public void GetMarkerDefinitionsContent_returns_marker_elements_without_defs_wrapper()
        {
            var result = _renderer.GetMarkerDefinitionsContent();

            Assert.Contains("<marker", result);
            Assert.DoesNotContain("<defs>", result);
            Assert.DoesNotContain("</defs>", result);
        }

        [Fact]
        public void GetMarkerDefinitionsContent_diamond_marker_has_correct_viewBox()
        {
            var result = _renderer.GetMarkerDefinitionsContent();

            Assert.Contains("id=\"diamond\"", result);
            Assert.Contains("viewBox=\"0 0 12 12\"", result);
        }

        [Fact]
        public void GetMarkerDefinitionsContent_diamond_marker_has_polygon_shape()
        {
            var result = _renderer.GetMarkerDefinitionsContent();

            // Diamond shape polygon points
            Assert.Contains("points=\"6,1 11,6 6,11 1,6\"", result);
        }

        [Fact]
        public void GetMarkerDefinitionsContent_inheritance_arrow_has_correct_viewBox()
        {
            var result = _renderer.GetMarkerDefinitionsContent();

            Assert.Contains("id=\"inheritance-arrow\"", result);
            Assert.Contains("viewBox=\"0 0 12 12\"", result);
        }

        [Fact]
        public void GetMarkerDefinitionsContent_inheritance_arrow_uses_hollow_css_class()
        {
            var result = _renderer.GetMarkerDefinitionsContent();

            // Hollow arrow uses CSS class for fill="none" styling
            Assert.Contains("class=\"arrow-hollow\"", result);
        }

        [Fact]
        public void GetMarkerDefinitionsContent_diamond_uses_css_class()
        {
            var result = _renderer.GetMarkerDefinitionsContent();

            // Diamond marker uses CSS class for styling
            Assert.Contains("class=\"diamond\"", result);
        }

        [Fact]
        public void GetMarkerDefinitionsContent_markers_have_auto_orient()
        {
            var result = _renderer.GetMarkerDefinitionsContent();

            Assert.Contains("orient=\"auto\"", result);
        }

        #endregion

        #region RenderAssociationConnector Tests

        [Fact]
        public void RenderAssociationConnector_with_null_connector_returns_empty_string()
        {
            var result = _renderer.RenderAssociationConnector(null, 0, 0);

            Assert.Equal(string.Empty, result);
        }

        #endregion

        #region RenderInheritanceConnector Tests

        [Fact]
        public void RenderInheritanceConnector_with_null_connector_returns_empty_string()
        {
            var result = _renderer.RenderInheritanceConnector(null, 0, 0);

            Assert.Equal(string.Empty, result);
        }

        #endregion
    }
}
