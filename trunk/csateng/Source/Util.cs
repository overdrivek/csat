#region --- MIT License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2011 mjt[matola@sci.fi]
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace CSatEng
{
    public static class Util
    {
        /// <summary>
        /// palauttaa str:stä float luvun. jos pisteen kanssa ei onnistu, kokeillaan pilkun kanssa.
        /// </summary>
        public static float GetFloat(string str)
        {
            float n;
            if (float.TryParse(str, out n) == true)
            {
                return n;
            }
            str = str.Replace('.', ','); // pisteet pilkuiksi
            if (float.TryParse(str, out n) == true)
            {
                return n;
            }
            Util.Error("GetFloat failed: " + str);
            return 0;
        }

        public static void CheckGLError(string methodName)
        {
            GL.Finish();
            ErrorCode error = error = GL.GetError();
            if (error != ErrorCode.NoError) Util.Error("Error " + error + " in " + methodName);
        }

        public static void Error(string str)
        {
            Log.WriteLine(str);
            throw new Exception(str);
        }
    }
}
