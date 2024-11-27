using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure.StructuralSections;
using System.Reflection.Emit;
using Autodesk.Revit.DB.Architecture;
using System.Xml.Linq;
using Autodesk.Revit.DB.Lighting;
using System.Net;
using Autodesk.Revit.Creation;
using System.Collections.ObjectModel;
using static System.Net.WebRequestMethods;
using System.IO;
using System.Security.Cryptography;

namespace split_Wall
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class SplitWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            // Start a transaction
            using (Transaction trans = new Transaction(doc, "Create Walls"))
            {
                trans.Start();

                Reference pickedObj = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //Get the element associated with the reference
                Element elementWall = doc.GetElement(pickedObj);

                //Get the wallTypeId
                ElementId wallTypeId = elementWall.GetTypeId();

                //Get the levelId
                // Use ElementLevel filter to find elements by their associated level in the document

                // Find the level whose Name is "Level 1"
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> levels = collector.OfClass(typeof(Level)).ToElements();
                var query = from element in collector where element.Name == "Level 0" select element;// Linq query

                // Get the level id which will be used to match elements
                List<Element> level1 = query.ToList<Element>();
                ElementId levelId = level1[0].Id;

                //check if the element is not a wall
                if (elementWall.GetType().Equals(typeof(Wall)) == false)
                {
                    trans.RollBack();
                }

                Wall wall = elementWall as Wall;
                Options geomOptions = new Options { ComputeReferences = true, DetailLevel = ViewDetailLevel.Fine };
                GeometryElement geomEleWall = wall.get_Geometry(geomOptions);
                Reference pickedRoom = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element roomElement = doc.GetElement(pickedRoom);
                Room room = roomElement as Room;

                LocationCurve wallLocation = wall.Location as LocationCurve;
                Line line = wallLocation.Curve as Line;

                CurveArray walls = new CurveArray();

                XYZ startPoint = line.GetEndPoint(0);
                XYZ endPoint = line.GetEndPoint(1);
                XYZ splitPoint = null;
                //splits(commandData, doc, pickedObj, room, geomOptions, geomEleWall, out splitPoint);

                if (pickedObj != null)
                {
                    //create a boundingbox  
                    GeometryElement geoEle = room.get_Geometry(geomOptions);

                    foreach (GeometryObject geoO in geoEle)
                    {
                        Solid solid = geoO as Solid;
                        foreach (Edge roomWall in solid.Edges)
                        {
                            if ()
                            {
                                splitPoint = (roomWall.AsCurve()).GetEndPoint(1);
                                //TaskDialog.Show("splitPoint = ", splitPoint.ToString());
                                break;
                                //splitPoints.Append(splitPoint);
                            }
                        }
                    }
                    if (splitPoint == null)
                    {
                        TaskDialog.Show("Error", "splitPoint is null");
                        trans.RollBack();
                    }
                }
                else
                {
                    trans.RollBack();
                }

                TaskDialog.Show("Coordinate: {0} List: {1}", splitPoint.ToString());

                //XYZ splitPoint = splitPoints.FirstOrDefault();


                walls.Append(Line.CreateBound(startPoint, splitPoint));
                walls.Append(Line.CreateBound(splitPoint, endPoint));

                // Iterate through each curve in the CurveArray
                foreach (Curve curve in walls)
                {
                    // Create a wall for each curve
                    Wall splitwall = Wall.Create(doc, curve, wallTypeId, levelId, 10.0, 0.0, false, false);

                    // Set additional parameters for the wall if needed
                    //wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(ElementId.InvalidElementId); // Unconnected height
                }
                // nfvsiujdnfvujsedrfgbvujhsdbfgvujhsdfujhgvbnslujdhf
                trans.Commit();
            }
            return Result.Succeeded;
        }

        //public XYZ splits(ExternalCommandData commandData, Autodesk.Revit.DB.Document doc, Reference pickedObj, Room room,  Options geomOptions, GeometryElement geomEleWall, out XYZ splitPoint)
        //{
            

            //FilteredElementCollector collector = new FilteredElementCollector(doc);

            //if (pickedObj != null)
            //{
            //    //create a boundingbox  
            //    GeometryElement geoEle = room.get_Geometry(geomOptions);
                
            //    foreach (GeometryObject geoO in geoEle)
            //    {
            //        Solid solid = geoO as Solid;
            //        foreach (Edge roomWall in solid.Edges)
            //        {
            //            if (geomEleWall.Contains(roomWall))
            //            {
            //                splitPoint = (roomWall.AsCurve()).GetEndPoint(1);
            //                break;
            //                //splitPoints.Append(splitPoint);
            //            }
            //        }
            //        //TaskDialog.Show("Coordinate: {0} List: {1}", splitPoint.ToString(), splitPoints.ToString());
            //    }
            //}
            //return splitPoint;
        //}
    }
}      