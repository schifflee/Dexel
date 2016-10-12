using System;
using System.Linq;
using System.Windows;
using FlowDesignModel;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign
{

    public static class Interactions
    {
        public enum DragMode
        {
            Single,
            Multiple
        }


        public static void AddNewIOCell(Point pos)
        {
            var softwareCell = SoftwareCellsManager.CreateNew();
            pos.X -= 100;
            pos.Y -= 20;
            softwareCell.Position = new Point(pos.X, pos.Y);
            softwareCell.InputStreams.Add(DataStreamManager.CreateNewDefinition("input"));
            softwareCell.OutputStreams.Add(DataStreamManager.CreateNewDefinition("output"));

            MainModel.Get().SoftwareCells.Add(softwareCell);
            ViewRedraw();
        }


        public static void ViewRedraw()
        {
            MainViewModel.Instance().LoadFromModel(MainModel.Get());
        }


        public static void ChangeConnectionDestination(DataStream dataStream, SoftwareCell newdestination,
            MainModel mainModel)
        {
            //MainModelManager.RemoveConnection(dataStream, mainModel);
            DeConnect(dataStream, mainModel);
            MainModelManager.Connect(dataStream.Sources.First().ID, newdestination.ID, dataStream.DataNames, mainModel,
                dataStream.ActionName);

            ViewRedraw();
        }


        public static void AddNewInput(Guid softwareCellID, string datanames, MainModel mainModel,
            string actionName = "")
        {
            //SoftwareCellsManager.GetAll(softwareCellID, mainModel).ToList()
            //    .ForEach(softwareCell =>
            //    {
            //        var dataStream = DataStreamManager.CreateNewDefinition(datanames, actionName);
            //        softwareCell.InputStreams.Add(dataStream);
            //    });
        }


        public static void AddNewOutput(SoftwareCell softwareCell, string datanames)
        {
            //var definition = DataStreamManager.CreateNewDefinition(datanames);
            //softwareCell.OutputStreams.Add(definition);
            ViewRedraw();
        }

        internal static void AddNewInput(SoftwareCell softwareCell, string datanames)
        {
            //var definition = DataStreamManager.CreateNewDefinition(datanames);
            //softwareCell.InputStreams.Add(definition);
            ViewRedraw();
        }

        public static void MoveSoftwareCell(SoftwareCell model, double horizontalChange, double verticalChange)
        {
            var pos = model.Position;
            pos.X += horizontalChange;
            pos.Y += verticalChange;
            model.Position = pos;
        }


        public static void DeConnect(DataStream dataStream, MainModel mainModel)
        {
            dataStream.Sources.ForEach(softwareCell =>
                DataStreamManager.DeConnect(softwareCell.OutputStreams, dataStream));
            dataStream.Destinations.ForEach(softwareCell =>
                DataStreamManager.DeConnect(softwareCell.InputStreams, dataStream));

            MainModelManager.RemoveConnection(dataStream, mainModel);
            ViewRedraw();
        }


        public static void ConnectTwoDangelingConnections(DataStreamDefinition defintion, SoftwareCell source,
            SoftwareCell destination, MainModel mainModel)
        {
            DataStreamManager.FindExistingDefinition(defintion, source.OutputStreams,
                def => def.Connected = true);

            DataStreamManager.FindExistingDefinition(defintion, destination.InputStreams,
                def => def.Connected = true);


            MainModelManager.AddNewConnection(defintion, source, destination, mainModel);

            ViewRedraw();
        }


        public static void ConnectDangelingConnectionAndSoftwareCell(DataStreamDefinition defintion, SoftwareCell source,
            SoftwareCell destination, MainModel mainModel)
        {
            DataStreamManager.FindExistingDefinition(defintion, source.OutputStreams,
                def => def.Connected = true,
                () => source.OutputStreams.Add(DataStreamManager.CreateNewDefinition(defintion, connected:true))
                );

            DataStreamManager.FindExistingDefinition(defintion, destination.InputStreams,
                def => def.Connected = true,
                () => destination.InputStreams.Add(DataStreamManager.CreateNewDefinition(defintion, connected:true))
                );

            MainModelManager.AddNewConnection(defintion, source, destination, mainModel);

            ViewRedraw();
        }


        public static void DeleteDatastreamDefiniton(DataStreamDefinition dataStreamDefinition, SoftwareCell softwareCell)
        {
            softwareCell.InputStreams.RemoveAll(x => x == dataStreamDefinition);
            softwareCell.OutputStreams.RemoveAll(x => x == dataStreamDefinition);
            ViewRedraw();
        }

        public static void ConsolePrintGeneratedCode(MainModel mainModel)
        {
            //MyGenerator gen = new MyGenerator();

        }
    }

}