﻿using FlatRedBall.Glue.CodeGeneration;
using FlatRedBall.Glue.Controls;
using FlatRedBall.Glue.Elements;
using FlatRedBall.Glue.Events;
using FlatRedBall.Glue.FormHelpers;
using FlatRedBall.Glue.Plugins;
using FlatRedBall.Glue.Plugins.ExportedImplementations;
using FlatRedBall.Glue.Plugins.Interfaces;
using FlatRedBall.Glue.Reflection;
using FlatRedBall.Glue.SaveClasses;
using FlatRedBall.Math.Collision;
using OfficialPlugins.CollisionPlugin.CodeGenerators;
using OfficialPlugins.CollisionPlugin.Controllers;
using OfficialPlugins.CollisionPlugin.Managers;
using OfficialPlugins.CollisionPlugin.ViewModels;
using OfficialPlugins.CollisionPlugin.Views;
using OfficialPlugins.CollisionPlugin.Errors;
using OfficialPlugins.CollisionPlugin.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfficialPlugins.CollisionPlugin
{
    [Export(typeof(PluginBase))]
    public class MainCollisionPlugin : PluginBase
    {
        #region Fields/Properties

        CollisionRelationshipView relationshipControl;
        PluginTab relationshipPluginTab;

        CollidableNamedObjectRelationshipDisplay collidableDisplay;
        CollidableNamedObjectRelationshipViewModel collidableViewModel;
        PluginTab collidableTab;

        public override string FriendlyName => "Collision Plugin";

        // 1.0
        //  - Initial release
        // 1.1
        //  - CollisionRelationships now have their Name set
        // 1.2
        //  - Added ability to mark a collision as inactive
        public override Version Version => new Version(1, 2);

        #endregion

        public override bool ShutDown(PluginShutDownReason shutDownReason)
        {
            UnregisterAllCodeGenerators();

            AvailableAssetTypes.Self.RemoveAssetType(
                AssetTypeInfoManager.Self.CollisionRelationshipAti);
            return true;
        }

        public override void StartUp()
        {
            collidableViewModel = new CollidableNamedObjectRelationshipViewModel();
            CollidableNamedObjectController.RegisterViewModel(collidableViewModel);

            RegisterCodeGenerator(new CollisionCodeGenerator());
            RegisterCodeGenerator(new StackableCodeGenerator());

            AvailableAssetTypes.Self.AddAssetType(
                AssetTypeInfoManager.Self.CollisionRelationshipAti);


            AssignEvents();

            AddErrorReporter(new CollisionErrorReporter());
        }

        private void AssignEvents()
        {
            this.ReactToLoadedGluxEarly += HandleGluxLoad;

            this.ReactToItemsSelected += HandleItemsSelected;

            this.AddEventsForObject += HandleAddEventsForObject;

            this.GetEventSignatureArgs += GetEventSignatureAndArgs;

            this.ReactToObjectRemoved += HandleObjectRemoved;

            this.ReactToChangedPropertyHandler += CollisionRelationshipViewModelController.HandleGlueObjectPropertyChanged;

            this.AdjustDisplayedEntity += StackableEntityManager.Self.HandleDisplayedEntity;

            this.ReactToCreateCollisionRelationshipsBetween += async (NamedObjectSave first, NamedObjectSave second) =>
            {
                if(first == null)
                {
                    throw new ArgumentNullException(nameof(first));
                }
                if(second == null)
                {
                    throw new ArgumentNullException(nameof(second));
                }


                var firstParent = first.GetContainer();
                var secondParent = second.GetContainer();

                var shouldCreate = true;

                if(firstParent == secondParent && firstParent != null)
                {
                    if(firstParent is ScreenSave parentScreen)
                    {
                        var gameScreen = ObjectFinder.Self.GetScreenSave("GameScreen");

                        var baseScreens = ObjectFinder.Self.GetAllBaseElementsRecursively(parentScreen);

                        var inheritsFromGameScreen = baseScreens.Contains(gameScreen);

                        if(inheritsFromGameScreen)
                        {
                            var message = "You are creating a collision relationship in the screen " +
                                $"{parentScreen}. Usually CollisionRelationships are created in the GameScreen. Are you sure you want to continue?";
                            var result = MessageBox.Show(message, "Create collision relationship?", MessageBoxButtons.YesNo);

                            if(result == DialogResult.No)
                            {
                                shouldCreate = false;
                            }
                        }
                    }
                }

                if(shouldCreate)
                {
                    var nos = await CollidableNamedObjectController.CreateCollisionRelationshipBetweenObjects(first.InstanceName, second.InstanceName, first.GetContainer());

                    return nos;
                }
                else
                {
                    return null;
                }
            };

            this.ReactToElementRenamed += HandleElementRenamed;
        }


        private void HandleElementRenamed(IElement element, string oldName)
        {
            // loop through all screens. If any screen has a collision relationship that is not DefinedByBase, then look at its objects.
            // If those objects are of type that reference the newly named element, then we generate that screen

            var screensToRegenerate = new HashSet<ScreenSave>();
            var elementNameWithDot = element.Name.Replace("\\", ".");
            foreach(var screen in GlueState.Self.CurrentGlueProject.Screens)
            {
                foreach(var nos in screen.AllNamedObjects)
                {
                    if(nos.DefinedByBase == false && nos.IsCollisionRelationship())
                    {
                        var firstType = AssetTypeInfoManager.GetFirstGenericType(nos, out bool isFirstList);
                        var secondType = AssetTypeInfoManager.GetSecondGenericType(nos, out bool isSecondList);

                        if(firstType == elementNameWithDot || secondType == elementNameWithDot)
                        {
                            screensToRegenerate.Add(screen);
                            // don't break - we still need to fix all source class types:
                            //break;
                            CollisionRelationshipViewModelController.TryFixSourceClassType(nos);
                        }
                    }
                }
            }

            foreach(var screen in screensToRegenerate)
            {
                GlueCommands.Self.GenerateCodeCommands.GenerateElementCode(screen, generateDerivedElements:false);
            }
        }

        private void HandleObjectRemoved(IElement element, NamedObjectSave namedObject)
        {
            if(namedObject.IsCollisionRelationship())
            {
                GlueCommands.Self.RefreshCommands.RefreshErrors();
            }
            else if(element.AllNamedObjects.Any(item => item.IsCollisionRelationship() &&
                (item.GetFirstCollidableObjectName() == namedObject.InstanceName) ||
                (item.GetSecondCollidableObjectName() == namedObject.InstanceName)))
            {
                GlueCommands.Self.RefreshCommands.RefreshErrors();
            }
        }

        private void HandleGluxLoad()
        {
            foreach(var screen in GlueState.Self.CurrentGlueProject.Screens)
            {
                foreach(var nos in screen.AllNamedObjects)
                {
                    CollisionRelationshipViewModelController.TryFixSourceClassType(nos);
                }
            }

            // entities probably won't have collisisons but...what if they do? Might as well be prepared:
            foreach (var entity in GlueState.Self.CurrentGlueProject.Entities)
            {
                foreach (var nos in entity.AllNamedObjects)
                {
                    CollisionRelationshipViewModelController.TryFixSourceClassType(nos);
                }
            }

        }

        private void GetEventSignatureAndArgs(NamedObjectSave namedObjectSave, EventResponseSave eventResponseSave, out string type, out string signatureArgs)
        {
            if(namedObjectSave == null)
            {
                throw new ArgumentNullException(nameof(namedObjectSave));
            }

            if (namedObjectSave.GetAssetTypeInfo() == AssetTypeInfoManager.Self.CollisionRelationshipAti &&
                eventResponseSave.SourceObjectEvent == "CollisionOccurred")
            {
                bool firstThrowaway;
                bool secondThrowaway;

                var firstType = AssetTypeInfoManager.GetFirstGenericType(namedObjectSave, out firstThrowaway);
                var secondType = AssetTypeInfoManager.GetSecondGenericType(namedObjectSave, out secondThrowaway);
                
                string GetUnqualified(string type, string fallback)
                {
                    if(type?.Contains(".") == true)
                    {
                        type = type.Substring(type.LastIndexOf('.') + 1);
                        type = Char.ToLowerInvariant(type[0]) + type.Substring(1);
                    }
                    else
                    {
                        type = fallback;
                    }
                    return type;
                }

                if(string.IsNullOrEmpty(secondType))
                {
                    // August 3, 2021
                    // This used to be
                    // an invalid case,
                    // but now Glue supports
                    // "Always" collisions which
                    // don't specify a 2nd type
                    type = $"System.Action<{firstType}>";
                    signatureArgs = $"{firstType} {GetUnqualified(firstType, "first")}";

                }
                else
                {
                    type = $"System.Action<{firstType}, {secondType}>";

                    var firstUnqualified = GetUnqualified(firstType, "first");
                    var secondUnqualified = GetUnqualified(secondType, "second");
                    if(firstUnqualified == secondUnqualified)
                    {
                        secondUnqualified += "2";
                    }
                    signatureArgs = $"{firstType} {firstUnqualified}, {secondType} {secondUnqualified}";
                }
            }
            else
            {
                type = null;
                signatureArgs = null;
            }
        }

        private void HandleAddEventsForObject(NamedObjectSave namedObject, List<ExposableEvent> listToAddTo)
        {
            if(namedObject.GetAssetTypeInfo() == AssetTypeInfoManager.Self.CollisionRelationshipAti)
            {
                var newEvent = new ExposableEvent("CollisionOccurred");
                listToAddTo.Add(newEvent);
            }
        }

        private void HandleItemsSelected(List<ITreeNode> list)
        {
            var selectedNos = GlueState.Self.CurrentNamedObjectSave;

            var element = GlueState.Self.CurrentElement;

            if (selectedNos != null)
            {
                CollisionRelationshipViewModelController.TryFixSourceClassType(selectedNos);
            }

            TryHandleSelectedCollisionRelationship(selectedNos);

            TryHandleSelectedCollidable(element, selectedNos);

        }


        private void TryHandleSelectedCollisionRelationship(NamedObjectSave selectedNos)
        {
            var shouldShowControl = false;
            if (selectedNos?.IsCollisionRelationship() == true)
            {
                shouldShowControl = true;
            }

            if (shouldShowControl)
            {
                if (relationshipControl == null)
                {
                    relationshipControl = new CollisionRelationshipView();
                    relationshipPluginTab = this.CreateTab(relationshipControl, "Collision");
                    

                }

                RefreshViewModelTo(selectedNos);

                relationshipPluginTab.Show();
            }
            else
            {
                relationshipPluginTab?.Hide();
            }
        }

        private void TryHandleSelectedCollidable(IElement element, NamedObjectSave selectedNos)
        {
            var shouldShowControl = selectedNos != null &&
                CollisionRelationshipViewModelController
                .GetIfCanBeReferencedByRelationship(selectedNos);

            if(shouldShowControl)
            {
                RefreshCollidableViewModelTo(element, selectedNos);

                if (collidableDisplay == null)
                {
                    collidableDisplay = new CollidableNamedObjectRelationshipDisplay();
                    collidableTab = this.CreateTab(collidableDisplay, "Collision");

                    collidableDisplay.DataContext = collidableViewModel;
                }
                collidableTab.Show();

                // not sure why this is required:
                collidableDisplay.DataContext = null;
                collidableDisplay.DataContext = collidableViewModel;
            }
            else
            {
                collidableTab?.Hide();
            }
        }

        private void RefreshViewModelTo(NamedObjectSave selectedNos)
        {
            // show UId
            // Vic says - not sure why but we have to remove and re-add the view model and
            // the view seems to show up properly. If we don't do this, parts don't show up correctly
            // (the parts that check if the view is a platformer). Vic could investigate this, but calling
            // this function seems to do the trick. Maybe return here if some other problem is found in the 
            // future, but for now leave it at this.
            if (relationshipControl != null)
            {
                relationshipControl.DataContext = null;
            }

            CollisionRelationshipViewModelController.RefreshViewModel(selectedNos);

            if (relationshipControl != null)
            {
                relationshipControl.DataContext = CollisionRelationshipViewModelController.ViewModel;
            }
        }

        private void RefreshCollidableViewModelTo(IElement element, NamedObjectSave selectedNos)
        {
            CollidableNamedObjectController.RefreshViewModelTo(element, selectedNos, collidableViewModel);
        }

        public bool FixNamedObjectCollisionType(NamedObjectSave selectedNos)
        {
            return CollisionRelationshipViewModelController.TryFixSourceClassType(selectedNos);
        }
    }
}
