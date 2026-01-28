// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Windows;
using Microsoft.Data.Entity.Design.EntityDesigner.View;
using Microsoft.Data.Entity.Design.EntityDesigner.View.ContextMenu;
using Microsoft.Data.Entity.Design.EntityDesigner.View.Export;
using Microsoft.Data.Entity.Design.VisualStudio;
using Microsoft.Data.Entity.Design.VisualStudio.Package;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Data.Entity.Design.Package
{
    /// <summary>
    /// Service that provides a Windows 11-style context menu for the Entity Designer diagram surface.
    /// This menu appears when right-clicking on empty space (not on shapes).
    /// </summary>
    internal sealed class DiagramSurfaceContextMenuService : IDisposable
    {
        private readonly MicrosoftDataEntityDesignDocView _docView;
        private readonly DiagramClientView _diagramClientView;
        private DiagramSurfaceContextMenu _contextMenu;
        private bool _isDisposed;

        /// <summary>
        /// Creates a new context menu service for the specified diagram view.
        /// </summary>
        /// <param name="docView">The document view containing the diagram.</param>
        /// <param name="diagramClientView">The diagram client view to attach to.</param>
        public DiagramSurfaceContextMenuService(MicrosoftDataEntityDesignDocView docView, DiagramClientView diagramClientView)
        {
            _docView = docView ?? throw new ArgumentNullException(nameof(docView));
            _diagramClientView = diagramClientView ?? throw new ArgumentNullException(nameof(diagramClientView));
        }

        /// <summary>
        /// Determines if the click location is on the diagram surface (empty space)
        /// rather than on a shape or connector.
        /// </summary>
        /// <param name="mousePosition">The mouse position in world coordinates.</param>
        /// <returns>True if the click is on empty diagram space.</returns>
        public bool IsClickOnDiagramSurface(PointD mousePosition)
        {
            System.Diagnostics.Debug.WriteLine($"[ModernEntityDesigner] IsClickOnDiagramSurface called with position: {mousePosition}");

            var diagram = _diagramClientView.Diagram;
            if (diagram == null)
            {
                System.Diagnostics.Debug.WriteLine("[ModernEntityDesigner] Diagram is null, returning false");
                return false;
            }

            // Perform hit test - use DiagramHitTestInfo to get detailed results
            DiagramHitTestInfo hitTestInfo = new DiagramHitTestInfo(_diagramClientView);
            var hitResult = diagram.DoHitTest(mousePosition, hitTestInfo);
            System.Diagnostics.Debug.WriteLine($"[ModernEntityDesigner] DoHitTest returned: {hitResult}");

            if (!hitResult)
            {
                // No hit - click was on empty space
                System.Diagnostics.Debug.WriteLine("[ModernEntityDesigner] No hit, returning true (empty space)");
                return true;
            }

            // Check if we hit the diagram itself (not a shape)
            if (hitTestInfo.HitDiagramItem == null)
            {
                System.Diagnostics.Debug.WriteLine("[ModernEntityDesigner] HitDiagramItem is null, returning true");
                return true;
            }

            var hitShape = hitTestInfo.HitDiagramItem.Shape;
            System.Diagnostics.Debug.WriteLine($"[ModernEntityDesigner] HitShape type: {hitShape?.GetType().Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"[ModernEntityDesigner] HitShape == diagram: {hitShape == diagram}");

            var result = hitShape == diagram || hitShape == null;
            System.Diagnostics.Debug.WriteLine($"[ModernEntityDesigner] Returning: {result}");
            return result;
        }

        /// <summary>
        /// Shows the custom context menu at the specified position.
        /// </summary>
        /// <param name="diagram">The diagram.</param>
        /// <param name="mousePosition">The mouse position in world coordinates.</param>
        public void ShowCustomContextMenu(EntityDesignerDiagram diagram, PointD mousePosition)
        {
            // Create context menu if needed
            if (_contextMenu == null)
            {
                _contextMenu = new DiagramSurfaceContextMenu();
                _contextMenu.ActionExecuted += OnMenuActionExecuted;

                // Populate the default commands
                PopulateDefaultCommands();
            }

            // Update command states based on current diagram
            UpdateCommandStates(diagram);

            // Convert world coordinates to screen coordinates
            var clientPoint = _diagramClientView.WorldToDevice(mousePosition);
            var screenPoint = _diagramClientView.PointToScreen(new System.Drawing.Point((int)clientPoint.X, (int)clientPoint.Y));
            var wpfScreenPoint = new Point(screenPoint.X, screenPoint.Y);

            // Show the menu
            _contextMenu.Show(diagram, wpfScreenPoint);
        }

        /// <summary>
        /// Populates the context menu with default commands.
        /// </summary>
        private void PopulateDefaultCommands()
        {
            // Top bar commands - 5 icon buttons (64px each + 4 separators = 324px total)
            _contextMenu.TopBarCommands.Add(new MenuCommandDefinition(
                "UpdateModelFromDatabase",
                "Update",
                KnownMonikers.DatabaseModelRefresh,
                "Update the model from the database"));

            _contextMenu.TopBarCommands.Add(new MenuCommandDefinition(
                "GenerateDatabaseFromModel",
                "Generate",
                KnownMonikers.DatabaseScript,
                "Generate database scripts from the model"));

            _contextMenu.TopBarCommands.Add(new MenuCommandDefinition(
                "Export",
                "Export",
                KnownMonikers.SaveAs,
                "Export diagram as..."));

            _contextMenu.TopBarCommands.Add(new MenuCommandDefinition(
                "ZoomToFit",
                "Fit",
                KnownMonikers.FitToScreen,
                "Zoom to fit all entities"));

            _contextMenu.TopBarCommands.Add(new MenuCommandDefinition(
                "Layout",
                "Layout",
                KnownMonikers.ShowAllFiles,
                "Auto-arrange diagram layout"));

            // Add New submenu
            var addNewMenu = new MenuCommandDefinition(
                "AddNew",
                "Add New",
                KnownMonikers.AddItem,
                "Add new items to the model");

            addNewMenu.Children.Add(new MenuCommandDefinition(
                "AddEntity",
                "Entity...",
                KnownMonikers.AddEntity,
                "Add a new entity"));

            addNewMenu.Children.Add(new MenuCommandDefinition(
                "AddComplexType",
                "Complex Type",
                KnownMonikers.EntityContainer,
                "Add a new complex type"));

            addNewMenu.Children.Add(new MenuCommandDefinition(
                "AddEnumType",
                "Enum Type...",
                KnownMonikers.EnumerationPublic,
                "Add a new enum type"));

            addNewMenu.Children.Add(MenuSeparatorDefinition.Instance);

            addNewMenu.Children.Add(new MenuCommandDefinition(
                "AddAssociation",
                "Association...",
                KnownMonikers.AssociationRelationship,
                "Add a new association between entities"));

            addNewMenu.Children.Add(new MenuCommandDefinition(
                "AddInheritance",
                "Inheritance...",
                KnownMonikers.Inheritance,
                "Add an inheritance relationship"));

            addNewMenu.Children.Add(MenuSeparatorDefinition.Instance);

            addNewMenu.Children.Add(new MenuCommandDefinition(
                "AddFunctionImport",
                "Function Import...",
                KnownMonikers.Method,
                "Add a function import"));

            addNewMenu.Children.Add(new MenuCommandDefinition(
                "AddCodeGenerationItem",
                "Code Generation...",
                KnownMonikers.ModifyClass,
                "Add a code generation template"));

            _contextMenu.MenuItems.Add(addNewMenu);

            // Scalar Property Format submenu
            var formatMenu = new MenuCommandDefinition(
                "ScalarPropertyFormat",
                "Scalar Property Format",
                KnownMonikers.Property,
                "Property display format");

            formatMenu.Children.Add(new MenuCommandDefinition(
                "DisplayName",
                "Display Name",
                KnownMonikers.Property,
                "Show property names only"));

            formatMenu.Children.Add(new MenuCommandDefinition(
                "DisplayNameAndType",
                "Display Name and Type",
                KnownMonikers.PropertyPublic,
                "Show property names and types"));

            _contextMenu.MenuItems.Add(formatMenu);

            // Separator
            _contextMenu.MenuItems.Add(MenuSeparatorDefinition.Instance);

            // Select All
            _contextMenu.MenuItems.Add(new MenuCommandDefinition(
                "SelectAll",
                "Select All",
                KnownMonikers.SelectAll,
                "Select all entities")
            {
                KeyboardShortcut = "Ctrl+A"
            });

            // Separator
            _contextMenu.MenuItems.Add(MenuSeparatorDefinition.Instance);

            // Show group
            _contextMenu.MenuItems.Add(new MenuCommandDefinition(
                "ShowMappingDetails",
                "Mapping Details",
                KnownMonikers.Table,
                "Show mapping details window"));

            _contextMenu.MenuItems.Add(new MenuCommandDefinition(
                "ShowModelBrowser",
                "Model Browser",
                KnownMonikers.Property,
                "Show model browser window"));

            // Separator
            _contextMenu.MenuItems.Add(MenuSeparatorDefinition.Instance);

            // Validate
            _contextMenu.MenuItems.Add(new MenuCommandDefinition(
                "Validate",
                "Validate",
                KnownMonikers.Checkmark,
                "Validate the model"));

            // Open in XML Editor
            _contextMenu.MenuItems.Add(new MenuCommandDefinition(
                "OpenXmlEditor",
                "Open in XML Editor",
                KnownMonikers.XMLFile,
                "Open the model in the XML editor"));
        }

        /// <summary>
        /// Updates command states based on the current diagram state.
        /// </summary>
        private void UpdateCommandStates(EntityDesignerDiagram diagram)
        {
            if (diagram == null)
            {
                return;
            }

            int entityCount = diagram.ModelElement?.EntityTypes?.Count ?? 0;

            // Update top bar command states
            foreach (var cmd in _contextMenu.TopBarCommands)
            {
                if (cmd.Id == "AddAssociation")
                {
                    cmd.IsEnabled = entityCount >= 1;
                }
            }

            // Update menu item states recursively
            UpdateMenuItemStates(_contextMenu.MenuItems, diagram, entityCount);
        }

        private void UpdateMenuItemStates(System.Collections.ObjectModel.ObservableCollection<object> items, EntityDesignerDiagram diagram, int entityCount)
        {
            foreach (var item in items)
            {
                if (item is MenuCommandDefinition cmd)
                {
                    switch (cmd.Id)
                    {
                        case "AddAssociation":
                            cmd.IsEnabled = entityCount >= 1;
                            break;

                        case "AddInheritance":
                            cmd.IsEnabled = entityCount >= 2;
                            break;

                        case "DisplayName":
                            cmd.IsEnabled = diagram.DisplayNameAndType;
                            break;

                        case "DisplayNameAndType":
                            cmd.IsEnabled = !diagram.DisplayNameAndType;
                            break;
                    }

                    // Recursively update children
                    if (cmd.HasChildren)
                    {
                        UpdateMenuItemStates(cmd.Children, diagram, entityCount);
                    }
                }
            }
        }

        private void OnMenuActionExecuted(object sender, MenuActionEventArgs e)
        {
            var diagram = _docView.CurrentDiagram as EntityDesignerDiagram;
            if (diagram == null)
            {
                return;
            }

            // Execute the appropriate command
            switch (e.ActionName)
            {
                // Add New commands
                case "AddEntity":
                    ExecuteAddEntity(diagram);
                    break;

                case "AddComplexType":
                    ExecuteAddComplexType(diagram);
                    break;

                case "AddEnumType":
                    ExecuteAddEnumType(diagram);
                    break;

                case "AddAssociation":
                    ExecuteAddAssociation(diagram);
                    break;

                case "AddInheritance":
                    ExecuteAddInheritance(diagram);
                    break;

                case "AddFunctionImport":
                    ExecuteAddFunctionImport(diagram);
                    break;

                // Diagram commands
                case "Layout":
                    ExecuteLayout(diagram);
                    break;

                case "Export":
                    ExecuteExport(diagram);
                    break;

                case "CollapseAll":
                    ExecuteCollapseAll(diagram);
                    break;

                case "ExpandAll":
                    ExecuteExpandAll(diagram);
                    break;

                // Zoom commands (ZoomToFit is still in the top bar)
                case "ZoomToFit":
                    ExecuteZoomToFit(diagram);
                    break;

                // Scalar Property Format commands
                case "DisplayName":
                    ExecuteDisplayName(diagram);
                    break;

                case "DisplayNameAndType":
                    ExecuteDisplayNameAndType(diagram);
                    break;

                // Select All
                case "SelectAll":
                    ExecuteSelectAll(diagram);
                    break;

                // Show commands
                case "ShowMappingDetails":
                    ExecuteShowMappingDetails();
                    break;

                case "ShowModelBrowser":
                    ExecuteShowModelBrowser();
                    break;

                // Model/Database interaction commands
                case "UpdateModelFromDatabase":
                    ExecuteUpdateModelFromDatabase(diagram);
                    break;

                case "GenerateDatabaseFromModel":
                    ExecuteGenerateDatabaseFromModel(diagram);
                    break;

                case "AddCodeGenerationItem":
                    ExecuteAddCodeGenerationItem(diagram);
                    break;

                // Validate
                case "Validate":
                    ExecuteValidate(diagram);
                    break;

                // Open in XML Editor
                case "OpenXmlEditor":
                    ExecuteOpenXmlEditor(diagram);
                    break;
            }
        }

        #region Command Execution

        private void ExecuteAddEntity(EntityDesignerDiagram diagram)
        {
            // Use the existing AddNewEntityType method with a default position
            var dropPoint = GetCenterPoint();
            diagram.AddNewEntityType(dropPoint);
        }

        private void ExecuteAddComplexType(EntityDesignerDiagram diagram)
        {
            // Execute the Add Complex Type command via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.AddComplexType);
        }

        private void ExecuteAddEnumType(EntityDesignerDiagram diagram)
        {
            // Execute the Add Enum Type command via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.AddEnumType);
        }

        private void ExecuteAddAssociation(EntityDesignerDiagram diagram)
        {
            // Check if there are enough entities to create an association
            if (diagram.ModelElement.EntityTypes.Count < 1)
            {
                VsUtils.ShowMessageBox(
                    PackageManager.Package,
                    Resources.ContextMenu_AddAssociation_NoEntities,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_INFO);
                return;
            }

            diagram.AddNewAssociation(null);
        }

        private void ExecuteAddInheritance(EntityDesignerDiagram diagram)
        {
            // Check if there are enough entities
            if (diagram.ModelElement.EntityTypes.Count < 2)
            {
                VsUtils.ShowMessageBox(
                    PackageManager.Package,
                    Resources.ContextMenu_AddInheritance_NotEnoughEntities,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_INFO);
                return;
            }

            diagram.AddNewInheritance(null);
        }

        private void ExecuteAddFunctionImport(EntityDesignerDiagram diagram)
        {
            diagram.AddNewFunctionImport(null);
        }

        private void ExecuteLayout(EntityDesignerDiagram diagram)
        {
            diagram.AutoLayoutDiagram();
        }

        private void ExecuteExport(EntityDesignerDiagram diagram)
        {
            // Show the export dialog directly
            var modelName = diagram.ModelElement?.Namespace ?? "EntityModel";
            var diagramShowsTypes = diagram.DisplayNameAndType;

            var dialog = new ExportDiagramDialog(modelName, diagramShowsTypes);

            // Get the VS main window handle via IVsUIShell
            IServiceProvider sp = PackageManager.Package;
            if (sp != null)
            {
                var uiShell = sp.GetService(typeof(SVsUIShell)) as IVsUIShell;
                if (uiShell != null)
                {
                    uiShell.GetDialogOwnerHwnd(out IntPtr hwndOwner);
                    var hwnd = new System.Windows.Interop.WindowInteropHelper(dialog);
                    hwnd.Owner = hwndOwner;
                }
            }

            if (dialog.ShowDialog() == true)
            {
                var options = dialog.CreateExportOptions();
                var exportManager = new ExportManager();
                exportManager.Export(diagram, options);
            }
        }

        private void ExecuteCollapseAll(EntityDesignerDiagram diagram)
        {
            diagram.CollapseAllEntityTypeShapes();
        }

        private void ExecuteExpandAll(EntityDesignerDiagram diagram)
        {
            diagram.ExpandAllEntityTypeShapes();
        }

        private void ExecuteZoomToFit(EntityDesignerDiagram diagram)
        {
            diagram.ZoomToFit();
        }

        private void ExecuteDisplayName(EntityDesignerDiagram diagram)
        {
            diagram.DisplayNameAndType = false;
        }

        private void ExecuteDisplayNameAndType(EntityDesignerDiagram diagram)
        {
            diagram.DisplayNameAndType = true;
        }

        private void ExecuteSelectAll(EntityDesignerDiagram diagram)
        {
            // Select all shapes on the diagram
            if (diagram.ActiveDiagramView != null)
            {
                var selection = diagram.ActiveDiagramView.Selection;
                selection.Clear();
                foreach (var shape in diagram.NestedChildShapes)
                {
                    if (shape is Microsoft.VisualStudio.Modeling.Diagrams.NodeShape)
                    {
                        selection.Add(new Microsoft.VisualStudio.Modeling.Diagrams.DiagramItem(shape));
                    }
                }
            }
        }

        private void ExecuteShowMappingDetails()
        {
            // Execute the View Mapping Details command via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.ShowMappingDesigner);
        }

        private void ExecuteShowModelBrowser()
        {
            // Execute the View Model Browser command via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.ShowEdmExplorer);
        }

        private void ExecuteUpdateModelFromDatabase(EntityDesignerDiagram diagram)
        {
            // Execute the Update Model from Database wizard via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.RefreshFromDatabase);
        }

        private void ExecuteGenerateDatabaseFromModel(EntityDesignerDiagram diagram)
        {
            // Execute the Generate Database from Model wizard via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.GenerateDatabaseScriptFromModel);
        }

        private void ExecuteAddCodeGenerationItem(EntityDesignerDiagram diagram)
        {
            // Execute the Add Code Generation Item wizard via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.AddNewTemplate);
        }

        private void ExecuteValidate(EntityDesignerDiagram diagram)
        {
            // Execute the Validate command via VS command
            ExecuteVsCommand(MicrosoftDataEntityDesignCommands.Validate);
        }

        private void ExecuteVsCommand(System.ComponentModel.Design.CommandID commandId)
        {
            IServiceProvider sp = PackageManager.Package;
            if (sp != null)
            {
                var menuCommandService = sp.GetService(typeof(System.ComponentModel.Design.IMenuCommandService)) as System.ComponentModel.Design.IMenuCommandService;
                menuCommandService?.GlobalInvoke(commandId);
            }
        }

        private void ExecuteOpenXmlEditor(EntityDesignerDiagram diagram)
        {
            var artifact = diagram.GetModel()?.EditingContext?.GetEFArtifactService()?.Artifact;
            if (artifact == null)
            {
                return;
            }

            IServiceProvider sp = PackageManager.Package;
            if (sp != null)
            {
                Microsoft.VisualStudio.Shell.VsShellUtilities.OpenDocumentWithSpecificEditor(
                    sp,
                    artifact.Uri.LocalPath,
                    CommonPackageConstants.xmlEditorGuid,
                    VSConstants.LOGVIEWID_Primary,
                    out _,
                    out _,
                    out IVsWindowFrame frame);

                frame?.Show();
            }
        }

        private PointD GetCenterPoint()
        {
            // Return a reasonable default position - center of the visible viewport
            // For simplicity, use a fixed point that will be visible
            return new PointD(100, 100);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (_contextMenu != null)
            {
                _contextMenu.ActionExecuted -= OnMenuActionExecuted;
                _contextMenu.Dispose();
                _contextMenu = null;
            }
        }

        #endregion
    }
}
