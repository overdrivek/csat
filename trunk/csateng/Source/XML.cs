#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using System.Xml;
using OpenTK;

namespace CSatEng
{
    public static class XML
    {
        public static String GetAttrib(XmlElement XMLNode, String attrib)
        {
            return GetAttrib(XMLNode, attrib, "");
        }

        public static String GetAttrib(XmlElement XMLNode, String attrib, String defaultValue)
        {
            if (!string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
                return XMLNode.GetAttribute(attrib);
            else
                return defaultValue;
        }

        public static bool GetAttribBool(XmlElement XMLNode, String parameter)
        {
            return GetAttribBool(XMLNode, parameter, false);
        }

        public static bool GetAttribBool(XmlElement XMLNode, String attrib, bool defaultValue)
        {
            if (string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
                return defaultValue;

            if (XMLNode.GetAttribute(attrib) == "true")
                return true;

            return false;
        }

        public static float GetAttribReal(XmlElement XMLNode, String parameter)
        {
            return GetAttribReal(XMLNode, parameter, 0.0f);
        }

        public static float GetAttribReal(XmlElement XMLNode, String attrib, float defaultValue)
        {
            if (!string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
                return Util.GetFloat(XMLNode.GetAttribute(attrib));
            else
                return defaultValue;
        }

        public static Vector3 ParseColor(XmlElement XMLNode)
        {
            return new Vector3(
               Util.GetFloat(XMLNode.GetAttribute("r")),
               Util.GetFloat(XMLNode.GetAttribute("g")),
               Util.GetFloat(XMLNode.GetAttribute("b")));
        }

        public static Quaternion ParseQuaternion(XmlElement XMLNode)
        {
            Quaternion orientation = new Quaternion();
            orientation.X = Util.GetFloat(XMLNode.GetAttribute("x"));
            orientation.Y = Util.GetFloat(XMLNode.GetAttribute("y"));
            orientation.Z = Util.GetFloat(XMLNode.GetAttribute("z"));
            orientation.W = Util.GetFloat(XMLNode.GetAttribute("w"));
            return orientation;
        }
        public static Quaternion ParseOrientation(XmlElement XMLNode)
        {
            Quaternion orientation = new Quaternion();
            orientation.X = Util.GetFloat(XMLNode.GetAttribute("qx"));
            orientation.Y = Util.GetFloat(XMLNode.GetAttribute("qy"));
            orientation.Z = Util.GetFloat(XMLNode.GetAttribute("qz"));
            orientation.W = Util.GetFloat(XMLNode.GetAttribute("qw"));
            return orientation;
        }

        public static Vector3 ParseRotation(XmlElement XMLNode)
        {
            return new Vector3(
               Util.GetFloat(XMLNode.GetAttribute("qx")),
               Util.GetFloat(XMLNode.GetAttribute("qy")),
               Util.GetFloat(XMLNode.GetAttribute("qz"))
              );
        }

        public static Vector3 ParseVector3(XmlElement XMLNode)
        {
            return new Vector3(
               Util.GetFloat(XMLNode.GetAttribute("x")),
               Util.GetFloat(XMLNode.GetAttribute("y")),
               Util.GetFloat(XMLNode.GetAttribute("z"))
              );
        }

        public static Vector3 ParseFace(XmlElement XMLNode)
        {
            return new Vector3(
               Util.GetFloat(XMLNode.GetAttribute("v1")),
               Util.GetFloat(XMLNode.GetAttribute("v2")),
               Util.GetFloat(XMLNode.GetAttribute("v3")));
        }
        public static Vector2 ParseUV(XmlElement XMLNode)
        {
            return new Vector2(
               Util.GetFloat(XMLNode.GetAttribute("u")),
               Util.GetFloat(XMLNode.GetAttribute("v"))
              );
        }

    }
}
