
using UnityEngine;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System;

namespace WM
{
    [Serializable]
    [XmlRoot("TrackedObject")]
    public class TrackedObject
    {
        //! Read a double from XML.
        //static public void FromXML
        //(out double v
        //, ref XPathNavigator nav
        //, string vectorElementPath)
        //{
        //    string xs = (nav.SelectSingleNode(vectorElementPath).Value);

        //    v = WM.Utils.XML.ConvertToFloat(xs);
        //}

        //! Read a Vector3 from XML.
        //static public void FromXML
        //(ref Vector3 v
        //, ref XPathNavigator nav
        //, string vectorElementPath)
        //{
        //    string xs = (nav.SelectSingleNode(vectorElementPath + "/X").Value);
        //    string ys = (nav.SelectSingleNode(vectorElementPath + "/Y").Value);
        //    string zs = (nav.SelectSingleNode(vectorElementPath + "/Z").Value);

        //    v.x = WM.Utils.XML.ConvertToFloat(xs);
        //    v.y = WM.Utils.XML.ConvertToFloat(ys);
        //    v.z = WM.Utils.XML.ConvertToFloat(zs);
        //}

        [XmlElement("Name")]
        public string Name { get; set; } = "";

        [XmlElement("Position")]
        public Vector3 Position { get; set; } = new Vector3();

        [XmlElement("Rotation")]
        public Quaternion Rotation { get; set; } = new Quaternion();

        /*
        [XmlElement("RotationAxis")]
        public Vector3 RotationAxis { get; set; } = new Vector3();

        [XmlElement("RotationAngle")]
        public double RotationAngle { get; set; } = 0.0;

        [XmlElement("RotationEuler")]
        public Vector3 RotationEuler { get; set; } = new Vector3();

        [XmlElement("AxisX")]
        public Vector3 AxisX { get; set; } = new Vector3();

        [XmlElement("AxisY")]
        public Vector3 AxisY { get; set; } = new Vector3();

        [XmlElement("AxisZ")]
        public Vector3 AxisZ { get; set; } = new Vector3();

        public override string ToString()
        {
            return
            "Name: " + Name + "\n" +
            "\n" +
            "Position:\n" +
            "X: " + Position.x + "\n" +
            "Y: " + Position.y + "\n" +
            "Z: " + Position.z + "\n" +
            "\n" +
            
            //"Rotation Euler Angles:\n" +
            //"X: " + m_rotationEuler.x + "\n" +
            //"Y: " + m_rotationEuler.y + "\n" +
            //"Z: " + m_rotationEuler.z + "\n" +
            //"\n" +
            //"Rotation Local Axes:\n" +
            //"X axis:\n" +
            //"X: " + m_axisX.x + "\n" +
            //"Y: " + m_axisX.y + "\n" +
            //"Z: " + m_axisX.z + "\n" +
            //"\n" +
            //"Y axis:\n" +
            //"X: " + m_axisY.x + "\n" +
            //"Y: " + m_axisY.y + "\n" +
            //"Z: " + m_axisY.z + "\n" +
            //"\n" +
            //"Z axis:\n" +
            //"X: " + m_axisZ.x + "\n" +
            //"Y: " + m_axisZ.y + "\n" +
            //"Z: " + m_axisZ.z +
            //"\n" +
            
            "Rotation AxisAngle:\n" +
            "X: " + RotationAxis.x + "\n" +
            "Y: " + RotationAxis.y + "\n" +
            "Z: " + RotationAxis.z + "\n" +
            "Angle:" + RotationAngle;
        }
    */
    }
} // WM