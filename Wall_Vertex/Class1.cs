using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.IFC;
using System.Diagnostics;

namespace Wall_Vertex
{
    [Transaction(TransactionMode.Manual)]
    //class that filter the wall, create it and extract vertex from it
    public class Class1 : IExternalCommand

    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            ElementId wall_id = null;
            var view = uidoc.ActiveView;
            var height = 20;
            var offset = 0;
            var flip = false;
            var structural = false;
            Parameter parameter;
            Parameter parameter1;
            double width_wall;
            double length;
            Location fr;
            XYZ start;
            XYZ end;
            double wall_orientation_x;
            double wall_orientation_y;
            double wall_orientation_z;
            Face face_wall;
            Element et_wall;
            Wall wall_my;
            Edge ed;
            Reference rf;
            Curve curves;
            var no_of_curves = 0;

            List<XYZ> startpoints = new List<XYZ>();


            List<XYZ> endpoints = new List<XYZ>();
            List<XYZ> allPoints = new List<XYZ>();


            //filter through element to get desired wall and then create it
            FilteredElementCollector filter_wall = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Walls)
            .WhereElementIsElementType();

            foreach (Element element in filter_wall)
            {
                if (element.Name == "Generic - 200mm")
                {
                    wall_id = element.Id;
                    break;
                }
            }
            //pre-requisite of creating a wall
            //pre-requisite of creating a line
            //creating start and end point
            XYZ start_point = new XYZ(0, 0, 0);
            XYZ end_point = new XYZ(10, 0, 0);

            //Creating a curve
            Line Line = Line.CreateBound(start_point, end_point);

            //Creating a levelid
            Level level = view.GenLevel;
            ElementId level_id = level.Id;

            using (Transaction trans = new Transaction(doc, "Creating a specific wall"))
            {
                trans.Start();

                //create a wall
                wall_my = Wall.Create(doc, Line, wall_id, level_id, height, offset, flip, structural);

                trans.Commit();
            }

            //getting its height
            BuiltInParameter para = BuiltInParameter.WALL_USER_HEIGHT_PARAM;
            parameter = wall_my.get_Parameter(para);
            //parameter to write for getting height, try below lines if any
            //parameter.Definition.Name;
            //parameter.AsDouble();

            //getting the width
            width_wall = wall_my.Width;

            //getting the length
            length = wall_my.LookupParameter("Length").AsDouble();

            //getting all the vertex of the solid

            //coordinates = wall_my.Orientation;

            //getting start and end points
            Curve curve_wall = (wall_my.Location as LocationCurve).Curve;
            start = curve_wall.GetEndPoint(0);
            end = curve_wall.GetEndPoint(1);

            //reference of a wall

            //getting faces in wall
            //this didnt worked
            //selecting the wall
            Wall pickwall = wall_my as Wall;

            //getting side faces

            IList<Reference> sideFaces = HostObjectUtils.GetSideFaces(pickwall, ShellLayerType.Exterior);

            //accessing the side faces of the wall
            Face face = uidoc.Document.GetElement(sideFaces[0]).GetGeometryObjectFromReference(sideFaces[0]) as Face;
            double area = face.Area;

            //full wall orientation
            XYZ wall_orientation = wall_my.Orientation;

            //orientation of the wall
            wall_orientation_x = wall_my.Orientation.X;
            wall_orientation_y = wall_my.Orientation.Y;
            wall_orientation_z = wall_my.Orientation.Z;

            //getting vertex of wall
            //accessing the solid
            String faceInfo = "";
            Options opt = new Options();
            GeometryElement geomElem = wall_my.get_Geometry(opt);
            foreach (GeometryObject geomObj in geomElem)
            {
                Solid geomSolid = geomObj as Solid;
                if (null != geomSolid)
                {
                    int faces = 0;
                    double totalArea = 0;
                    foreach (Face geomFace in geomSolid.Faces)
                    {
                        faces++;
                        faceInfo += "Face " + faces + " area: " + geomFace.Area.ToString() + "\n";
                        totalArea += geomFace.Area;
                    }
                    faceInfo += "Number of faces: " + faces + "\n";
                    faceInfo += "Total area: " + totalArea.ToString() + "\n";

                    foreach (Edge geomEdge in geomSolid.Edges)
                    {
                        if (geomEdge != null)
                        {
                            ed = geomEdge;

                            curves = ed.AsCurve() as Line;
                            no_of_curves++;
                            start = curves.GetEndPoint(0);
                            end = curves.GetEndPoint(1);
                            startpoints.Add(start);
                            endpoints.Add(end);
                        }

                        // get wall's geometry edges 
                    }
                    //combining the end and start lists and distinct will clear out the duplicate
                }
            }
            allPoints.AddRange(startpoints);
            allPoints.AddRange(endpoints);
            allPoints.Distinct().ToList();

            //Debug.Print("All Points");
            //int i = 0;
            //foreach (XYZ xyz in allPoints)
            //{
            //    i++;
            //    Debug.Print(i.ToString() + " - " + xyz.ToString());
            //}
            List<XYZ> lstxyzPointsDistinct = GetDistinctPoints(allPoints);
            //Debug.Print("Distinct Points");
            int i = 0;
            foreach (XYZ xyz in lstxyzPointsDistinct)
            {
                i++;
                TaskDialog.Show("Vertices", i.ToString() + " - " + xyz.ToString());
            }

            //List<XYZ> points = new List<XYZ>();
            //points = allPoints.Union(points).ToList();


            //var noDupes = new HashSet<XYZ>(allPoints);
            //allPoints.Clear();
            //allPoints.AddRange(noDupes);

            //// Create a HashSet and add all the numbers to it
            //List<XYZ> uniqueNumbers = new List<XYZ>();
            //foreach (XYZ number in startpoints)
            //{
            //    uniqueNumbers.Add(number);
            //}

            //// Print out the unique numbers
            //foreach (XYZ number in uniqueNumbers)
            //{
            //    TaskDialog.Show("Prompt", number.ToString());

            //}

            //XYZ a;
            //foreach (XYZ x in startpoints)
            //{
            //    a = x;
            //    TaskDialog.Show("Prompt", a.ToString());
            //}


            TaskDialog.Show("Notificaton", $"The Wall created is has height : {parameter.AsDouble()} \n,Width is : {width_wall}\n," +
            $" length is : {length}\n, Starting point of the wall is : {start}\n, Orientation of x: {wall_orientation_x}, \n Orientation of y: {wall_orientation_y}, Orientation of z:{wall_orientation_z} \n, Start points are : {startpoints}, End points are : {endpoints}, Unique Vertex are: {lstxyzPointsDistinct}", TaskDialogCommonButtons.Ok);

            return Result.Succeeded;
        }

        private List<XYZ> GetDistinctPoints(List<XYZ> lstxyz)
        {
            List<XYZ> lstxyzRound = new List<XYZ>();
            int i = 1, j = 1;

            //round the points to 6 decimal places (we may not need this)
            foreach (XYZ xyz in lstxyz)
            {
                lstxyzRound.Add(new XYZ(Math.Round(xyz.X, 6), Math.Round(xyz.Y, 6), Math.Round(xyz.Z, 6)));
            }

            //order by Z,X,Y (depends on your need)
            lstxyzRound = lstxyzRound.OrderBy(p => p.Y).ToList();
            lstxyzRound = lstxyzRound.OrderBy(p => p.X).ToList();
            lstxyzRound = lstxyzRound.OrderBy(p => p.Z).ToList();

            //remove points from list if duplicates
            bool blnDuplicate = true;
            while (blnDuplicate)
            {
                 blnDuplicate = false;
                for (i = j; i < lstxyzRound.Count; i++)
                {
                    if (lstxyzRound[i - 1].DistanceTo(lstxyzRound[i]) < 0.0001)
                    {
                        blnDuplicate = true;
                        j = i;
                        break;
                    }
                }
                if (blnDuplicate)
                {
                    lstxyzRound.RemoveAt(j);
                }
            }
            return lstxyzRound;
        }
    }
}