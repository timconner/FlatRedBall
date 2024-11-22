﻿using System.Collections.Generic;
using FlatRedBall.Glue.Elements;
using FlatRedBall.Glue.SaveClasses;
using FlatRedBall.Glue.Plugins.ExportedInterfaces;
using System.Windows.Forms;
using FlatRedBall.Glue.VSHelpers.Projects;
using FlatRedBall.Glue.Events;
using FlatRedBall.Glue.Data;
using FlatRedBall.Glue.Managers;
using Glue;
using FlatRedBall.IO;
using FlatRedBall.Glue.Errors;
using System.Linq;
using FlatRedBall.Glue.IO;
using GlueFormsCore.Plugins.EmbeddedPlugins.ExplorerTabPlugin;
using FlatRedBall.Glue.Controls;
using FlatRedBall.Glue.FormHelpers;
using FlatRedBall.Glue.Navigation;
using FlatRedBall.Glue.Tiled;
using GlueFormsCore.ViewModels;
using Microsoft.Build.Evaluation;
using Mono.Cecil;


namespace FlatRedBall.Glue.Plugins.ExportedImplementations
{
    #region GlueStateSnapshot

    public class GlueStateSnapshot
    {
        public ITreeNode CurrentTreeNode
        {
            get => CurrentTreeNodes.FirstOrDefault();
            set
            {
                if(value == null)
                {
                    CurrentTreeNodes.Clear();
                }
                else if(CurrentTreeNodes.Count != 1 || CurrentTreeNodes[0] != value)
                {
                    CurrentTreeNodes.Clear();

                    CurrentTreeNodes.Add(value);
                }
            }
        }
        public List<ITreeNode> CurrentTreeNodes = new List<ITreeNode>();

        public GlueElement CurrentElement;
        public EntitySave CurrentEntitySave;
        public ScreenSave CurrentScreenSave;
        public ReferencedFileSave CurrentReferencedFileSave;
        public NamedObjectSave CurrentNamedObjectSave
        {
            get => CurrentNamedObjectSaves.FirstOrDefault();
            set
            {
                if(value == null)
                {
                    CurrentNamedObjectSaves.Clear();
                }
                else if(CurrentNamedObjectSaves.Count != 1 || CurrentNamedObjectSaves[0] != value)
                {
                    CurrentNamedObjectSaves.Clear();

                    CurrentNamedObjectSaves.Add(value);
                }
            }
        }
        public List<NamedObjectSave> CurrentNamedObjectSaves = new List<NamedObjectSave>();
        public StateSave CurrentStateSave;
        public StateSaveCategory CurrentStateSaveCategory;
        public CustomVariable CurrentCustomVariable;
        public EventResponseSave CurrentEventResponseSave;
        public int? SelectedSubIndex;

    }

    #endregion

    public class GlueState : IGlueState
    {
        #region Current Selection Properties

        public ITreeNode CurrentTreeNode
        {
            get => snapshot.CurrentTreeNode;
            set
            {
                UpdateToSetTreeNode(value, recordState:true);
            }
        }

        public IReadOnlyList<ITreeNode> CurrentTreeNodes
        {
            get => snapshot.CurrentTreeNodes;
            set
            {
                UpdateToSetTreeNode(value, recordState: true);
            }
        }

        public GlueElement CurrentElement
        {
            get => snapshot.CurrentElement;
            set
            {
                var treeNode = GlueState.Self.Find.TreeNodeByTag(value);

                CurrentTreeNode = treeNode;
            }

        }

        public EntitySave CurrentEntitySave
        {
            get => snapshot.CurrentEntitySave;
            set => CurrentElement = value; 
        }

        public ScreenSave CurrentScreenSave
        {
            get => snapshot.CurrentScreenSave;
            set
            {
                CurrentTreeNode = GlueState.Self.Find.TreeNodeByTag(value);
            }
        }

        public ReferencedFileSave CurrentReferencedFileSave
        {
            get => snapshot.CurrentReferencedFileSave;
            set
            {
                CurrentTreeNode = GlueState.Self.Find.TreeNodeByTag(value);
            }
        }

        public NamedObjectSave CurrentNamedObjectSave
        {
            get => snapshot.CurrentNamedObjectSave;
            set
            {
                if (value == null)
                {
                    CurrentTreeNode = null;
                }
                else
                {
                    CurrentTreeNode =  GlueState.Self.Find.TreeNodeByTag(value);
                }
            }
        }

        public IReadOnlyList<NamedObjectSave> CurrentNamedObjectSaves
        {
            get => snapshot.CurrentNamedObjectSaves;
            set
            {
                if( value == null || value.Count == 0)
                {
                    CurrentTreeNode = null;
                }
                else
                {
                    List<ITreeNode> treeNodes = value.Select(item =>GlueState.Self.Find.TreeNodeByTag(item)).ToList();
                    CurrentTreeNodes = treeNodes;
                }
            }
        }

        public StateSave CurrentStateSave
        {
            get => snapshot.CurrentStateSave;
            set
            {
                var treeNode = GlueState.Self.Find.TreeNodeByTag(value);
                if (treeNode != null)
                {
                    CurrentTreeNode = treeNode;
                }
            }
        }

        public StateSaveCategory CurrentStateSaveCategory
        {
            get => snapshot.CurrentStateSaveCategory;
            set
            {
                var treeNode = GlueState.Self.Find.TreeNodeByTag(value);
                if(treeNode != null)
                {
                    CurrentTreeNode = treeNode;
                }
            }
        }

        public CustomVariable CurrentCustomVariable
        {
            get => snapshot.CurrentCustomVariable;

            set
            {
                CurrentTreeNode = GlueState.Self.Find.TreeNodeByTag(value);

            }

        }

        public EventResponseSave CurrentEventResponseSave
        {
            get => snapshot.CurrentEventResponseSave;
            set
            {
                CurrentTreeNode = GlueState.Self.Find.TreeNodeByTag(value);
            }
        }

        public string[] CurrentFocusedTabs
        {
            get
            {
                string GetFocusFor(TabContainerViewModel tabContainerVm)
                {
                    foreach(var tabPage in tabContainerVm.Tabs)
                    {
                        if(tabPage.IsSelected)
                        {
                            return tabPage.Title;
                        }
                    }
                    return null;
                }

                List<string> listToReturn = new List<string>();

                GlueCommands.Self.DoOnUiThread(() =>
                {
                    void AddIfNotNull(string value) 
                    {
                        if(value != null)
                        {
                            listToReturn.Add(value);
                        }
                    };

                    AddIfNotNull(GetFocusFor(PluginManager.TabControlViewModel.TopTabItems));
                    AddIfNotNull(GetFocusFor(PluginManager.TabControlViewModel.BottomTabItems));
                    AddIfNotNull(GetFocusFor(PluginManager.TabControlViewModel.LeftTabItems));
                    AddIfNotNull(GetFocusFor(PluginManager.TabControlViewModel.RightTabItems));
                    AddIfNotNull(GetFocusFor(PluginManager.TabControlViewModel.CenterTabItems));

                });

                return listToReturn.ToArray();
            }
        }

        public int? SelectedSubIndex
        {
            get => snapshot.SelectedSubIndex;
            set
            {
                if(snapshot.SelectedSubIndex != value)
                {
                    snapshot.SelectedSubIndex = value;
                    PluginManager.ReactToSelectedSubIndexChanged(snapshot.SelectedSubIndex);
                }
            }
        }

        public List<ProjectBase> SyncedProjects { get; private set; } = new List<ProjectBase>();
        IEnumerable<ProjectBase> IGlueState.SyncedProjects
        {
            get => SyncedProjects;
        }

        #endregion

        #region Project Properties

        public string ContentDirectory
        {
            get
            {
                return CurrentMainProject?.GetAbsoluteContentFolder();
            }
        }

        public FilePath ContentDirectoryPath => ContentDirectory != null
            ? new FilePath(ContentDirectory) : null;

        /// <summary>
        /// Returns the current Glue code project file name
        /// </summary>
        public FilePath CurrentCodeProjectFileName
        {
            get; private set;
        }

        /// <summary>
        /// Returns the directory of the .gluj, which is the same directory as the .csproj
        /// </summary>
        public string CurrentGlueProjectDirectory
        {
            get
            {
                return CurrentCodeProjectFileName?.GetDirectoryContainingThis().FullPath;
            }
        }


        VisualStudioProject _currentMainProject;
        public VisualStudioProject CurrentMainProject
        {
            get => _currentMainProject;
            set
            {
                _currentMainProject = value;
                // This preserves the old file name even after we exit in case there are extra tasks running
                if (value != null)
                {
                    CurrentCodeProjectFileName = value.FullFileName;
                }
            }
        }

        public VisualStudioProject CurrentMainContentProject { get { return ProjectManager.ContentProject; } }

        public FilePath CurrentSlnFileName => SlnFileForProject(CurrentMainProject);

        public FilePath SlnFileForProject(VisualStudioProject vsproject)
        {
            if (vsproject == null)
            {
                return null;
            }
            else
            {
                var csprojLocation = vsproject.FullFileName;
                return VSHelpers.ProjectSyncer.LocateSolution(csprojLocation);
            }
        }

        public FilePath GlueExeDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
            }
        }

        public string ProjectNamespace
        {
            get
            {
                return ProjectManager.ProjectNamespace;
            }

        }

        /// <summary>
        /// The file name of the GLUX
        /// </summary>
        public FilePath GlueProjectFileName
        {
            get
            {
                if (CurrentMainProject == null)
                {
                    return null;
                }
                else
                {
                    if (CurrentGlueProject?.FileVersion >= (int)GlueProjectSave.GluxVersions.GlueSavedToJson)
                    {
                        return CurrentMainProject.FullFileName.RemoveExtension() + ".gluj";
                    }
                    else
                    {
                        return CurrentMainProject.FullFileName.RemoveExtension() + ".glux";
                    }
                }
            }

        }

        public string ProjectSpecificSettingsFolder
        {
            get
            {
                var projectDirectory = GlueProjectFileName.GetDirectoryContainingThis();

                return projectDirectory.FullPath + "GlueSettings/";
            }
        }

        public FilePath ProjectSpecificSettingsPath => new FilePath(ProjectSpecificSettingsFolder);


        public GlueProjectSave CurrentGlueProject => ObjectFinder.Self.GlueProject; 

        public PluginSettings CurrentPluginSettings => ProjectManager.PluginSettings;

        public bool IsProjectLoaded(VisualStudioProject project)
        {
            return CurrentMainProject == project || SyncedProjects.Contains(project);
        }

        /// <summary>
        /// The global glue settings for the current user, not tied to a particular project.
        /// </summary>
        public GlueSettingsSave GlueSettingsSave
        {
            get => ProjectManager.GlueSettingsSave;
            set => ProjectManager.GlueSettingsSave = value;
        }

        public int? EngineDllSyntaxVersion
        {
            get
            {
                // for now we'll use the main project, but eventually we may want to include synced projects too:
                var project = GlueState.Self.CurrentMainProject;
                var referenceItems = project.EvaluatedItems.Where(item =>
                {
                    return item.ItemType == "PackageReference" && item.EvaluatedInclude.StartsWith("FlatRedBall");
                });

                foreach (var item in referenceItems)
                {
                    var path = GetFilePathFor(item);

                    if (path != null)
                    {
                        var module = ModuleDefinition.ReadModule(path.FullPath);
                        var frbServicesType = module.Types.FirstOrDefault(item => item.FullName == "FlatRedBall.FlatRedBallServices");
                        foreach (var attribute in frbServicesType.CustomAttributes)
                        {
                            if (attribute.AttributeType.Name == "SyntaxVersionAttribute" && attribute.Fields.Count > 0)
                            {
                                var version = int.Parse(attribute.Fields[0].Argument.Value.ToString());
                                return version;
                            }
                        }
                    }

                }
                return null;
            }
        }

        private static FilePath GetFilePathFor(ProjectItem item)
        {
            string packageName = item.EvaluatedInclude;
            string packageVersion = item.Metadata.FirstOrDefault(item => item.Name == "Version")?.EvaluatedValue;

            var userName = System.Environment.UserName;


            if (userName != null)
            {
                string[] searchPaths = {
                    @"C:\Program Files\dotnet\packs",
                    $@"C:\Users\{userName}\.nuget\packages"
                };

                foreach (string path in searchPaths)
                {
                    string fullPath = System.IO.Path.Combine(path, $"{packageName}", $"{packageVersion}", $"{packageName}.{packageVersion}.nupkg");
                    if (System.IO.File.Exists(fullPath))
                    {
                        var directory = FileManager.GetDirectory(fullPath);
                        // find a .dll with matching file
                        var allFiles = FlatRedBall.IO.FileManager.GetAllFilesInDirectory(directory, "dll");
                        foreach (var file in allFiles)
                        {
                            if (file.Contains($"{packageName}.dll"))
                            {
                                return file;
                            }
                        }
                    }
                }
            }
            return null;
        }

        #endregion

        #region Sub-containers and Self

        static GlueState mSelf;
        public static GlueState Self
        {
            get
            {
                if (mSelf == null)
                {
                    mSelf = new GlueState();
                }
                return mSelf;
            }
        }
        public IFindManager Find
        {
            get;
            set;
        }
        public States.Clipboard Clipboard
        {
            get;
            private set;
        }

        public TiledCache TiledCache { get; private set; } = new TiledCache();

        #endregion

        #region Properties

        ITreeNode draggedTreeNode;
        public ITreeNode DraggedTreeNode 
        {
            get => draggedTreeNode;
            set
            {
                if(value != draggedTreeNode)
                {
                    if(draggedTreeNode != null)
                    {
                        PluginManager.ReactToGrabbedTreeNodeChanged(draggedTreeNode, TreeNodeAction.Released);
                    }
                    draggedTreeNode = value;
                    if(draggedTreeNode == null)
                    {
                        //GlueCommands.Self.PrintOutput("Released node");
                    }
                    else
                    {
                        if (value != null)
                        {
                            PluginManager.ReactToGrabbedTreeNodeChanged(draggedTreeNode, TreeNodeAction.Grabbed);
                        }

                    }
                }
            }
        }

        GlueStateSnapshot snapshot = new GlueStateSnapshot();

        public ErrorListViewModel ErrorList { get; private set; } = new ErrorListViewModel();

        public static object ErrorListSyncLock = new object();

        public bool IsReferencingFrbSource
        {
            get
            {
                if(CurrentMainProject == null)
                {
                    return false;
                }
                else
                {
                    if(CurrentMainProject is MonoGameDesktopGlBaseProject)
                    {
                        // todo - handle different types of projects
                        string projectReferenceName;
                        if(CurrentMainProject.DotNetVersion.Major >= 6)
                        {
                            projectReferenceName = "FlatRedBallDesktopGLNet6";
                        }
                        else
                        {
                            projectReferenceName = "FlatRedBallDesktopGL";
                        }
                        return CurrentMainProject.HasProjectReference(projectReferenceName);

                    }
                    return false;
                }
            }
        }

        #endregion

        public GlueState()
        {
            // find will be assigned by plugins
            Clipboard = new States.Clipboard();

            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(
                ErrorList.Errors, ErrorListSyncLock);
        }

        /// <summary>
        /// Returns all loaded IDE projects, including the main project and all synced projects.
        /// </summary>
        /// <returns></returns>
        public List<ProjectBase> GetProjects()
        {
            var list = new List<ProjectBase>();

            list.Add(GlueState.Self.CurrentMainProject);

            list.AddRange(ProjectManager.SyncedProjects);

            return list;
        }

        public void SetCurrentTreeNode(ITreeNode treeNode, bool recordState) => UpdateToSetTreeNode(treeNode, recordState);

        private void UpdateToSetTreeNode(ITreeNode value, bool recordState)
        {
            if(value != null)
            {
                UpdateToSetTreeNode(new List<ITreeNode> { value }, recordState);
            }
            else
            {
                UpdateToSetTreeNode(new List<ITreeNode> (), recordState);
            }
        }

        private void UpdateToSetTreeNode(IReadOnlyList<ITreeNode> value, bool recordState)
        {
            var isSame = snapshot?.CurrentTreeNodes.Count == value.Count;
            if(isSame)
            {
                for(int i = 0; i < value.Count; i++)
                {
                    if (value[i] != snapshot.CurrentTreeNodes[i])
                    {
                        isSame = false;
                        break;
                    }
                }
            }

            // Push to the stack for history before taking a snapshot, so that the "old" one is pushed
            if (!isSame && snapshot?.CurrentTreeNode != null && recordState)
            {
                // todo - need to support multi select
                TreeNodeStackManager.Self.Push(snapshot.CurrentTreeNode);
            }

            // Snapshot should come first so everyone can update to the snapshot
            GlueState.Self.TakeSnapshot(value);

            // If we don't check for isSame, then selecting the same tree node will result in double-selects in the game.
            if(!isSame)
            {
                PluginManager.ReactToItemsSelected(value.ToList());
            }
        }


        public IEnumerable<ReferencedFileSave> GetAllReferencedFiles()
        {
            return ObjectFinder.Self.GetAllReferencedFiles();
        }

        void TakeSnapshot(IReadOnlyList<ITreeNode> selectedTreeNodes)
        {
            snapshot.CurrentTreeNodes = selectedTreeNodes.ToList();
            snapshot.CurrentElement = GetCurrentElementFromSelection();
            snapshot.CurrentEntitySave = GetCurrentEntitySaveFromSelection();
            snapshot.CurrentScreenSave = GetCurrentScreenSaveFromSelection();
            snapshot.CurrentReferencedFileSave = GetCurrentReferencedFileSaveFromSelection();
            snapshot.CurrentNamedObjectSaves = GetCurrentNamedObjectSavesFromSelection();
            snapshot.CurrentStateSave = GetCurrentStateSaveFromSelection();
            snapshot.CurrentStateSaveCategory = GetCurrentStateSaveCategoryFromSelection();
            snapshot.CurrentCustomVariable = GetCurrentCustomVariableFromSelection();
            snapshot.CurrentEventResponseSave = GetCurrentEventResponseSaveFromSelection();
            snapshot.SelectedSubIndex = null;

            GlueElement GetCurrentElementFromSelection()
            {
                return (GlueElement)GetCurrentEntitySaveFromSelection() ?? GetCurrentScreenSaveFromSelection();
            }
            EntitySave GetCurrentEntitySaveFromSelection()
            {
                var treeNode = selectedTreeNodes.FirstOrDefault();

                while (treeNode != null)
                {
                    if (treeNode.Tag is EntitySave entitySave)
                    {
                        return entitySave;
                    }
                    else
                    {
                        treeNode = treeNode.Parent;
                    }
                }

                return null;
            }
            ScreenSave GetCurrentScreenSaveFromSelection()
            {

                var treeNode = selectedTreeNodes.FirstOrDefault();

                while (treeNode != null)
                {
                    if (treeNode.Tag is ScreenSave screenSave)
                    {
                        return screenSave;
                    }
                    else
                    {
                        treeNode = treeNode.Parent;
                    }
                }

                return null;
            }
            ReferencedFileSave GetCurrentReferencedFileSaveFromSelection()
            {
                var treeNode = selectedTreeNodes.FirstOrDefault();

                if (treeNode != null && treeNode.Tag != null && treeNode.Tag is ReferencedFileSave rfs)
                {
                    return rfs;
                }
                else
                {
                    return null;
                }
            }
            List<NamedObjectSave> GetCurrentNamedObjectSavesFromSelection()
            {
                var treeNodes = selectedTreeNodes;

                var noses = treeNodes.Select(item => item.Tag).Where(item => item is NamedObjectSave).Cast<NamedObjectSave>().ToList();

                return noses;
            }
            StateSave GetCurrentStateSaveFromSelection()
            {
                var treeNode = selectedTreeNodes.FirstOrDefault();

                if (treeNode != null && treeNode.IsStateNode())
                {
                    return (StateSave)treeNode.Tag;
                }

                return null;
            }
            StateSaveCategory GetCurrentStateSaveCategoryFromSelection()
            {
                var treeNode = selectedTreeNodes.FirstOrDefault();

                if (treeNode != null)
                {
                    if (treeNode.IsStateCategoryNode())
                    {
                        return (StateSaveCategory)treeNode.Tag;
                    }
                    // if the current node is a state, maybe the parent is a category
                    else if (treeNode.Parent != null && treeNode.Parent.IsStateCategoryNode())
                    {
                        return (StateSaveCategory)treeNode.Parent.Tag;
                    }
                }

                return null;
            }
            CustomVariable GetCurrentCustomVariableFromSelection()
            {
                var treeNode = selectedTreeNodes.FirstOrDefault();

                if (treeNode == null)
                {
                    return null;
                }
                else if (treeNode.IsCustomVariable())
                {
                    return (CustomVariable)treeNode.Tag;
                }
                else
                {
                    return null;
                }
            }
            EventResponseSave GetCurrentEventResponseSaveFromSelection()
            {
                var treeNode = selectedTreeNodes.FirstOrDefault();

                if (treeNode == null)
                {
                    return null;
                }
                else if (treeNode.Tag != null && treeNode.Tag is EventResponseSave eventResponse)
                {
                    return eventResponse;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
