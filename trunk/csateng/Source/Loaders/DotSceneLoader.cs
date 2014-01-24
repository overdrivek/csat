#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2008-2014 mjt
 * This notice may not be removed from any source distribution.
 * See csat-license.txt for licensing details.
 */
#endregion
// orig code: new ogre .scene loader, modified mjt, 2011

using System;
using System.Collections.Generic;
using System.Xml;
using OpenTK;

namespace CSatEng
{
    public class DotScene
    {
        public List<string> DynamicObjects;
        public List<string> StaticObjects;
        protected Node attachNode;
        static string dir = "";

        public DotScene(string fileName, Node root)
        {
            LoadDotScene(fileName, root);
        }

        public static DotScene Load(string fileName, Node root)
        {
            DotScene ds = new DotScene(fileName, root);
            return ds;
        }

        void LoadDotScene(string fileName, Node root)
        {
            this.StaticObjects = new List<string>();
            this.DynamicObjects = new List<string>();

            XmlDocument XMLDoc = null;
            XmlElement XMLRoot;
            dir = System.IO.Path.GetDirectoryName(fileName);
            if (dir.Length > 0) dir += "/";

            string fileNameWithoutExtension = fileName.Substring(0, fileName.LastIndexOf('.'));

            // ensin ladataan .material tiedosto jos löytyy
            string matFile = Settings.ModelDir + fileNameWithoutExtension + ".material";
            if (System.IO.File.Exists(matFile))
            {
                new Material(matFile);
            }

            // ladataan userdatat .scene.userdata.xml tiedostosta jos löytyy
            string userFile = Settings.ModelDir + fileName + ".userdata.xml";
            if (System.IO.File.Exists(userFile))
            {
                new UserData(userFile);
            }

            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.ModelDir + fileName))
                {
                    // tiedosto muistiin
                    string data = file.ReadToEnd();

                    XMLDoc = new XmlDocument();
                    XMLDoc.LoadXml(data);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            // Validate the File
            XMLRoot = XMLDoc.DocumentElement;
            if (XMLRoot.Name != "scene")
            {
                Log.Error("Error [" + fileName + "] Invalid .scene File. Missing <scene>");
            }

            // figure out where to attach any nodes we create
            attachNode = root;

            // Process the scene
            processScene(XMLRoot);
        }

        protected Camera processCamera(XmlElement XMLNode)
        {
            // Process attributes
            String name = XML.GetAttrib(XMLNode, "name");
            Camera pCamera = Camera.cam;

            float pFov = XML.GetAttribFloat(XMLNode, "fov", 45);
            Camera.Fov = pFov * MathExt.RadToDeg;

            XmlElement pElement;

            // Process normal 
            pElement = (XmlElement)XMLNode.SelectSingleNode("clipping");
            if (pElement != null)
            {
                // Blender
                float nearDist = XML.GetAttribFloat(pElement, "nearPlaneDist");
                if (nearDist == 0)
                {
                    // 3ds
                    nearDist = XML.GetAttribFloat(pElement, "near");
                }
                Camera.Near = nearDist;

                // Blender
                float farDist = XML.GetAttribFloat(pElement, "farPlaneDist");
                if (farDist == 0)
                {
                    // 3ds
                    farDist = XML.GetAttribFloat(pElement, "far");
                }
                Camera.Far = farDist;
            }
            return pCamera;
        }

        protected OgreMesh processEntity(XmlElement XMLNode)
        {
            // Process attributes
            String name = XML.GetAttrib(XMLNode, "name");
            String userDataID = XML.GetAttrib(XMLNode, "id");
            String meshFile = XML.GetAttrib(XMLNode, "meshFile") + ".xml";

            bool bstatic = XML.GetAttribBool(XMLNode, "static", false);
            if (bstatic)
                StaticObjects.Add(name);
            else
                DynamicObjects.Add(name);

            bool bvisible = XML.GetAttribBool(XMLNode, "visible", true);
            bool bcastshadows = XML.GetAttribBool(XMLNode, "castShadows", true);
            float brenderingDistance = XML.GetAttribFloat(XMLNode, "renderingDistance", 0);

            meshFile = Settings.ModelDir + dir + meshFile;

            OgreMesh mesh;
            try
            {
                mesh = new OgreMesh(name, meshFile);
                if (name.StartsWith("Path_")) return null; // pathia ei lisätä sceneen

                mesh.Visible = bvisible;
                mesh.CastShadow = bcastshadows;
                mesh.UserDataID = userDataID;

                XmlElement pElement;
                // Process subentities 
                pElement = (XmlElement)XMLNode.SelectSingleNode("subentities");
                if (pElement != null)
                {
                    pElement = (XmlElement)pElement.FirstChild;
                    while (pElement != null)
                    {
                        mesh.MaterialName = XML.GetAttrib(pElement, "materialName");
                        mesh.Material = Material.GetMaterial(mesh.MaterialName);

                        pElement = (XmlElement)pElement.NextSibling;
                    }
                }
                return mesh;
            }
            catch (Exception e)
            {
                Log.Error("Error loading " + meshFile + "\n" + e.Message);
            }

            return null;
        }

        protected Light processLight(XmlElement XMLNode)
        {
            // Process attributes
            String name = XML.GetAttrib(XMLNode, "name");

            // Create the light
            Light pLight = new Light(name);

            String sValue = XML.GetAttrib(XMLNode, "type");
            if (sValue == "point")
                pLight.Type = Light.LightTypes.Point;
            else if (sValue == "directional")
                pLight.Type = Light.LightTypes.Directional;
            else if (sValue == "spotLight")
                pLight.Type = Light.LightTypes.Spot;

            // Process colourDiffuse
            XmlElement pElement = (XmlElement)XMLNode.SelectSingleNode("colourDiffuse");
            if (pElement != null)
            {
                Vector3 v = XML.ParseColor(pElement);
                pLight.Diffuse = new Vector4(v.X, v.Y, v.Z, 1.0f);
            }
            // Process colourSpecular 
            pElement = (XmlElement)XMLNode.SelectSingleNode("colourSpecular");
            if (pElement != null)
            {
                Vector3 v = XML.ParseColor(pElement);
                pLight.Specular = new Vector4(v.X, v.Y, v.Z, 1.0f);
            }

            /*
            // Process lightRange 
            pElement = (XmlElement)XMLNode.SelectSingleNode("lightRange");
            if (pElement != null)
                processLightRange(pElement, pLight);

            // Process lightAttenuation 
            pElement = (XmlElement)XMLNode.SelectSingleNode("lightAttenuation");
            if (pElement != null)
                processLightAttenuation(pElement, pLight);
            */
            return pLight;
        }

        /*
        protected void processLightAttenuation(XmlElement XMLNode, Light pLight)
        {
            // Process attributes
            float range = XML.GetAttribReal(XMLNode, "range");
            float constant = XML.GetAttribReal(XMLNode, "constant");
            float linear = XML.GetAttribReal(XMLNode, "linear");
            float quadratic = XML.GetAttribReal(XMLNode, "quadratic");
            // Setup the light attenuation
            pLight.SetAttenuation(range, constant, linear, quadratic);
        }

        protected void processLightRange(XmlElement XMLNode, Light pLight)
        {
            // Process attributes
            float inner = XML.GetAttribReal(XMLNode, "inner");
            float outer = XML.GetAttribReal(XMLNode, "outer");
            float falloff = XML.GetAttribReal(XMLNode, "falloff", 1.0f);
            // Setup the light range
            pLight.SetSpotlightRange(new Radian((Degree)inner), new Radian((Degree)outer), falloff);
        }
        */

        protected void processNode(XmlElement XMLNode, Node pParent)
        {
            // Construct the node's name
            String name = XML.GetAttrib(XMLNode, "name");

            // Create the scene node
            Node pNode = new Node();
            Vector3 pos = Vector3.Zero, scale = Vector3.Zero;
            Quaternion orientation = new Quaternion();

            // Process other attributes
            XmlElement pElement;

            // Process position 
            pElement = (XmlElement)XMLNode.SelectSingleNode("position");
            if (pElement != null)
            {
                pos = XML.ParseVector3(pElement);
            }

            // Process rotation 
            pElement = (XmlElement)XMLNode.SelectSingleNode("rotation");
            if (pElement != null)
            {
                orientation = XML.ParseOrientation(pElement);
            }

            // Process scale 
            pElement = (XmlElement)XMLNode.SelectSingleNode("scale");
            if (pElement != null)
            {
                scale = XML.ParseVector3(pElement);
            }

            // Process ogremesh
            pElement = (XmlElement)XMLNode.SelectSingleNode("entity");
            if (pElement != null)
            {
                pNode = processEntity(pElement);
                if (pNode == null) return;

                // jos .scene tiedostossa näkyy että pathin position, rotation tai scale on muuttunut,
                // silloin path ei toimi oikein koska niitä ei tässä oteta huomioon ollenkaan 
                // (joten path voi ollaki ihan eri kohdassa missä pitäis).

                // ratkaisu on että pathia EI liikuteta, pyöritetä eikä skaalata 3dsmaxissa!
                // pivot point pitää olla origossa ja muokkaukset tehdään vain vertex edit moodissa.
            }

            // Process light 
            pElement = (XmlElement)XMLNode.SelectSingleNode("light");
            if (pElement != null)
            {
                pNode = processLight(pElement);
                pNode.Rotation = QuaternionExt.QuatToEuler(orientation);

                pElement = (XmlElement)XMLNode.NextSibling;
                if (pElement != null)
                {
                    string nname = XML.GetAttrib(pElement, "name");
                    if (nname.Contains(".Target"))
                    {
                        Vector3 targetPos;
                        pElement = (XmlElement)pElement.SelectSingleNode("position");
                        if (pElement != null) targetPos = XML.ParseVector3(pElement);
                        else targetPos = Vector3.Zero;

                        pNode.OrigOrientationMatrix = Matrix4.LookAt(pos, targetPos, Vector3.UnitY);
                    }
                    pNode.Name = name;
                    pNode.Position = pos;
                    pNode.Scale = scale;
                    pParent.Add(pNode);
                    return;
                }

            }

            // Process camera 
            pElement = (XmlElement)XMLNode.SelectSingleNode("camera");
            if (pElement != null)
            {
                pNode = processCamera(pElement);
                pNode.Rotation = QuaternionExt.QuatToEuler(orientation);
            }

            // Process childnodes
            pElement = (XmlElement)XMLNode.SelectSingleNode("node");
            while (pElement != null)
            {
                processNode(pElement, pNode);
                pElement = (XmlElement)pElement.NextSibling;
            }

            pNode.Name = name;
            pNode.Position = pos;
            pNode.Scale = scale;
            Matrix4Ext.CreateFromQuaternion(ref orientation, out pNode.OrigOrientationMatrix);

            pParent.Add(pNode);
        }

        protected void processNodes(XmlElement XMLNode)
        {
            XmlElement pElement;

            // Process node 
            pElement = (XmlElement)XMLNode.SelectSingleNode("node");
            while (pElement != null)
            {
                processNode(pElement, attachNode);
                XmlNode nextNode = pElement.NextSibling;
                pElement = nextNode as XmlElement;
                while (pElement == null && nextNode != null)
                {
                    nextNode = nextNode.NextSibling;
                    pElement = nextNode as XmlElement;
                }
            }
        }

        protected void processScene(XmlElement XMLRoot)
        {
            XmlElement pElement;

            // Process nodes 
            pElement = (XmlElement)XMLRoot.SelectSingleNode("nodes");
            if (pElement != null)
                processNodes(pElement);
        }
    }

    public class UserData
    {
        // userDataID, user_data
        public static Dictionary<string, string> Data = new Dictionary<string, string>();

        public UserData(string fileName)
        {
            XmlDocument XMLDoc = null;
            XmlElement XMLRoot;
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(Settings.ModelDir + fileName))
                {
                    // tiedosto muistiin
                    string data = file.ReadToEnd();

                    XMLDoc = new XmlDocument();
                    XMLDoc.LoadXml(data);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            // Validate the File
            XMLRoot = XMLDoc.DocumentElement;
            if (XMLRoot.Name != "userData")
            {
                Log.Error("Error [" + fileName + "] Invalid .userdata File. Missing <userData>");
            }

            XmlElement pElement;
            pElement = (XmlElement)XMLRoot.SelectSingleNode("object");
            if (pElement != null)
            {
                string userDataID, userData;

                userDataID = XML.GetAttrib(pElement, "__id");
                userData = pElement.InnerText;

                if (userDataID.Length > 0 && userData != null && userData.Length > 0)
                    Data.Add(userDataID, userData);
            }
        }

        public void AddUserData(string userDataID, string userData)
        {
            Data.Add(userDataID, userData);
        }
    }
}
