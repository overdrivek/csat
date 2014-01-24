#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
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

        public static bool GetAttribBool(XmlElement XMLNode, String attrib)
        {
            return GetAttribBool(XMLNode, attrib, false);
        }

        public static bool GetAttribBool(XmlElement XMLNode, String attrib, bool defaultValue)
        {
            if (string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
                return defaultValue;

            if (XMLNode.GetAttribute(attrib) == "true")
                return true;

            return false;
        }

        public static float GetAttribFloat(XmlElement XMLNode, String attrib)
        {
            return GetAttribFloat(XMLNode, attrib, 0.0f);
        }

        public static float GetAttribFloat(XmlElement XMLNode, String attrib, float defaultValue)
        {
            if (!string.IsNullOrEmpty(XMLNode.GetAttribute(attrib)))
                return MathExt.GetFloat(XMLNode.GetAttribute(attrib));
            else
                return defaultValue;
        }

        public static Vector3 ParseColor(XmlElement XMLNode)
        {
            return new Vector3(
               MathExt.GetFloat(XMLNode.GetAttribute("r")),
               MathExt.GetFloat(XMLNode.GetAttribute("g")),
               MathExt.GetFloat(XMLNode.GetAttribute("b")));
        }

        public static Quaternion ParseQuaternion(XmlElement XMLNode)
        {
            Quaternion orientation = new Quaternion();
            orientation.X = MathExt.GetFloat(XMLNode.GetAttribute("x"));
            orientation.Y = MathExt.GetFloat(XMLNode.GetAttribute("y"));
            orientation.Z = MathExt.GetFloat(XMLNode.GetAttribute("z"));
            orientation.W = MathExt.GetFloat(XMLNode.GetAttribute("w"));
            return orientation;
        }

        public static Quaternion ParseOrientation(XmlElement XMLNode)
        {
            Quaternion orientation = new Quaternion();
            orientation.X = MathExt.GetFloat(XMLNode.GetAttribute("qx"));
            orientation.Y = MathExt.GetFloat(XMLNode.GetAttribute("qy"));
            orientation.Z = MathExt.GetFloat(XMLNode.GetAttribute("qz"));
            orientation.W = MathExt.GetFloat(XMLNode.GetAttribute("qw"));
            return orientation;
        }

        public static Vector3 ParseRotation(XmlElement XMLNode)
        {
            return new Vector3(
               MathExt.GetFloat(XMLNode.GetAttribute("qx")),
               MathExt.GetFloat(XMLNode.GetAttribute("qy")),
               MathExt.GetFloat(XMLNode.GetAttribute("qz"))
              );
        }

        public static Vector3 ParseVector3(XmlElement XMLNode)
        {
            return new Vector3(
               MathExt.GetFloat(XMLNode.GetAttribute("x")),
               MathExt.GetFloat(XMLNode.GetAttribute("y")),
               MathExt.GetFloat(XMLNode.GetAttribute("z"))
              );
        }

        public static Vector3 ParseFace(XmlElement XMLNode)
        {
            return new Vector3(
               MathExt.GetFloat(XMLNode.GetAttribute("v1")),
               MathExt.GetFloat(XMLNode.GetAttribute("v2")),
               MathExt.GetFloat(XMLNode.GetAttribute("v3")));
        }

        public static Vector2 ParseUV(XmlElement XMLNode)
        {
            return new Vector2(
               MathExt.GetFloat(XMLNode.GetAttribute("u")),
               MathExt.GetFloat(XMLNode.GetAttribute("v"))
              );
        }
    }
}
