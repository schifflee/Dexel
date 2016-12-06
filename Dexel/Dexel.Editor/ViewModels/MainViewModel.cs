﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dexel.Editor.CustomControls;
using Dexel.Editor.DragAndDrop;
using Dexel.Editor.ViewModels.DataTypeEditor;
using Dexel.Editor.ViewModels.DrawingBoard;
using Dexel.Library;
using Dexel.Model;
using Dexel.Model.DataTypes;
using PropertyChanged;

namespace Dexel.Editor.ViewModels
{

    [ImplementPropertyChanged]
    public class MainViewModel : IDropable
    {
        private static MainViewModel _self;
        public bool LoadingModelFlag;


        public MainViewModel()
        {
            SoftwareCells = new ObservableCollection<IOCellViewModel>();
            SelectedSoftwareCells = new ObservableCollection<IOCellViewModel>();
            Connections = new ObservableCollection<ConnectionViewModel>();
            IntegrationBorders = new ObservableCollection<IOCellViewModel>();
            DataTypes = new ObservableCollection<DataTypeViewModel>();
            VisibileDataTypes = new ObservableCollection<DataTypeViewModel>();
            FontSizeCell = 12;
            VisibilityDatanames = Visibility.Visible;
            VisibilityBlockTextBox = Visibility.Hidden;
            SelectedSoftwareCells.CollectionChanged += (sender, args) => UpdateSelectionState();
        }


        public ObservableCollection<DataTypeViewModel> DataTypes { get; set; }
        public ObservableCollection<IOCellViewModel> IntegrationBorders { get; set; }
        public ObservableCollection<ConnectionViewModel> Connections { get; set; }
        public ObservableCollection<IOCellViewModel> SoftwareCells { get; set; }
        public ObservableCollection<IOCellViewModel> SelectedSoftwareCells { get; set; }
        public ObservableCollection<DataTypeViewModel> VisibileDataTypes { get; set; }
        public ConnectionViewModel TemporaryConnection { get; set; }
        public MainModel Model { get; set; }
        public int FontSizeCell { get; set; }
        public Visibility VisibilityDatanames { get; set; }
        public Visibility VisibilityBlockTextBox { get; set; }
        public int MissingDataTypes { get; set; }

        public static MainViewModel Instance() => _self ?? (_self = new MainViewModel());


        public void SetDataTypeFilter(string datanames)
        {
            if (datanames == null)
            {
                VisibileDataTypes.Clear();
                DataTypes.ForEach(vm => VisibileDataTypes.Add(vm));
                return;
            }

            var types = Interactions.GetFocusedDataTypes(datanames, Model);

            VisibileDataTypes.Clear();
            DataTypes.Where(x => types.Any(y => x.Model.Name == y)).ForEach(vm => VisibileDataTypes.Add(vm));
        }

        #region Modify Selection

        private void UpdateSelectionState()
        {
            SoftwareCells.ForEach(x => x.IsSelected = false);
            SelectedSoftwareCells.ForEach(x => x.IsSelected = true);
        }


        public void SetSelection(IOCellViewModel ioCellViewModel)
        {
            if (SelectedSoftwareCells.Contains(ioCellViewModel)) return;

            SelectedSoftwareCells.Clear();
            SelectedSoftwareCells.Add(ioCellViewModel);
        }


        public void SetSelectionCTRLMod(IOCellViewModel ioCellViewModel)
        {
            if (SelectedSoftwareCells.Contains(ioCellViewModel))
                SelectedSoftwareCells.Remove(ioCellViewModel);
            else
                SelectedSoftwareCells.Add(ioCellViewModel);
        }


        public void MoveSelectedIOCells(Vector dragDelta)
        {
            SelectedSoftwareCells.ForEach(sc => Interactions.MoveSoftwareCell(sc.Model, dragDelta.X, dragDelta.Y));
        }


        public void DuplicateSelectionAndSelectNew()
        {
            var duplicted = DuplicateSelection();
            Select(duplicted);
        }


        private void Select(List<SoftwareCell> duplicted)
        {
            SelectedSoftwareCells.Clear();
            SoftwareCells.Where(sc => duplicted.Contains(sc.Model)).ForEach(vm => SelectedSoftwareCells.Add(vm));
        }


        public SoftwareCell DuplicateIncludingChildrenAndIntegrated(SoftwareCell softwareCell)
        {
            var list = MainModelManager.GetChildrenAndIntegrated(softwareCell, new List<SoftwareCell>(), Model);
            var copiedlist = MainModelManager.Duplicate(list, Model);
            var first = copiedlist.First(x => x.OriginGuid == softwareCell.ID);
            return first.NewCell;
        }


        private List<SoftwareCell> DuplicateSelection()
        {
            var copiedList = MainModelManager.Duplicate(SelectedSoftwareCells.Select(vm => vm.Model).ToList(), Model);

            Reload();
            return copiedList.Select(x => x.NewCell).ToList();
        }


        public void ClearSelection()
        {
            SelectedSoftwareCells.Clear();
        }


        public void AddToSelection(IOCellViewModel ioCellViewModel)
        {
            SelectedSoftwareCells.Add(ioCellViewModel);
        }

        #endregion

        #region Drop

        public List<Type> AllowedDropTypes => new List<Type>
        {
            typeof(ConnectionViewModel)
        };


        public void Drop(object data)
        {
            data.TryCast<ConnectionViewModel>(
                connectionVM => Interactions.DeConnect(connectionVM.Model, Model));
        }

        #endregion

        #region Update Positions

        public void UpdateConnectionsPosition(Point inputPoint, Point outputPoint, IOCellViewModel ioCellViewModel)
        {
            var allOutputs = Connections.Where(conn => conn.Model.Sources.Any(x => x.Parent == ioCellViewModel.Model));
            var allInputs =
                Connections.Where(conn => conn.Model.Destinations.Any(x => x.Parent == ioCellViewModel.Model));

           

            allInputs.ToList().ForEach(connVM =>
            {
                var index = ioCellViewModel.Model.InputStreams.IndexOf(connVM.Model.Destinations.First());
                var count = ioCellViewModel.Model.InputStreams.Count;
                const int dsdHeight = 42;
                inputPoint.Y -= (count-1)* (dsdHeight /2);
                inputPoint.Y += index * dsdHeight +2;

                var inputVM = (ConnectionAdapterViewModel)ioCellViewModel.Inputs.First(ioVM => ioVM.Model == connVM.Model.Destinations.First());
                inputPoint.X -= inputVM.Width -5;
                connVM.End = inputPoint;
            });
            allOutputs.ToList().ForEach(connVM =>
            {
                var index = ioCellViewModel.Model.OutputStreams.IndexOf(connVM.Model.Sources.First());
                var count = ioCellViewModel.Model.OutputStreams.Count;
                const int dsdHeight = 42;
                outputPoint.Y -= (count - 1) * (dsdHeight / 2);
                outputPoint.Y += index * dsdHeight + 2;

                var outputVM = (ConnectionAdapterViewModel)ioCellViewModel.Outputs.First(ioVM => ioVM.Model == connVM.Model.Sources.First());
                outputPoint.X += outputVM.Width - 5;
                connVM.Start = outputPoint;
            });
        }


        public void UpdateIntegrationBorderPositions(ObservableCollection<IOCellViewModel> integrationsBorders)
        {
            integrationsBorders.ForEach(UpdateIntegrationBorderPosition);
        }


        public void UpdateIntegrationBorderPosition(IOCellViewModel iocellvm)
        {
            if (iocellvm.Integration.Count == 0)
                return;
            var tempIntegrations =
                iocellvm.Integration.OrderBy(cellvm1 => cellvm1.Model.Position.X + cellvm1.CellWidth);
            var min = tempIntegrations.First();
            var max = tempIntegrations.Last();

            tempIntegrations = iocellvm.Integration.OrderBy(cellvm1 => cellvm1.Model.Position.Y + cellvm1.CellHeight);
            var miny = tempIntegrations.First();
            iocellvm.IntegrationStartPosition = new Point(min.Model.Position.X - 60, miny.Model.Position.Y);
            iocellvm.IntegrationEndPosition = new Point(max.Model.Position.X + max.CellWidth + 60,
                miny.Model.Position.Y);
        }

        #endregion

        #region Load Model

        public void Reload()
        {
            if (Model != null)
                LoadFromModel(Model);
        }


        public void LoadFromModel(MainModel mainModel)
        {
            LoadingModelFlag = true;
            try
            {
                Model = mainModel;
                LoadSoftwareCells(mainModel.SoftwareCells);
                LoadConnection(mainModel.Connections);               
                LoadIntegrations();
                LoadDataTypes(mainModel.DataTypes);
            }
            finally
            {
                LoadingModelFlag = false;
            }
        }


        private void LoadDataTypes(List<DataType> dataTypes)
        {
            DataTypes.Clear();
            dataTypes.ForEach(dataType =>
            {
                var vm = new DataTypeViewModel();
                vm.Model = dataType;

                if ((dataType.DataTypes == null) || !dataType.DataTypes.Any())
                    vm.Definitions = "";
                else
                    vm.Definitions = dataType.DataTypes
                        .Select(x => string.IsNullOrEmpty(x.Name) ? x.Type : $"{x.Name}:{x.Type}")
                        .Aggregate((str, type) => str + "\n" + type);

                DataTypes.Add(vm);
            });
        }


        private void LoadIntegrations()
        {
            var newcollection = new ObservableCollection<IOCellViewModel>();
            SoftwareCells.Where(x => x.Model.Integration.Count != 0).ToList().ForEach(hasIntegration =>
            {
                var integratedVMs =
                    SoftwareCells.Where(otherVM => hasIntegration.Model.Integration.Contains(otherVM.Model));
                var list = new ObservableCollection<IOCellViewModel>();
                integratedVMs.ToList().ForEach(list.Add);
                hasIntegration.Integration = list;
                newcollection.Add(hasIntegration);
            });

            UpdateIntegrationBorderPositions(newcollection);
            IntegrationBorders = newcollection;
        }


        private void LoadSoftwareCells(List<SoftwareCell> softwareCellsToLoad)
        {
            RemoveDeletedSoftwareCells(softwareCellsToLoad);

            var lookup = SoftwareCells.ToLookup(x => x.Model.ID, x => x);
            softwareCellsToLoad.ForEach(model => FindIOCellViewModel(lookup, model,
                onFound: viewModel => IOCellViewModel.LoadFromModel(viewModel, model),
                onNotFound: () => AddNewSoftwareCell(model)));
        }


        private void AddNewSoftwareCell(SoftwareCell model)
        {
            var vm = new IOCellViewModel();
            vm.LoadFromModel(model);
            SoftwareCells.Add(vm);
        }


        private void FindIOCellViewModel(ILookup<Guid, IOCellViewModel> lookup, SoftwareCell model,
            Action<IOCellViewModel> onFound, Action onNotFound)
        {
            var found = lookup[model.ID].ToList();
            if (found.Any())
                onFound(found.First());
            else
                onNotFound();
        }


        private void FindConnectionViewModel(ILookup<Guid, ConnectionViewModel> lookup, DataStream model,
            Action<ConnectionViewModel> onFound, Action onNotFound)
        {
            var found = lookup[model.ID].ToList();
            if (found.Any())
                onFound(found.First());
            else
                onNotFound();
        }


        private void RemoveDeletedSoftwareCells(List<SoftwareCell> softwareCellsToLoad)
        {
            var todelte = SoftwareCells.Where(vm => softwareCellsToLoad.All(cell => cell.ID != vm.Model.ID)).ToList();
            todelte.ForEach(vm => SoftwareCells.Remove(vm));
        }


        private void LoadConnection(List<DataStream> datastreamsToLoad)
        {
            RemoveDeletedConnections(datastreamsToLoad);

            var lookup = Connections.ToLookup(x => x.Model.ID, x => x);
            datastreamsToLoad.ForEach(model => FindConnectionViewModel(lookup, model,
                onFound: viewModel => ConnectionViewModel.LoadFromModel(viewModel, model),
                onNotFound: () => AddNewConnection(model)));
        }


        private void AddNewConnection(DataStream model)
        {
            var vm = new ConnectionViewModel();
            vm.LoadFromModel(model);
            Connections.Add(vm);
        }


        private void RemoveDeletedConnections(List<DataStream> datastreamsToLoad)
        {
            var todelte = Connections.Where(vm => datastreamsToLoad.All(cell => cell.ID != vm.Model.ID)).ToList();
            todelte.ForEach(vm => Connections.Remove(vm));
        }

        #endregion
    }

}